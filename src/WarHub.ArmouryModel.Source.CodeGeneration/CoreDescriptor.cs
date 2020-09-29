using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MoreLinq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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
            CoreBuilderType = QualifiedName(coreType, IdentifierName(Names.Builder));
            ListOfCoreBuilderType = GenericName(Names.ListGeneric).AddTypeArgumentListArguments(CoreBuilderType);
            ImmutableArrayOfCoreType = GenericName(Names.ImmutableArray).AddTypeArgumentListArguments(CoreType);
            RawModelName = coreTypeIdentifier.ValueText.StripSuffixes();
            Entries = entries;
            CoreTypeAttributeLists = coreTypeAttributeLists;
            var (derived, declared) = entries.Partition(x => x.IsDerived);
            (DeclaredEntries, DerivedEntries) = (declared.ToImmutableArray(), derived.ToImmutableArray());
        }

        public INamedTypeSymbol TypeSymbol { get; }

        public NameSyntax CoreType { get; }

        public QualifiedNameSyntax CoreBuilderType { get; }

        public GenericNameSyntax ListOfCoreBuilderType { get; }

        public GenericNameSyntax ImmutableArrayOfCoreType { get; }

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
            protected Entry(IPropertySymbol symbol, bool isDerived, SyntaxToken identifier, TypeSyntax type, ImmutableArray<AttributeListSyntax> attributeLists)
            {
                Symbol = symbol;
                IsDerived = isDerived;
                Identifier = identifier;
                IdentifierName = SyntaxFactory.IdentifierName(identifier);
                Type = type;
                AttributeLists = attributeLists;
                CamelCaseIdentifier = SyntaxFactory.Identifier(identifier.ValueText.ToLowerFirstLetter());
                CamelCaseIdentifierName = SyntaxFactory.IdentifierName(CamelCaseIdentifier);
                CamelCaseParameterSyntax = SyntaxFactory.Parameter(CamelCaseIdentifier).WithType(Type);
                ArgumentSyntax = SyntaxFactory.Argument(IdentifierName);
                CamelCaseArgumentSyntax = SyntaxFactory.Argument(CamelCaseIdentifierName);
            }

            public IPropertySymbol Symbol { get; }

            public bool IsDerived { get; }

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

            public ParameterSyntax CamelCaseParameterSyntax { get; }

            public ArgumentSyntax ArgumentSyntax { get; }

            public ArgumentSyntax CamelCaseArgumentSyntax { get; }

            public abstract bool IsComplex { get; }
            public abstract bool IsCollection { get; }
            public abstract bool IsSimple { get; }
        }

        internal class SimpleEntry : Entry
        {
            public SimpleEntry(IPropertySymbol symbol, bool isDerived, SyntaxToken identifier, TypeSyntax type, ImmutableArray<AttributeListSyntax> attributeLists)
                : base(symbol, isDerived, identifier, type, attributeLists)
            {
            }

            public override bool IsSimple => true;
            public override bool IsComplex => false;
            public override bool IsCollection => false;
        }

        internal class CollectionEntry : Entry
        {
            public CollectionEntry(IPropertySymbol symbol, bool isDerived, SyntaxToken identifier, NameSyntax type, ImmutableArray<AttributeListSyntax> attributeLists)
                : base(symbol, isDerived, identifier, type, attributeLists)
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
            public ComplexEntry(IPropertySymbol symbol, bool isDerived, SyntaxToken identifier, TypeSyntax type, ImmutableArray<AttributeListSyntax> attributeLists)
                : base(symbol, isDerived, identifier, type, attributeLists)
            {
                NameSyntax = (NameSyntax) (type is NullableTypeSyntax nullable ? nullable.ElementType : type);
                BuilderType = QualifiedName(NameSyntax, IdentifierName(Names.Builder));
            }

            public NameSyntax NameSyntax { get; }

            public QualifiedNameSyntax BuilderType { get; }

            public override bool IsSimple => false;
            public override bool IsComplex => true;
            public override bool IsCollection => false;
        }
    }
}
