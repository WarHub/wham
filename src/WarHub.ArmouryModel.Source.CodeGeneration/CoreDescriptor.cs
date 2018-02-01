﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal partial class CoreDescriptor
    {
        public CoreDescriptor(INamedTypeSymbol TypeSymbol, NameSyntax CoreType, SyntaxToken CoreTypeIdentifier, ImmutableArray<Entry> Entries, ImmutableArray<AttributeListSyntax> CoreTypeAttributeLists)
        {
            this.TypeSymbol = TypeSymbol;
            this.CoreType = CoreType;
            this.CoreTypeIdentifier = CoreTypeIdentifier;
            this.Entries = Entries;
            this.CoreTypeAttributeLists = CoreTypeAttributeLists;
            DeclaredEntries = Entries
                .Where(x => x.Symbol.ContainingType == TypeSymbol)
                .ToImmutableArray();
            DerivedEntries = Entries
                .Where(x => x.Symbol.ContainingType != TypeSymbol)
                .ToImmutableArray();
        }

        public INamedTypeSymbol TypeSymbol { get; }

        public NameSyntax CoreType { get; }

        public SyntaxToken CoreTypeIdentifier { get; }

        public ImmutableArray<Entry> Entries { get; }

        public ImmutableArray<Entry> DerivedEntries { get; }

        public ImmutableArray<Entry> DeclaredEntries { get; }

        public ImmutableArray<AttributeListSyntax> CoreTypeAttributeLists { get; }

        internal abstract class Entry
        {
            public Entry(IPropertySymbol Symbol, SyntaxToken Identifier, TypeSyntax Type, ImmutableArray<AttributeListSyntax> AttributeLists)
            {
                this.Symbol = Symbol;
                this.Identifier = Identifier;
                this.IdentifierName = SyntaxFactory.IdentifierName(Identifier);
                this.Type = Type;
                this.AttributeLists = AttributeLists;
                this.CamelCaseIdentifier = SyntaxFactory.Identifier(Identifier.ValueText.ToLowerFirstLetter());
                this.CamelCaseIdentifierName = SyntaxFactory.IdentifierName(this.CamelCaseIdentifier);
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
        }

        internal class SimpleEntry : Entry
        {
            public SimpleEntry(IPropertySymbol Symbol, SyntaxToken Identifier, TypeSyntax Type, ImmutableArray<AttributeListSyntax> AttributeLists)
                : base(Symbol, Identifier, Type, AttributeLists)
            {
            }
        }

        internal class CollectionEntry : Entry
        {
            public CollectionEntry(IPropertySymbol Symbol, SyntaxToken Identifier, NameSyntax Type, ImmutableArray<AttributeListSyntax> AttributeLists)
                : base(Symbol, Identifier, Type, AttributeLists)
            {
                CollectionTypeParameter = (NameSyntax)Type
                    .DescendantNodesAndSelf()
                    .OfType<GenericNameSyntax>()
                    .First()
                    .TypeArgumentList
                    .Arguments[0];
            }
            public NameSyntax CollectionTypeParameter { get; }
        }
    }
}
