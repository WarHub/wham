using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class CoreDescriptorBuilder
    {
        public static CoreDescriptor.Entry CreateRecordEntry(
            IPropertySymbol symbol,
            PropertyDeclarationSyntax syntax,
            INamedTypeSymbol typeSymbol,
            INamedTypeSymbol immutableArraySymbol,
            INamedTypeSymbol attributeSymbol)
        {
            var typeSyntax = syntax.Type;
            var typeIdentifier = syntax.Identifier.WithoutTrivia();
            var attributes = GetPropertyAttributeLists(syntax).ToImmutableArray();
            var isDerived = !typeSymbol.Equals(symbol.ContainingType, SymbolEqualityComparer.Default);
            if (symbol.Type is INamedTypeSymbol namedType && namedType.SpecialType == SpecialType.None)
            {
                if (namedType.Arity == 1 && immutableArraySymbol.Equals(namedType.OriginalDefinition, SymbolEqualityComparer.IncludeNullability))
                {
                    return new CoreDescriptor.CollectionEntry(symbol, isDerived, typeIdentifier, (NameSyntax)typeSyntax, attributes);
                }
                if (namedType.GetAttributes().Any(a => a.AttributeClass?.Equals(attributeSymbol, SymbolEqualityComparer.Default) == true))
                {
                    return new CoreDescriptor.ComplexEntry(symbol, isDerived, typeIdentifier, typeSyntax, attributes);
                }
            }
            return new CoreDescriptor.SimpleEntry(symbol, isDerived, typeIdentifier, typeSyntax, attributes);
        }

        private static IEnumerable<AttributeListSyntax> GetPropertyAttributeLists(PropertyDeclarationSyntax syntax)
        {
            var xmlAttributeNames = new[] { Names.XmlArray, Names.XmlAttribute, Names.XmlElement, Names.XmlText };
            var attributes = syntax.AttributeLists.SelectMany(list => list.Attributes);
            var xmlAttributes = attributes.Where(att => xmlAttributeNames.Any(name => att.IsNamed(name)));
            return xmlAttributes.Select(att => SyntaxFactory.AttributeList().AddAttributes(att));
        }
    }
}
