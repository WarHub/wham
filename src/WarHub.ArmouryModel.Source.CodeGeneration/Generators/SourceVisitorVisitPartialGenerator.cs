using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class SourceVisitorVisitPartialGenerator : PartialGeneratorBase
    {
        protected SourceVisitorVisitPartialGenerator(CoreDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        public static TypeDeclarationSyntax Generate(CoreDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new SourceVisitorVisitPartialGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        protected override SyntaxToken GenerateTypeIdentifier() => Identifier(Names.SourceVisitor);

        protected override IEnumerable<MemberDeclarationSyntax> GenerateMembers()
        {
            const string node = "node";
            yield return
                MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.VoidKeyword)),
                    Names.Visit + Descriptor.RawModelName)
                .AddModifiers(
                    SyntaxKind.PublicKeyword,
                    SyntaxKind.VirtualKeyword)
                .AddParameterListParameters(
                    Parameter(
                        Identifier(node))
                    .WithType(
                        Descriptor.GetNodeTypeIdentifierName()))
                .AddBodyStatements(
                    ExpressionStatement(
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                ThisExpression(),
                                IdentifierName(Names.DefaultVisit)))
                        .AddArgumentListArguments(
                            Argument(
                                IdentifierName(node)))));
        }
    }
}
