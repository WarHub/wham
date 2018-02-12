using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class BasicDeclarationNodeGenerator : NodePartialGeneratorBase
    {
        protected BasicDeclarationNodeGenerator(CoreDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        public static TypeDeclarationSyntax Generate(CoreDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new BasicDeclarationNodeGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        protected override SyntaxTokenList GenerateModifiers()
        {
            return
                TokenList(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.PartialKeyword));
        }

        protected override IEnumerable<BaseTypeSyntax> GenerateBaseTypes()
        {
            var baseName = IsDerived
                ? BaseType.Name.GetNodeTypeNameCore()
                : Names.SourceNode;

            yield return SimpleBaseType(
                        IdentifierName(baseName));
        }
    }
}
