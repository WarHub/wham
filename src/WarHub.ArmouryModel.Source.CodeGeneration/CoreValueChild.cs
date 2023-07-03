﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class CoreValueChild : CoreChildBase
    {
        public CoreValueChild(IPropertySymbol symbol, INamedTypeSymbol parent, ImmutableArray<AttributeListSyntax> attributeLists, XmlResolvedInfo xml)
            : base(symbol, parent, attributeLists, xml)
        {
        }
    }
}
