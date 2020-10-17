using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class CoreValueChild : CoreChildBase
    {
        public CoreValueChild(IPropertySymbol symbol, bool isDerived, ImmutableArray<AttributeListSyntax> attributeLists, XmlResolvedInfo xml)
            : base(symbol, isDerived, attributeLists, xml)
        {
        }
    }
}
