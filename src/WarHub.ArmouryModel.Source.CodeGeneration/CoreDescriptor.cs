using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class CoreDescriptor
    {
        public CoreDescriptor(
            INamedTypeSymbol typeSymbol,
            NameSyntax coreType,
            SyntaxToken coreTypeIdentifier,
            ImmutableArray<Entry> entries,
            ImmutableArray<AttributeListSyntax> coreTypeAttributeLists)
        {
            TypeSymbol = typeSymbol;
            CoreType = coreType;
            CoreTypeIdentifier = coreTypeIdentifier;
            RawModelName = coreTypeIdentifier.ValueText.StripSuffixes();
            Entries = entries;
            CoreTypeAttributeLists = coreTypeAttributeLists;
            DeclaredEntries = entries
                .Where(x => x.Symbol.ContainingType == typeSymbol)
                .ToImmutableArray();
            DerivedEntries = entries
                .Where(x => x.Symbol.ContainingType != typeSymbol)
                .ToImmutableArray();
        }

        public INamedTypeSymbol TypeSymbol { get; }

        public NameSyntax CoreType { get; }

        public SyntaxToken CoreTypeIdentifier { get; }

        /// <summary>
        /// Gets raw (un-suffixed with Core or Node) model name.
        /// </summary>
        public string RawModelName { get; }

        public ImmutableArray<Entry> Entries { get; }

        public ImmutableArray<Entry> DerivedEntries { get; }

        public ImmutableArray<Entry> DeclaredEntries { get; }

        public ImmutableArray<AttributeListSyntax> CoreTypeAttributeLists { get; }

        internal abstract class Entry
        {
            protected Entry(IPropertySymbol symbol, SyntaxToken identifier, TypeSyntax type, ImmutableArray<AttributeListSyntax> attributeLists)
            {
                Symbol = symbol;
                Identifier = identifier;
                IdentifierName = SyntaxFactory.IdentifierName(identifier);
                Type = type;
                AttributeLists = attributeLists;
                CamelCaseIdentifier = SyntaxFactory.Identifier(identifier.ValueText.ToLowerFirstLetter());
                CamelCaseIdentifierName = SyntaxFactory.IdentifierName(CamelCaseIdentifier);
            }

            public IPropertySymbol Symbol { get; }

            /// <summary>
            /// PascalCase (original) identifier
            /// </summary>
            public SyntaxToken Identifier { get; }

            /// <summary>
            /// PascalCase (original) <see cref="IdentifierNameSyntax"/>.
            /// </summary>
            public IdentifierNameSyntax IdentifierName { get; }

            public TypeSyntax Type { get; }

            public ImmutableArray<AttributeListSyntax> AttributeLists { get; }

            public SyntaxToken CamelCaseIdentifier { get; }

            public IdentifierNameSyntax CamelCaseIdentifierName { get; }

            public abstract bool IsComplex { get; }
            public abstract bool IsCollection { get; }
            public abstract bool IsSimple { get; }
        }

        internal class SimpleEntry : Entry
        {
            public SimpleEntry(IPropertySymbol symbol, SyntaxToken identifier, TypeSyntax type, ImmutableArray<AttributeListSyntax> attributeLists)
                : base(symbol, identifier, type, attributeLists)
            {
            }

            public override bool IsSimple => true;
            public override bool IsComplex => false;
            public override bool IsCollection => false;
        }

        internal class CollectionEntry : Entry
        {
            public CollectionEntry(IPropertySymbol symbol, SyntaxToken identifier, NameSyntax type, ImmutableArray<AttributeListSyntax> attributeLists)
                : base(symbol, identifier, type, attributeLists)
            {
                CollectionTypeParameter = (NameSyntax)type
                    .DescendantNodesAndSelf()
                    .OfType<GenericNameSyntax>()
                    .First()
                    .TypeArgumentList
                    .Arguments[0];
            }

            public NameSyntax CollectionTypeParameter { get; }

            public override bool IsSimple => false;
            public override bool IsComplex => false;
            public override bool IsCollection => true;
        }

        internal class ComplexEntry : Entry
        {
            public ComplexEntry(IPropertySymbol symbol, SyntaxToken identifier, NameSyntax type, ImmutableArray<AttributeListSyntax> attributeLists)
                : base(symbol, identifier, type, attributeLists)
            {
                Type = type;
            }

            public new NameSyntax Type { get; }

            public override bool IsSimple => false;
            public override bool IsComplex => true;
            public override bool IsCollection => false;
        }
    }
}
