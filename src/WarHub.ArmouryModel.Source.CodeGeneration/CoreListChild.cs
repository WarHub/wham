using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class CoreListChild : CoreChildBase
    {
        public CoreListChild(IPropertySymbol symbol, bool isDerived, ImmutableArray<AttributeListSyntax> attributeLists, XmlResolvedInfo xml)
            : base(symbol, isDerived, attributeLists, xml)
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
