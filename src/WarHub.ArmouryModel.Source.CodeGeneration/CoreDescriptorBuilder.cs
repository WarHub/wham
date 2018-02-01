using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class CoreDescriptorBuilder
    {
        private const string ImmutableArrayMetadataName = "System.Collections.Immutable.ImmutableArray`1";

        private static SymbolCache<INamedTypeSymbol> ImmutableArraySymbolCache;

        public CoreDescriptorBuilder(TransformationContext context, CancellationToken cancellationToken)
        {
            SemanticModel = context.SemanticModel;
            Compilation = context.Compilation;
            TypeDeclaration = (ClassDeclarationSyntax)context.ProcessingMember;
            CancellationToken = cancellationToken;
            if (ImmutableArraySymbolCache.Compilation != Compilation)
            {
                ImmutableArraySymbolCache = CreateCache(Compilation.GetTypeByMetadataName(ImmutableArrayMetadataName), Compilation);
            }
            TypeSymbol = GetNamedTypeSymbol(TypeDeclaration);
        }

        private SemanticModel SemanticModel { get; }
        private CSharpCompilation Compilation { get; }
        private ClassDeclarationSyntax TypeDeclaration { get; }
        private CancellationToken CancellationToken { get; }
        public INamedTypeSymbol ImmutableArraySymbol => ImmutableArraySymbolCache.Symbol;
        public INamedTypeSymbol TypeSymbol { get; }

        public CoreDescriptor CreateDescriptor()
        {
            var firstDeclaration =
                NodeFromLocation(
                    TypeSymbol.Locations.First())
                .FirstAncestorOrSelf<ClassDeclarationSyntax>();

            var attributeLists = TypeSymbol.Locations
                .SelectMany(l => NodeFromLocation(l).FirstAncestorOrSelf<ClassDeclarationSyntax>().AttributeLists)
                .ToImmutableArray();

            var properties =
                GetCustomBaseTypesAndSelf(TypeSymbol)
                .Reverse()
                .SelectMany(GetAutoReadonlyPropertySymbols)
                .Select(p =>
                {
                    var x = p.DeclaringSyntaxReferences.FirstOrDefault();
                    var syntax = (PropertyDeclarationSyntax)x?.GetSyntax();
                    var auto = syntax?.AccessorList?.Accessors.All(accessor => accessor.Body == null && accessor.ExpressionBody == null) ?? false;
                    return auto ? new { syntax, symbol = p } : null;
                })
                .Where(x => x != null)
                .Select(x => CreateRecordEntry(x.symbol, x.syntax))
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
            while (self.BaseType.SpecialType == SpecialType.None)
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

        private CoreDescriptor.Entry CreateRecordEntry(IPropertySymbol symbol, PropertyDeclarationSyntax syntax)
        {
            if (symbol.Type is INamedTypeSymbol namedType
                && namedType.Arity == 1
                && namedType.OriginalDefinition == ImmutableArraySymbol)
            {
                return CreateCollectionEntry(symbol, syntax);
            }
            return CreateSimpleEntry(symbol, syntax);
        }

        private CoreDescriptor.Entry CreateSimpleEntry(IPropertySymbol symbol, PropertyDeclarationSyntax syntax)
        {
            var typeString = symbol.Type.ToDisplayString();
            var typeSyntax = SyntaxFactory.ParseTypeName(typeString);
            return new CoreDescriptor.SimpleEntry(
                symbol,
                syntax.Identifier.WithoutTrivia(),
                typeSyntax,
                syntax.AttributeLists.ToImmutableArray());
        }

        private CoreDescriptor.Entry CreateCollectionEntry(IPropertySymbol symbol, PropertyDeclarationSyntax syntax)
        {
            var typeString = symbol.Type.ToDisplayString();
            var typeSyntax = SyntaxFactory.ParseTypeName(typeString);
            return new CoreDescriptor.CollectionEntry(
                symbol,
                syntax.Identifier.WithoutTrivia(),
                (NameSyntax)typeSyntax,
                syntax.AttributeLists.ToImmutableArray());
        }

        private INamedTypeSymbol GetNamedTypeSymbol(SyntaxNode node)
        {
            var namedTypeSymbol = SemanticModel.GetDeclaredSymbol(node, CancellationToken) as INamedTypeSymbol;
            return namedTypeSymbol is IErrorTypeSymbol ? null : namedTypeSymbol;
        }

        private static SymbolCache<T> CreateCache<T>(T symbol, CSharpCompilation compilation)
        {
            return new SymbolCache<T>(symbol, compilation);
        }

        private struct SymbolCache<T>
        {
            public SymbolCache(T symbol, CSharpCompilation compilation)
            {
                Symbol = symbol;
                Compilation = compilation;
            }
            public T Symbol { get; }
            public CSharpCompilation Compilation { get; }

            public void Deconstruct(out T symbol, out CSharpCompilation compilation)
            {
                symbol = Symbol;
                compilation = Compilation;
            }
        }
    }
}
