using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class CoreObjectChild : CoreChildBase
    {
        public CoreObjectChild(IPropertySymbol symbol, bool isDerived, ImmutableArray<AttributeListSyntax> attributeLists, XmlResolvedInfo xml)
            : base(symbol, isDerived, attributeLists, xml)
        {
        }

        private NameSyntax? nameSyntax;
        public NameSyntax NameSyntax =>
            nameSyntax ??= (NameSyntax)(Type is NullableTypeSyntax nullable ? nullable.ElementType : Type);

        private QualifiedNameSyntax? builderType;
        public QualifiedNameSyntax BuilderType =>
            builderType ??= QualifiedName(NameSyntax, IdentifierName(Names.Builder));
    }
}
