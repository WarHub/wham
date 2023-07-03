using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MoreLinq;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class CoreListChild : CoreChildBase
    {
        public CoreListChild(IPropertySymbol symbol, INamedTypeSymbol parent, ImmutableArray<AttributeListSyntax> attributeLists, XmlResolvedInfo xml)
            : base(symbol, parent, attributeLists, xml)
        {
        }

        private NameSyntax? collectionTypeParameter;
        public NameSyntax CollectionTypeParameter =>
            collectionTypeParameter ??= CreateCollectionTypeParameter();

        private NameSyntax CreateCollectionTypeParameter()
        {
            return (NameSyntax)Type
                .DescendantNodesAndSelf()
                .OfType<GenericNameSyntax>()
                .First().TypeArgumentList.Arguments[0];
        }
    }
}
