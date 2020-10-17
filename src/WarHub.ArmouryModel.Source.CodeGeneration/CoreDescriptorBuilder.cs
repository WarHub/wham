using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MoreLinq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class CoreDescriptorBuilder
    {
        private const string XmlArrayAttributeMetadataName = "System.Xml.Serialization.XmlArrayAttribute";
        private const string XmlAttributeAttributeMetadataName = "System.Xml.Serialization.XmlAttributeAttribute";
        private const string XmlElementAttributeMetadataName = "System.Xml.Serialization.XmlElementAttribute";
        private const string XmlEnumAttributeMetadataName = "System.Xml.Serialization.XmlEnumAttribute";
        private const string XmlRootAttributeMetadataName = "System.Xml.Serialization.XmlRootAttribute";
        private const string XmlTextAttributeMetadataName = "System.Xml.Serialization.XmlTextAttribute";
        private const string XmlTypeAttributeMetadataName = "System.Xml.Serialization.XmlTypeAttribute";
        private const string ImmutableArrayMetadataName = "System.Collections.Immutable.ImmutableArray`1";
        private const string WhamNodeCoreAttributeMetadataName = "WarHub.ArmouryModel.Source.WhamNodeCoreAttribute";

        public CoreDescriptorBuilder(
            INamedTypeSymbol immutableArraySymbol,
            INamedTypeSymbol whamNodeCoreAttributeSymbol,
            ImmutableDictionary<INamedTypeSymbol, Type> xmlAttributeSymbols)
        {
            ImmutableArraySymbol = immutableArraySymbol ?? throw new ArgumentNullException(nameof(immutableArraySymbol));
            WhamNodeCoreAttributeSymbol = whamNodeCoreAttributeSymbol ?? throw new ArgumentNullException(nameof(whamNodeCoreAttributeSymbol));
            XmlAttributeSymbols = xmlAttributeSymbols;
        }

        public INamedTypeSymbol ImmutableArraySymbol { get; }

        public INamedTypeSymbol WhamNodeCoreAttributeSymbol { get; }

        public ImmutableDictionary<INamedTypeSymbol, Type> XmlAttributeSymbols { get; }

        public static CoreDescriptorBuilder Create(Compilation compilation)
        {
            var attributeSymbol = compilation.GetTypeByMetadataNameOrThrow(WhamNodeCoreAttributeMetadataName);
            var immutableArraySymbol = compilation.GetTypeByMetadataNameOrThrow(ImmutableArrayMetadataName);
            var xmlAttributeSymbols = new[]
            {
                (MetadataName: XmlArrayAttributeMetadataName, Type: typeof(XmlArrayAttribute)),
                (MetadataName: XmlAttributeAttributeMetadataName, Type: typeof(XmlAttributeAttribute)),
                (MetadataName: XmlElementAttributeMetadataName, Type: typeof(XmlElementAttribute)),
                (MetadataName: XmlEnumAttributeMetadataName, Type: typeof(XmlEnumAttribute)),
                (MetadataName: XmlRootAttributeMetadataName, Type: typeof(XmlRootAttribute)),
                (MetadataName: XmlTextAttributeMetadataName, Type: typeof(XmlTextAttribute)),
                (MetadataName: XmlTypeAttributeMetadataName, Type: typeof(XmlTypeAttribute))
            }
            .Select(x => (Symbol: compilation.GetTypeByMetadataNameOrThrow(x.MetadataName), x.Type))
            .ToImmutableDictionary(x => x.Symbol, x => x.Type);
            return new CoreDescriptorBuilder(immutableArraySymbol, attributeSymbol, xmlAttributeSymbols);
        }

        public CoreDescriptor CreateDescriptor(INamedTypeSymbol coreSymbol)
        {
            var declarationSyntax = (RecordDeclarationSyntax)coreSymbol.DeclaringSyntaxReferences[0].GetSyntax();
            var xmlAttributes = declarationSyntax.AttributeLists
                .SelectMany(x => x.Attributes)
                .Where(x => x.IsNamed(Names.XmlRoot) || x.IsNamed(Names.XmlType))
                .Select(x => AttributeList(SingletonSeparatedList(x)))
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
                .Select(x => CreateCoreChild(x!.symbol, x.syntax, coreSymbol))
                .ToImmutableArray();
            var descriptor = new CoreDescriptor(
                coreSymbol,
                entries,
                xmlAttributes,
                CreateXmlResolvedInfo(coreSymbol));
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

        public CoreChildBase CreateCoreChild(
            IPropertySymbol symbol,
            PropertyDeclarationSyntax syntax,
            INamedTypeSymbol typeSymbol)
        {
            var isInherited = !typeSymbol.Equals(symbol.ContainingType, SymbolEqualityComparer.Default);
            var attributes = GetPropertyAttributeLists(syntax).ToImmutableArray();
            var xml = CreateXmlResolvedInfo(symbol);
            if (symbol.Type is INamedTypeSymbol namedType && namedType.SpecialType == SpecialType.None)
            {
                if (namedType.Arity == 1 && ImmutableArraySymbol.Equals(namedType.OriginalDefinition, SymbolEqualityComparer.IncludeNullability))
                {
                    return new CoreListChild(symbol, isInherited, attributes, xml);
                }
                if (namedType.GetAttributes().Any(a => a.AttributeClass?.Equals(WhamNodeCoreAttributeSymbol, SymbolEqualityComparer.Default) == true))
                {
                    return new CoreObjectChild(symbol, isInherited, attributes, xml);
                }
            }
            return new CoreValueChild(symbol, isInherited, attributes, xml);
        }

        private static IEnumerable<AttributeListSyntax> GetPropertyAttributeLists(PropertyDeclarationSyntax syntax)
        {
            var xmlAttributeNames = new[] { Names.XmlArray, Names.XmlAttribute, Names.XmlElement, Names.XmlText };
            var attributes = syntax.AttributeLists.SelectMany(list => list.Attributes);
            var xmlAttributes = attributes.Where(att => xmlAttributeNames.Any(name => att.IsNamed(name)));
            return xmlAttributes.Select(att => AttributeList().AddAttributes(att));
        }

        public XmlResolvedInfo CreateXmlResolvedInfo(ISymbol symbol)
        {
            var xmlAttributes = GetXmlAttributes().ToImmutableArray();
            foreach (var attribute in xmlAttributes)
            {
                var info = attribute switch
                {
                    XmlAttributeAttribute a => XmlResolvedInfo.CreateAttribute(a.AttributeName ?? symbol.Name),
                    XmlArrayAttribute a => XmlResolvedInfo.CreateArray(a.ElementName ?? symbol.Name),
                    XmlElementAttribute a => XmlResolvedInfo.CreateElement(a.ElementName ?? symbol.Name),
                    //XmlEnumAttribute att => XmlResolvedInfo.CreateAttribute(att.AttributeName ?? property.Name),
                    XmlRootAttribute a => XmlResolvedInfo.CreateRootElement(a.ElementName, a.Namespace ?? ""),
                    XmlTextAttribute _ => XmlResolvedInfo.CreateTextContent(),
                    XmlTypeAttribute a => XmlResolvedInfo.CreateElement(a.TypeName, a.Namespace ?? ""),
                    _ => null,
                };
                if (info is { })
                    return info;
            }
            return XmlResolvedInfo.CreateElement(symbol.Name);

            IEnumerable<Attribute> GetXmlAttributes()
            {
                foreach (var data in symbol.GetAttributes())
                {
                    if (data.AttributeClass is { } @class && XmlAttributeSymbols.TryGetValue(@class, out var type))
                        yield return data.Instantiate(type);
                }
            }
        }
    }
}
