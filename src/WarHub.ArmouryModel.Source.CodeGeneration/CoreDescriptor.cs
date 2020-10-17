using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
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
            ImmutableArray<CoreChildBase> entries,
            ImmutableArray<AttributeListSyntax> coreTypeAttributeLists)
        {
            TypeSymbol = typeSymbol;
            CoreType = coreType;
            CoreTypeIdentifier = coreTypeIdentifier;
            Entries = entries;
            CoreTypeAttributeLists = coreTypeAttributeLists;
            CoreBuilderType = QualifiedName(coreType, IdentifierName(Names.Builder));
            ListOfCoreBuilderType = GenericName(Names.ListGeneric).AddTypeArgumentListArguments(CoreBuilderType);
            ImmutableArrayOfCoreType = GenericName(Names.ImmutableArray).AddTypeArgumentListArguments(CoreType);
            RawModelName = coreTypeIdentifier.ValueText.StripSuffixes();
            var (derived, declared) = entries.Partition(x => x.IsInherited);
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

        public ImmutableArray<CoreChildBase> Entries { get; }

        public ImmutableArray<CoreChildBase> DerivedEntries { get; }

        public ImmutableArray<CoreChildBase> DeclaredEntries { get; }

        public ImmutableArray<AttributeListSyntax> CoreTypeAttributeLists { get; }

    }
}
