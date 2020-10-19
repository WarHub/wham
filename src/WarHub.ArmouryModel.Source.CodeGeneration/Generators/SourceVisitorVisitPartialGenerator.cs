using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            const string Node = "node";
            yield return CreateVisitMethod(
                Names.Visit + Descriptor.RawModelName,
                Descriptor.GetNodeTypeIdentifierName());
            yield return CreateVisitMethod(
                Names.Visit + Descriptor.RawModelName + Names.ListSuffix,
                Descriptor.GetListNodeTypeIdentifierName());

            static MethodDeclarationSyntax CreateVisitMethod(string methodName, IdentifierNameSyntax type)
            {
                return
                    MethodDeclaration(
                        PredefinedType(Token(SyntaxKind.VoidKeyword)),
                        methodName)
                    .AddModifiers(
                        SyntaxKind.PublicKeyword,
                        SyntaxKind.VirtualKeyword)
                    .AddParameterListParameters(
                        Parameter(
                            Identifier(Node))
                        .WithType(type))
                    .AddBodyStatements(
                        ExpressionStatement(
                            ThisExpression()
                            .Dot(
                                IdentifierName(Names.DefaultVisit))
                            .Invoke(
                                IdentifierName(Node))));
            }
        }
    }
}
