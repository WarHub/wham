using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal abstract class ModelExtensionsPartialGeneratorBase : PartialGeneratorBase
    {
        protected ModelExtensionsPartialGeneratorBase(CoreDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        protected override SyntaxTokenList GenerateModifiers()
        {
            return TokenList(
                Token(SyntaxKind.StaticKeyword),
                Token(SyntaxKind.PartialKeyword));
        }

        protected override SyntaxToken GenerateTypeIdentifier()
        {
            return Identifier(Names.ModelExtensions);
        }
    }
}
