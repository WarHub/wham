using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MoreLinq;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal abstract class CoreChildBase
    {
        protected CoreChildBase(IPropertySymbol symbol, bool isInherited, ImmutableArray<AttributeListSyntax> xmlAttributeLists)
        {
            Symbol = symbol;
            IsInherited = isInherited;
            XmlAttributeLists = xmlAttributeLists;
        }

        public IPropertySymbol Symbol { get; }

        public bool IsInherited { get; }

        public ImmutableArray<AttributeListSyntax> XmlAttributeLists { get; }

        private PropertyDeclarationSyntax DeclarationSyntax =>
            declarationSyntax ??=
                (PropertyDeclarationSyntax)Symbol.DeclaringSyntaxReferences.Single().GetSyntax();

        private PropertyDeclarationSyntax? declarationSyntax;

        /// <summary>
        /// PascalCase (original) identifier
        /// </summary>
        public SyntaxToken Identifier =>
            identifier ??= DeclarationSyntax.Identifier.WithoutTrivia();

        private SyntaxToken? identifier;

        /// <summary>
        /// PascalCase (original) <see cref="IdentifierNameSyntax"/>.
        /// </summary>
        public IdentifierNameSyntax IdentifierName =>
            identifierName ??= SyntaxFactory.IdentifierName(Identifier);

        private IdentifierNameSyntax? identifierName;

        public TypeSyntax Type => type ??= DeclarationSyntax.Type;

        private TypeSyntax? type;

        public SyntaxToken CamelCaseIdentifier =>
            camelCaseIdentifier ??= SyntaxFactory.Identifier(Identifier.ValueText.ToLowerFirstLetter());

        private SyntaxToken? camelCaseIdentifier;

        public IdentifierNameSyntax CamelCaseIdentifierName =>
            camelCaseIdentifierName ??= SyntaxFactory.IdentifierName(CamelCaseIdentifier);

        private IdentifierNameSyntax? camelCaseIdentifierName;
    }
}
