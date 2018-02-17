using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class NodeAcceptSourceVisitorPartialGenerator : NodePartialGeneratorBase
    {
        protected NodeAcceptSourceVisitorPartialGenerator(CoreDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        public static TypeDeclarationSyntax Generate(CoreDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new NodeAcceptSourceVisitorPartialGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        protected override IEnumerable<MemberDeclarationSyntax> GenerateMembers()
        {
            if (IsAbstract)
            {
                yield break;
            }
            yield return AcceptMethod();
            yield return AcceptGenericMethod();
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
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(visitor),
                                IdentifierName(Names.Visit + Descriptor.RawModelName)))
                        .AddArgumentListArguments(
                            Argument(
                                ThisExpression()))));
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
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(visitor),
                                IdentifierName(Names.Visit + Descriptor.RawModelName)))
                        .AddArgumentListArguments(
                            Argument(
                                ThisExpression()))));
        }
    }
}
