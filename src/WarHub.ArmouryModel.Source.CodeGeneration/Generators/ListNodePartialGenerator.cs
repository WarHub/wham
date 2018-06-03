using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class ListNodePartialGenerator : PartialGeneratorBase
    {
        protected ListNodePartialGenerator(CoreDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        public static TypeDeclarationSyntax Generate(CoreDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new ListNodePartialGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        protected override SyntaxTokenList GenerateModifiers()
        {
            return
                TokenList(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.PartialKeyword));
        }

        protected override SyntaxToken GenerateTypeIdentifier()
        {
            return Identifier(Descriptor.GetListNodeTypeName());
        }

        protected override IEnumerable<BaseTypeSyntax> GenerateBaseTypes()
        {
            yield return
                SimpleBaseType(
                    GenericName(Names.ListNode)
                    .AddTypeArgumentListArguments(
                        Descriptor.GetNodeTypeIdentifierName()));
        }

        protected override IEnumerable<MemberDeclarationSyntax> GenerateMembers()
        {
            if (IsAbstract)
            {
                yield break;
            }
            yield return GenerateConstructor();
            yield return CreateKindProperty();
            yield return AcceptMethod();
            yield return AcceptGenericMethod();
        }

        private ConstructorDeclarationSyntax GenerateConstructor()
        {
            const string listLocal = "list";
            const string parentLocal = "parent";
            return
                ConstructorDeclaration(
                    Descriptor.GetListNodeTypeName())
                .AddModifiers(SyntaxKind.ProtectedKeyword, SyntaxKind.InternalKeyword)
                .AddParameterListParameters(
                    Parameter(
                        Identifier(listLocal))
                    .WithType(Descriptor.GetNodeTypeIdentifierName().ToNodeListType()),
                    Parameter(
                        Identifier(parentLocal))
                    .WithType(
                        IdentifierName(Names.SourceNode)))
                .WithInitializer(
                    ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                    .AddArgumentListArguments(
                        Argument(
                            IdentifierName(listLocal)),
                        Argument(
                            IdentifierName(parentLocal))))
                .AddBodyStatements();
        }

        private PropertyDeclarationSyntax CreateKindProperty()
        {
            var kindString = Descriptor.RawModelName + Names.ListSuffix;
            return
                PropertyDeclaration(
                    IdentifierName(Names.SourceKind),
                    Names.Kind)
                .AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.OverrideKeyword)
                .WithExpressionBodyFull(
                    IdentifierName(Names.SourceKind)
                    .MemberAccess(
                        IdentifierName(kindString)));
        }

        private MemberDeclarationSyntax AcceptMethod()
        {
            const string visitor = "visitor";
            return
                MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.VoidKeyword)),
                    Names.Accept)
                .AddModifiers(
                    SyntaxKind.PublicKeyword,
                    SyntaxKind.OverrideKeyword)
                .AddParameterListParameters(
                    Parameter(
                        Identifier(visitor))
                    .WithType(
                        IdentifierName(Names.SourceVisitor)))
                .AddBodyStatements(
                    ExpressionStatement(
                        IdentifierName(visitor)
                        .MemberAccess(
                            IdentifierName(Names.Visit + Descriptor.RawModelName + Names.ListSuffix))
                        .InvokeWithArguments(
                            ThisExpression())));
        }

        private MemberDeclarationSyntax AcceptGenericMethod()
        {
            const string visitor = "visitor";
            return
                MethodDeclaration(
                    IdentifierName(Names.SourceVisitorTypeParameter),
                    Names.Accept)
                .AddTypeParameterListParameters(
                    TypeParameter(Names.SourceVisitorTypeParameter))
                .AddModifiers(
                    SyntaxKind.PublicKeyword,
                    SyntaxKind.OverrideKeyword)
                .AddParameterListParameters(
                    Parameter(
                        Identifier(visitor))
                    .WithType(
                        GenericName(Names.SourceVisitor)
                        .AddTypeArgumentListArguments(
                            IdentifierName(Names.SourceVisitorTypeParameter))))
                .AddBodyStatements(
                    ReturnStatement(
                        IdentifierName(visitor)
                        .MemberAccess(
                            IdentifierName(Names.Visit + Descriptor.RawModelName + Names.ListSuffix))
                        .InvokeWithArguments(
                            ThisExpression())));
        }
    }
}
