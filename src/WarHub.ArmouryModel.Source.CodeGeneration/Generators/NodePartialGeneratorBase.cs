using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal abstract class NodePartialGeneratorBase : PartialGeneratorBase
    {
        protected NodePartialGeneratorBase(CoreDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        protected override SyntaxToken GenerateTypeIdentifier()
        {
            return Identifier(Descriptor.GetNodeTypeName());
        }

        protected override IEnumerable<TypeParameterSyntax> GenerateTypeParameters()
        {
            return
                Descriptor
                .CoreType
                .DescendantNodesAndSelf()
                .OfType<GenericNameSyntax>()
                .FirstOrDefault()
                ?.TypeArgumentList
                ?.Arguments
                .Cast<IdentifierNameSyntax>()
                .Select(x => TypeParameter(x.Identifier))
                ?? Enumerable.Empty<TypeParameterSyntax>();
        }
    }
}
