using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MoreLinq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class CoreDescriptorBuilder
    {
        private const string ImmutableArrayMetadataName = "System.Collections.Immutable.ImmutableArray`1";
        private const string WhamNodeCoreAttributeMetadataName = "WarHub.ArmouryModel.Source.WhamNodeCoreAttribute";

        public CoreDescriptorBuilder(
            INamedTypeSymbol immutableArraySymbol,
            INamedTypeSymbol whamNodeCoreAttributeSymbol)
        {
            ImmutableArraySymbol = immutableArraySymbol ?? throw new ArgumentNullException(nameof(immutableArraySymbol));
            WhamNodeCoreAttributeSymbol = whamNodeCoreAttributeSymbol ?? throw new ArgumentNullException(nameof(whamNodeCoreAttributeSymbol));
        }

        public INamedTypeSymbol ImmutableArraySymbol { get; }

        public INamedTypeSymbol WhamNodeCoreAttributeSymbol { get; }

        public static CoreDescriptorBuilder Create(Compilation compilation)
        {
            var attributeSymbol = compilation.GetTypeByMetadataName(WhamNodeCoreAttributeMetadataName)
                ?? throw new InvalidOperationException("Symbol not found: " + WhamNodeCoreAttributeMetadataName);
            var immutableArraySymbol = compilation.GetTypeByMetadataName(ImmutableArrayMetadataName)
                ?? throw new InvalidOperationException("Symbol not found: " + ImmutableArrayMetadataName);
            return new CoreDescriptorBuilder(immutableArraySymbol, attributeSymbol);
        }

        public CoreDescriptor CreateDescriptor(INamedTypeSymbol coreSymbol)
        {
            var declarationSyntax = (RecordDeclarationSyntax)coreSymbol.DeclaringSyntaxReferences[0].GetSyntax();
            var attributes = declarationSyntax.AttributeLists
                .SelectMany(x => x.Attributes)
                .Where(x => x.IsNamed(Names.XmlRoot) || x.IsNamed(Names.XmlType))
                .Select(x => AttributeList().AddAttributes(x))
                .ToImmutableArray();

            var entries = GetCustomBaseTypesAndSelf(coreSymbol)
                .Reverse()
                .SelectMany(x => x.GetMembers().OfType<IPropertySymbol>())
                .Where(p => p is { IsStatic: false, IsIndexer: false, DeclaredAccessibility: Accessibility.Public })
                .Select(p =>
                {
                    var x = p.DeclaringSyntaxReferences.FirstOrDefault();
                    var syntax = (PropertyDeclarationSyntax?)x?.GetSyntax();
                    var auto = syntax?.AccessorList?.Accessors.All(ac => ac is { Body: null, ExpressionBody: null, Keyword: { ValueText: "get" or "init" } }) ?? false;
                    return auto ? new { syntax = syntax!, symbol = p } : null;
                })
                .Where(x => x != null)
                .Select(x => CreateRecordEntry(x!.symbol, x.syntax, coreSymbol))
                .ToImmutableArray();
            var descriptor = new CoreDescriptor(
                coreSymbol,
                IdentifierName(declarationSyntax.Identifier),
                declarationSyntax.Identifier.WithoutTrivia(),
                entries,
                attributes);
            return descriptor;

            static IEnumerable<INamedTypeSymbol> GetCustomBaseTypesAndSelf(INamedTypeSymbol self)
            {
                yield return self;
                while (self.BaseType?.SpecialType == SpecialType.None)
                {
                    self = self.BaseType;
                    yield return self;
                }
            }
        }

        public CoreDescriptor.Entry CreateRecordEntry(
            IPropertySymbol symbol,
            PropertyDeclarationSyntax syntax,
            INamedTypeSymbol typeSymbol)
        {
            var typeSyntax = syntax.Type;
            var typeIdentifier = syntax.Identifier.WithoutTrivia();
            var attributes = GetPropertyAttributeLists(syntax).ToImmutableArray();
            var isDerived = !typeSymbol.Equals(symbol.ContainingType, SymbolEqualityComparer.Default);
            if (symbol.Type is INamedTypeSymbol namedType && namedType.SpecialType == SpecialType.None)
            {
                if (namedType.Arity == 1 && ImmutableArraySymbol.Equals(namedType.OriginalDefinition, SymbolEqualityComparer.IncludeNullability))
                {
                    return new CoreDescriptor.CollectionEntry(symbol, isDerived, typeIdentifier, (NameSyntax)typeSyntax, attributes);
                }
                if (namedType.GetAttributes().Any(a => a.AttributeClass?.Equals(WhamNodeCoreAttributeSymbol, SymbolEqualityComparer.Default) == true))
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
            return xmlAttributes.Select(att => AttributeList().AddAttributes(att));
        }
    }
}
