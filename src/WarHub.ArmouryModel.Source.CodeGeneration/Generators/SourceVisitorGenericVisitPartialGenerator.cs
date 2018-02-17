using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class SourceVisitorGenericVisitPartialGenerator : PartialGeneratorBase
    {
        protected SourceVisitorGenericVisitPartialGenerator(CoreDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        public static TypeDeclarationSyntax Generate(CoreDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new SourceVisitorGenericVisitPartialGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        protected override SyntaxToken GenerateTypeIdentifier() => Identifier(Names.SourceVisitor);

        protected override IEnumerable<TypeParameterSyntax> GenerateTypeParameters()
        {
            yield return TypeParameter(Names.SourceVisitorTypeParameter);
        }

        protected override IEnumerable<MemberDeclarationSyntax> GenerateMembers()
        {
            const string node = "node";
            yield return
                MethodDeclaration(
                    IdentifierName(Names.SourceVisitorTypeParameter),
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
                    ReturnStatement(
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
