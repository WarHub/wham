using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MoreLinq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class CoreDescriptor
    {
        private QualifiedNameSyntax? coreBuilderType;
        private GenericNameSyntax? listOfCoreBuilderType;
        private GenericNameSyntax? immutableArrayOfCoreType;
        private string? rawModelName;
        private RecordDeclarationSyntax? declarationSyntax;
        private SyntaxToken? coreTypeIdentifier;
        private IdentifierNameSyntax? coreType;

        public CoreDescriptor(
            INamedTypeSymbol typeSymbol,
            ImmutableArray<CoreChildBase> entries,
            ImmutableArray<AttributeListSyntax> xmlAttributeLists,
            XmlResolvedInfo xml)
        {
            TypeSymbol = typeSymbol;
            Entries = entries;
            XmlAttributeLists = xmlAttributeLists;
            Xml = xml;
        }

        public INamedTypeSymbol TypeSymbol { get; }

        private RecordDeclarationSyntax DeclarationSyntax =>
            declarationSyntax ??=
            (RecordDeclarationSyntax)TypeSymbol.DeclaringSyntaxReferences.Single().GetSyntax();

        public SyntaxToken CoreTypeIdentifier =>
            coreTypeIdentifier ??= DeclarationSyntax.Identifier.WithoutTrivia();

        public NameSyntax CoreType =>
            coreType ??= IdentifierName(CoreTypeIdentifier);

        public QualifiedNameSyntax CoreBuilderType =>
            coreBuilderType ??= QualifiedName(CoreType, IdentifierName(Names.Builder));

        public GenericNameSyntax ListOfCoreBuilderType =>
            listOfCoreBuilderType ??= GenericName(Names.ListGeneric).AddTypeArgumentListArguments(CoreBuilderType);

        public GenericNameSyntax ImmutableArrayOfCoreType =>
            immutableArrayOfCoreType ??= GenericName(Names.ImmutableArray).AddTypeArgumentListArguments(CoreType);

        /// <summary>
        /// Gets raw (un-suffixed with Core or Node) model name.
        /// </summary>
        public string RawModelName =>
            rawModelName ??= CoreTypeIdentifier.ValueText.StripSuffixes();

        public ImmutableArray<CoreChildBase> Entries { get; }

        public ImmutableArray<AttributeListSyntax> XmlAttributeLists { get; }

        public XmlResolvedInfo Xml { get; }
    }
}
