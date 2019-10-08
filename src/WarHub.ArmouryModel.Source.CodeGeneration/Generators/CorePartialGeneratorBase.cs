using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal abstract class CorePartialGeneratorBase : PartialGeneratorBase
    {
        protected CorePartialGeneratorBase(CoreDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        protected override SyntaxToken GenerateTypeIdentifier()
        {
            return Descriptor.CoreTypeIdentifier;
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
