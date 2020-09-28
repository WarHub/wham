using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class CoreDescriptorBuilder
    {
        private const string ImmutableArrayMetadataName = "System.Collections.Immutable.ImmutableArray`1";
        private const string WhamNodeCoreAttributeMetadataName = "WarHub.ArmouryModel.Source.WhamNodeCoreAttribute";

        private static SymbolCache<INamedTypeSymbol> immutableArraySymbolCache = new SymbolCache<INamedTypeSymbol>(ImmutableArrayMetadataName);
        private static SymbolCache<INamedTypeSymbol> whamNodeCoreAttributeSymbolCache = new SymbolCache<INamedTypeSymbol>(WhamNodeCoreAttributeMetadataName);

        public CoreDescriptorBuilder(TransformationContext context, CancellationToken cancellationToken = default)
        {
            SemanticModel = context.SemanticModel;
            Compilation = context.Compilation;
            TypeDeclaration = (ClassDeclarationSyntax)context.ProcessingNode;
            CancellationToken = cancellationToken;
            UpdateNamedTypeSymbolCache(ref immutableArraySymbolCache, Compilation);
            UpdateNamedTypeSymbolCache(ref whamNodeCoreAttributeSymbolCache, Compilation);
            TypeSymbol = GetNamedTypeSymbol(TypeDeclaration) ?? throw new ArgumentException("Type is not a named type.");
        }

        private SemanticModel SemanticModel { get; }
        private CSharpCompilation Compilation { get; }
        private ClassDeclarationSyntax TypeDeclaration { get; }
        private CancellationToken CancellationToken { get; }
        public static INamedTypeSymbol ImmutableArraySymbol => immutableArraySymbolCache.Symbol!;
        public INamedTypeSymbol TypeSymbol { get; }

        public CoreDescriptor CreateDescriptor()
        {
            var firstDeclaration =
                NodeFromLocation(TypeSymbol.Locations[0])
                .FirstAncestorOrSelf<ClassDeclarationSyntax>();

            var attributeLists = GetClassAttributeLists().ToImmutableArray();

            var properties =
                GetCustomBaseTypesAndSelf(TypeSymbol)
                .Reverse()
                .SelectMany(GetAutoReadonlyPropertySymbols)
                .Select(p =>
                {
                    var x = p.DeclaringSyntaxReferences.FirstOrDefault();
                    var syntax = (PropertyDeclarationSyntax?)x?.GetSyntax();
                    var auto = syntax?.AccessorList?.Accessors.All(accessor => accessor.Body == null && accessor.ExpressionBody == null) ?? false;
                    return auto ? new { syntax = syntax!, symbol = p } : null;
                })
                .Where(x => x != null)
                .Select(x => CreateRecordEntry(x!.symbol, x.syntax))
                .ToImmutableArray();

            return new CoreDescriptor(
                TypeSymbol,
                firstDeclaration.GetTypeSyntax().WithoutTrivia(),
                firstDeclaration.Identifier.WithoutTrivia(),
                properties,
                attributeLists);
        }

        private static IEnumerable<INamedTypeSymbol> GetCustomBaseTypesAndSelf(INamedTypeSymbol self)
        {
            yield return self;
            while (self.BaseType?.SpecialType == SpecialType.None)
            {
                self = self.BaseType;
                yield return self;
            }
        }

        private static IEnumerable<IPropertySymbol> GetAutoReadonlyPropertySymbols(INamedTypeSymbol namedTypeSymbol)
        {
            return namedTypeSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.IsReadOnly && !p.IsStatic && !p.IsIndexer && p.DeclaredAccessibility == Accessibility.Public);
        }

        private static SyntaxNode NodeFromLocation(Location l)
        {
            return l.SourceTree.GetRoot().FindNode(l.SourceSpan);
        }

        private IEnumerable<AttributeListSyntax> GetClassAttributeLists()
        {
            var xmlAttributeNames = new[] { Names.XmlType, Names.XmlRoot };
            var attributes = TypeSymbol.Locations
                .SelectMany(l => NodeFromLocation(l).FirstAncestorOrSelf<ClassDeclarationSyntax>().AttributeLists)
                .SelectMany(list => list.Attributes)
                .ToImmutableArray();
            var xmlAttributes = attributes.Where(att => xmlAttributeNames.Any(name => att.IsNamed(name)));
            return xmlAttributes.Select(att => SyntaxFactory.AttributeList().AddAttributes(att));
        }

        public static CoreDescriptor.Entry CreateRecordEntry(IPropertySymbol symbol, PropertyDeclarationSyntax syntax)
        {
            var typeSyntax = syntax.Type;
            var typeIdentifier = syntax.Identifier.WithoutTrivia();
            var attributes = GetPropertyAttributeLists(syntax).ToImmutableArray();
            if (symbol.Type is INamedTypeSymbol namedType && namedType.SpecialType == SpecialType.None)
            {
                if (namedType.Arity == 1 && ImmutableArraySymbol.Equals(namedType.OriginalDefinition, SymbolEqualityComparer.IncludeNullability))
                {
                    return new CoreDescriptor.CollectionEntry(symbol, typeIdentifier, (NameSyntax)typeSyntax, attributes);
                }
                if (namedType.GetAttributes().Any(a => a.AttributeClass.Equals(whamNodeCoreAttributeSymbolCache.Symbol, SymbolEqualityComparer.Default)))
                {
                    return new CoreDescriptor.ComplexEntry(symbol, typeIdentifier, typeSyntax, attributes);
                }
            }
            return new CoreDescriptor.SimpleEntry(symbol, typeIdentifier, typeSyntax, attributes);
        }

        private static IEnumerable<AttributeListSyntax> GetPropertyAttributeLists(PropertyDeclarationSyntax syntax)
        {
            var xmlAttributeNames = new[] { Names.XmlArray, Names.XmlAttribute, Names.XmlElement, Names.XmlText };
            var attributes = syntax.AttributeLists.SelectMany(list => list.Attributes);
            var xmlAttributes = attributes.Where(att => xmlAttributeNames.Any(name => att.IsNamed(name)));
            return xmlAttributes.Select(att => SyntaxFactory.AttributeList().AddAttributes(att));
        }

        private INamedTypeSymbol? GetNamedTypeSymbol(SyntaxNode node)
        {
            var namedTypeSymbol = SemanticModel.GetDeclaredSymbol(node, CancellationToken) as INamedTypeSymbol;
            return namedTypeSymbol is IErrorTypeSymbol ? null : namedTypeSymbol;
        }

        private static void UpdateNamedTypeSymbolCache(ref SymbolCache<INamedTypeSymbol> cache, CSharpCompilation compilation)
        {
            if (cache.Compilation != compilation)
            {
                cache = new SymbolCache<INamedTypeSymbol>(
                    compilation.GetTypeByMetadataName(cache.FullMetadataName),
                    compilation,
                    cache.FullMetadataName);
            }
        }

        private readonly struct SymbolCache<T>
        {
            public SymbolCache(string fullMetadataName)
            {
                FullMetadataName = fullMetadataName;
                Symbol = default;
                Compilation = null;
            }

            public SymbolCache(T symbol, CSharpCompilation compilation, string fullMetadataName)
            {
                FullMetadataName = fullMetadataName;
                Symbol = symbol;
                Compilation = compilation;
            }

            [AllowNull, MaybeNull]
            public T Symbol { get; }
            public CSharpCompilation? Compilation { get; }
            public string FullMetadataName { get; }

            public void Deconstruct([MaybeNull] out T symbol, out CSharpCompilation? compilation)
            {
                symbol = Symbol;
                compilation = Compilation;
            }
        }
    }
}
