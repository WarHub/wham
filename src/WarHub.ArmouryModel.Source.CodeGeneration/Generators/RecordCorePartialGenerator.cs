using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class RecordCorePartialGenerator : CorePartialGeneratorBase
    {
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
                    Descriptor.Entries.Select(CreateParameter))
                .MutateIf(
                    IsDerived,
                    x => x
                    .WithInitializer(
                        ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                        .AddArgumentListArguments(
                            Descriptor.DerivedEntries.Select(
                                entry => Argument(entry.IdentifierName)))))
                .AddBodyStatements(
                    Descriptor.DeclaredEntries.Select(CreateCtorAssignment));
            StatementSyntax CreateCtorAssignment(CoreDescriptor.Entry entry)
            {
                return
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                ThisExpression(),
                                entry.IdentifierName),
                            entry.IdentifierName));
            }
        }

        private IEnumerable<MethodDeclarationSyntax> GenerateUpdateMethods()
        {
            var abstractBaseUpdateName = Names.Update + Descriptor.CoreTypeIdentifier.Text;
            var arguments = Descriptor.Entries
                .Select(x =>
                {
                    return Argument(x.IdentifierName);
                });
            var parameters = Descriptor.Entries
                .Select(CreateParameter)
                .ToImmutableArray();
            yield return
                MethodDeclaration(Descriptor.CoreType, Names.Update)
                .AddModifiers(SyntaxKind.PublicKeyword)
                .MutateIf(
                    IsDerived && Descriptor.Entries.Length == Descriptor.DerivedEntries.Length, 
                    x => x.AddModifiers(SyntaxKind.NewKeyword))
                .AddParameterListParameters(parameters)
                .AddBodyStatements(
                    UpdateBodyStatements());
            if (IsAbstract)
            {
                yield return
                    MethodDeclaration(Descriptor.CoreType, abstractBaseUpdateName)
                    .AddModifiers(SyntaxKind.ProtectedKeyword, SyntaxKind.AbstractKeyword)
                    .AddParameterListParameters(parameters)
                    .WithSemicolonTokenDefault();
            }
            if (IsDerived)
            {
                string baseTypeName = Descriptor.TypeSymbol.BaseType.Name;
                yield return
                    MethodDeclaration(
                        IdentifierName(baseTypeName),
                        Names.Update + baseTypeName)
                    .AddModifiers(SyntaxKind.ProtectedKeyword, SyntaxKind.SealedKeyword, SyntaxKind.OverrideKeyword)
                    .AddParameterListParameters(
                        Descriptor.DerivedEntries.Select(CreateParameter))
                    .AddBodyStatements(
                        ReturnStatement(
                            InvocationExpression(
                                IdentifierName(Names.Update))
                            .AddArgumentListArguments(arguments)));
            }
            IEnumerable<StatementSyntax> UpdateBodyStatements()
            {
                if (IsAbstract)
                {
                    yield return
                        ReturnStatement(
                            InvocationExpression(
                                IdentifierName(abstractBaseUpdateName))
                            .AddArgumentListArguments(arguments));
                }
                else
                {
                    yield return
                        ReturnStatement(
                            ObjectCreationExpression(Descriptor.CoreType)
                            .AddArgumentListArguments(arguments));
                }
            }
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateMutators()
        {
            return Descriptor.Entries.Select(CreateRecordMutator);
            MethodDeclarationSyntax CreateRecordMutator(CoreDescriptor.Entry entry)
            {
                var arguments = Descriptor.Entries.Select(x => Argument(x.IdentifierName));
                var mutator =
                    MethodDeclaration(
                        Descriptor.CoreType,
                        GetMutatorIdentifier())
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .MutateIf(
                        IsDerived && entry.Symbol.ContainingType != Descriptor.TypeSymbol,
                        x => x.AddModifiers(SyntaxKind.NewKeyword))
                    .AddParameterListParameters(
                        Parameter(entry.Identifier)
                        .WithType(entry.Type))
                    .AddBodyStatements(
                        ReturnStatement(
                            InvocationExpression(
                                IdentifierName(Names.Update))
                            .AddArgumentListArguments(arguments)));
                return mutator;

                SyntaxToken GetMutatorIdentifier()
                {
                    return Identifier($"{Names.WithPrefix}{entry.Identifier.ValueText}");
                }
            }
        }

        ParameterSyntax CreateParameter(CoreDescriptor.Entry property)
        {
            return 
                Parameter(property.Identifier)
                .WithType(property.Type);
        }
    }
}
