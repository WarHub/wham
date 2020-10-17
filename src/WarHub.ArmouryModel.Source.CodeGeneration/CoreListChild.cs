using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MoreLinq;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class CoreListChild : CoreChildBase
    {
        public CoreListChild(IPropertySymbol symbol, bool isDerived, ImmutableArray<AttributeListSyntax> attributeLists)
            : base(symbol, isDerived, attributeLists)
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
