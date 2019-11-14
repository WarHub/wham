using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MoreLinq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class RecordCorePartialGenerator : CorePartialGeneratorBase
    {
        public static readonly SyntaxToken ValueParamToken = Identifier("value");
        public static readonly IdentifierNameSyntax ValueParamSyntax = IdentifierName(ValueParamToken);

        protected RecordCorePartialGenerator(CoreDescriptor descriptor, CancellationToken cancellationToken)
            : base(descriptor, cancellationToken)
        {
        }

        public static TypeDeclarationSyntax Generate(CoreDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new RecordCorePartialGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        protected override IEnumerable<MemberDeclarationSyntax> GenerateMembers()
        {
            return
                SingletonList<MemberDeclarationSyntax>(
                    GenerateConstructor())
                .AddRange(
                    GenerateUpdateMethods())
                .AddRange(
                    GenerateMutators());
        }

        private ConstructorDeclarationSyntax GenerateConstructor()
        {
            return
                ConstructorDeclaration(Descriptor.CoreTypeIdentifier)
                .AddModifiers(SyntaxKind.PublicKeyword)
                .AddParameterListParameters(
                    Descriptor.Entries.Select(entry => entry.CamelCaseParameterSyntax))
                .MutateIf(
                    IsDerived,
                    x => x
                    .WithInitializer(
                        ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                        .AddArgumentListArguments(
                            Descriptor.DerivedEntries.Select(entry => entry.CamelCaseArgumentSyntax))))
                .AddBodyStatements(
                    Descriptor.DeclaredEntries.Select(CreateCtorAssignment));

            static StatementSyntax CreateCtorAssignment(CoreDescriptor.Entry entry)
            {
                return
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            ThisExpression()
                            .MemberAccess(entry.IdentifierName),
                            entry.CamelCaseIdentifierName));
            }
        }

        private IEnumerable<MethodDeclarationSyntax> GenerateUpdateMethods()
        {
            var abstractBaseUpdateName = Names.Update + Descriptor.CoreTypeIdentifier.Text;
            yield return
                MethodDeclaration(Descriptor.CoreType, Names.Update)
                .AddModifiers(SyntaxKind.PublicKeyword)
                .MutateIf(
                    IsDerived && Descriptor.Entries.Length == Descriptor.DerivedEntries.Length,
                    x => x.AddModifiers(SyntaxKind.NewKeyword))
                .AddParameterListParameters(
                    Descriptor.Entries.Select(entry => entry.CamelCaseParameterSyntax))
                .AddBodyStatements(
                        UpdateStatements());
            if (IsAbstract)
            {
                yield return
                    MethodDeclaration(Descriptor.CoreType, abstractBaseUpdateName)
                    .AddModifiers(SyntaxKind.ProtectedKeyword, SyntaxKind.AbstractKeyword)
                    .AddParameterListParameters(
                        Descriptor.Entries.Select(entry => entry.CamelCaseParameterSyntax))
                    .WithSemicolonTokenDefault();
            }
            if (IsDerived)
            {
                var baseTypeName = Descriptor.TypeSymbol.BaseType.Name;
                yield return
                    MethodDeclaration(
                        IdentifierName(baseTypeName),
                        Names.Update + baseTypeName)
                    .AddModifiers(
                        SyntaxKind.ProtectedKeyword,
                        SyntaxKind.SealedKeyword,
                        SyntaxKind.OverrideKeyword)
                    .AddParameterListParameters(
                        Descriptor.DerivedEntries.Select(entry => entry.CamelCaseParameterSyntax))
                    .AddBodyStatements(
                        ReturnStatement(
                            IdentifierName(Names.Update)
                            .InvokeWithArguments(
                                Descriptor.DerivedEntries
                                .Select(entry => entry.CamelCaseIdentifierName)
                                .Concat(
                                    Descriptor.DeclaredEntries
                                    .Select(entry => entry.IdentifierName)))));
            }
            IEnumerable<StatementSyntax> UpdateStatements()
            {
                if (IsAbstract)
                {
                    yield return
                        ReturnStatement(
                            IdentifierName(abstractBaseUpdateName)
                            .InvokeWithArguments(
                                Descriptor.Entries.Select(entry => entry.CamelCaseIdentifierName)));
                    yield break;
                }
                const string EqualVarName = "equal";
                yield return
                    LocalDeclarationStatement(
                        VariableDeclaration(
                            IdentifierName("var"))
                        .AddVariables(
                            VariableDeclarator(EqualVarName)
                            .WithInitializer(
                                EqualsValueClause(
                                    EqualsInitializer()))));
                yield return
                    ReturnStatement(
                        ConditionalExpression(
                            IdentifierName(EqualVarName),
                            ThisExpression(),
                            ObjectCreationExpression(Descriptor.CoreType)
                            .WithArguments(
                                Descriptor.Entries.Select(x => x.CamelCaseIdentifierName))));
                ExpressionSyntax EqualsInitializer() =>
                    (from entry in Descriptor.Entries
                     let idName = entry.IdentifierName
                     let param = entry.CamelCaseIdentifierName
                     let thisAccess =
                        ThisExpression()
                        .MemberAccess(idName)
                     select BinaryExpression(SyntaxKind.EqualsExpression, thisAccess, param))
                    .Aggregate((x, y) => BinaryExpression(SyntaxKind.LogicalAndExpression, x, y));
            }
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateMutators()
        {
            return Descriptor.Entries.Select(CreateRecordMutator);
            MethodDeclarationSyntax CreateRecordMutator(CoreDescriptor.Entry entry)
            {
                var arguments =
                    Descriptor.Entries
                    .Select(x => x == entry ? ValueParamSyntax : x.IdentifierName);
                return
                    MethodDeclaration(
                        Descriptor.CoreType,
                        GetMutatorIdentifier())
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .MutateIf(
                        IsDerived && entry.Symbol.ContainingType != Descriptor.TypeSymbol,
                        x => x.AddModifiers(SyntaxKind.NewKeyword))
                    .AddParameterListParameters(
                        Parameter(ValueParamToken)
                        .WithType(entry.Type))
                    .AddBodyStatements(
                        ReturnStatement(
                            ConditionalExpression(
                                BinaryExpression(
                                    SyntaxKind.EqualsExpression,
                                    ThisExpression()
                                    .MemberAccess(entry.IdentifierName),
                                    ValueParamSyntax),
                                ThisExpression(),
                                IdentifierName(Names.Update)
                                .InvokeWithArguments(arguments))));

                SyntaxToken GetMutatorIdentifier() =>
                    Identifier(Names.WithPrefix + entry.Identifier.ValueText);
            }
        }

        private ParameterSyntax CreateParameter(CoreDescriptor.Entry property)
        {
            return
                Parameter(property.Identifier)
                .WithType(property.Type);
        }
    }
}
