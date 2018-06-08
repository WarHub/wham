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
            yield return CreateElementKindProperty();
            yield return CreateNodeListProperty();
            yield return AcceptMethod();
            yield return AcceptGenericMethod();
            yield return CreateWithNodesMethod();
        }

        private ConstructorDeclarationSyntax GenerateConstructor()
        {
            const string cores = "cores";
            const string parent = "parent";
            return
                ConstructorDeclaration(
                    Descriptor.GetListNodeTypeName())
                .AddModifiers(SyntaxKind.ProtectedKeyword, SyntaxKind.InternalKeyword)
                .AddParameterListParameters(
                    Parameter(
                        Identifier(cores))
                    .WithType(Descriptor.CoreType.ToImmutableArrayType()),
                    Parameter(
                        Identifier(parent))
                    .WithType(
                        IdentifierName(Names.SourceNode)))
                .WithInitializer(
                    ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                    .AddArgumentListArguments(
                        Argument(
                            IdentifierName(parent))))
                .AddBodyStatements(
                    IdentifierName(cores)
                        .MemberAccess(
                            IdentifierName(Names.ToNodeList))
                        .InvokeWithArguments(
                            ThisExpression())
                        .AssignTo(
                            IdentifierName(Names.NodeList))
                        .AsStatement());
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

        private PropertyDeclarationSyntax CreateElementKindProperty()
        {
            var kindString = Descriptor.RawModelName;
            return
                PropertyDeclaration(
                    IdentifierName(Names.SourceKind),
                    Names.ElementKind)
                .AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.OverrideKeyword)
                .WithExpressionBodyFull(
                    IdentifierName(Names.SourceKind)
                    .MemberAccess(
                        IdentifierName(kindString)));
        }

        private PropertyDeclarationSyntax CreateNodeListProperty()
        {
            return
                PropertyDeclaration(
                        GenericName(Names.NodeList)
                            .AddTypeArgumentListArguments(
                                Descriptor.GetNodeTypeIdentifierName()),
                        Names.NodeList)
                    .AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.OverrideKeyword)
                    .AddAccessorListAccessors(
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonTokenDefault());
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

        private MemberDeclarationSyntax CreateWithNodesMethod()
        {
            const string nodes = "nodes";
            return
                MethodDeclaration(
                        GenericName(Names.ListNode)
                            .AddTypeArgumentListArguments(
                                Descriptor.GetNodeTypeIdentifierName()),
                        Names.WithNodes)
                    .AddModifiers(
                        SyntaxKind.PublicKeyword,
                        SyntaxKind.OverrideKeyword)
                    .AddParameterListParameters(
                        Parameter(
                                Identifier(nodes))
                            .WithType(
                                Descriptor.GetNodeTypeIdentifierName().ToNodeListType()))
                    .AddBodyStatements(
                        ReturnStatement(
                            IdentifierName(nodes)
                                .MemberAccess(
                                    IdentifierName(Names.ToListNode))
                                .InvokeWithArguments()));
        }
    }
}
