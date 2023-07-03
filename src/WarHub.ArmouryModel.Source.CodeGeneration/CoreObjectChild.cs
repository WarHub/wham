using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class CoreObjectChild : CoreChildBase
    {
        public CoreObjectChild(IPropertySymbol symbol, INamedTypeSymbol parent, ImmutableArray<AttributeListSyntax> attributeLists, XmlResolvedInfo xml)
            : base(symbol, parent, attributeLists, xml)
        {
        }

        public bool IsNullable => Symbol.Type.NullableAnnotation == NullableAnnotation.Annotated;

        private NameSyntax? nameSyntax;
        public NameSyntax NameSyntax =>
            nameSyntax ??= (NameSyntax)(Type is NullableTypeSyntax nullable ? nullable.ElementType : Type);

        private QualifiedNameSyntax? builderType;
        public QualifiedNameSyntax BuilderType =>
            builderType ??= QualifiedName(NameSyntax, IdentifierName(Names.Builder));
    }
}
