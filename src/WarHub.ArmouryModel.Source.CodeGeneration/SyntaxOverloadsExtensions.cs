using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal static class SyntaxOverloadsExtensions
    {
        public static PropertyDeclarationSyntax AddAttributeListAttributes(this PropertyDeclarationSyntax syntax, params AttributeSyntax[] attributes) =>
            syntax.AddAttributeLists(AttributeList(SeparatedList(attributes.ToArray())));

        public static TMemberDeclaration AddAttributeLists<TMemberDeclaration>(this TMemberDeclaration syntax, IEnumerable<AttributeListSyntax> lists)
            where TMemberDeclaration : MemberDeclarationSyntax =>
            (TMemberDeclaration)syntax.AddAttributeLists(lists.ToArray());

        public static MethodDeclarationSyntax AddBodyStatements(this MethodDeclarationSyntax syntax, IEnumerable<StatementSyntax> parameters) =>
            syntax.AddBodyStatements(parameters.ToArray());

        public static ConstructorDeclarationSyntax AddBodyStatements(this ConstructorDeclarationSyntax syntax, IEnumerable<StatementSyntax> parameters) =>
            syntax.AddBodyStatements(parameters.ToArray());

        public static TTypeDeclaration AddMembers<TTypeDeclaration>(this TTypeDeclaration syntax, IEnumerable<MemberDeclarationSyntax> members)
            where TTypeDeclaration : TypeDeclarationSyntax =>
            (TTypeDeclaration)syntax.AddMembers(members.ToArray());

        public static NamespaceDeclarationSyntax AddMembers(this NamespaceDeclarationSyntax syntax, IEnumerable<MemberDeclarationSyntax> members) =>
            syntax.AddMembers(members.ToArray());

        public static ParameterSyntax AddModifiers(this ParameterSyntax syntax, params SyntaxKind[] modifier) =>
            syntax.AddModifiers(modifier.Select(Token).ToArray());

        public static TMemberDeclaration AddModifiers<TMemberDeclaration>(this TMemberDeclaration syntax, params SyntaxKind[] modifier)
            where TMemberDeclaration : MemberDeclarationSyntax =>
            (TMemberDeclaration)syntax.AddModifiers(modifier.Select(Token).ToArray());

        public static MethodDeclarationSyntax AddParameterListParameters(this MethodDeclarationSyntax syntax, IEnumerable<ParameterSyntax> parameters) =>
            syntax.AddParameterListParameters(parameters.ToArray());

        public static AccessorDeclarationSyntax WithSemicolonToken(this AccessorDeclarationSyntax syntax) =>
            syntax.WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        public static IndexerDeclarationSyntax WithSemicolonToken(this IndexerDeclarationSyntax syntax) =>
            syntax.WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        public static MethodDeclarationSyntax WithSemicolonToken(this MethodDeclarationSyntax syntax) =>
            syntax.WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        public static PropertyDeclarationSyntax WithSemicolonToken(this PropertyDeclarationSyntax syntax) =>
            syntax.WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }
}
