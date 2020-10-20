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
        protected CoreChildBase(
            IPropertySymbol symbol,
            bool isInherited,
            ImmutableArray<AttributeListSyntax> xmlAttributeLists,
            XmlResolvedInfo xml)
        {
            Symbol = symbol;
            IsInherited = isInherited;
            XmlAttributeLists = xmlAttributeLists;
            Xml = xml;
        }

        public IPropertySymbol Symbol { get; }

        /// <summary>
        /// Gets true if this property was declared in descriptor's type, not inherited.
        /// </summary>
        public bool IsDeclared => !IsInherited;

        /// <summary>
        /// Gets true if this property was inherited, not declared in descriptor's type.
        /// </summary>
        public bool IsInherited { get; }

        public ImmutableArray<AttributeListSyntax> XmlAttributeLists { get; }

        public XmlResolvedInfo Xml { get; }

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
