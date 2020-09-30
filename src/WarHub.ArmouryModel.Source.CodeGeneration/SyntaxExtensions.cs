using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal static class SyntaxExtensions
    {
        public static StructDeclarationSyntax AddMembers(this StructDeclarationSyntax syntax, IEnumerable<MemberDeclarationSyntax> members)
        {
            return syntax.AddMembers(members.ToArray());
        }

        public static ClassDeclarationSyntax AddModifiers(this ClassDeclarationSyntax syntax, params SyntaxKind[] modifier)
        {
            return syntax.AddModifiers(modifier.Select(Token).ToArray());
        }

        public static StructDeclarationSyntax AddModifiers(this StructDeclarationSyntax syntax, params SyntaxKind[] modifier)
        {
            return syntax.AddModifiers(modifier.Select(Token).ToArray());
        }

        public static FieldDeclarationSyntax AddModifiers(this FieldDeclarationSyntax syntax, params SyntaxKind[] modifier)
        {
            return syntax.AddModifiers(modifier.Select(Token).ToArray());
        }

        public static ParameterSyntax AddModifiers(this ParameterSyntax syntax, params SyntaxKind[] modifier)
        {
            return syntax.AddModifiers(modifier.Select(Token).ToArray());
        }

        public static ConversionOperatorDeclarationSyntax AddModifiers(this ConversionOperatorDeclarationSyntax syntax, params SyntaxKind[] modifier)
        {
            return syntax.AddModifiers(modifier.Select(Token).ToArray());
        }

        public static MethodDeclarationSyntax AddModifiers(this MethodDeclarationSyntax syntax, params SyntaxKind[] modifier)
        {
            return syntax.AddModifiers(modifier.Select(Token).ToArray());
        }

        public static PropertyDeclarationSyntax AddModifiers(this PropertyDeclarationSyntax syntax, params SyntaxKind[] modifier)
        {
            return syntax.AddModifiers(modifier.Select(Token).ToArray());
        }

        public static IndexerDeclarationSyntax AddModifiers(this IndexerDeclarationSyntax syntax, params SyntaxKind[] modifier)
        {
            return syntax.AddModifiers(modifier.Select(Token).ToArray());
        }

        public static ConstructorDeclarationSyntax AddModifiers(this ConstructorDeclarationSyntax syntax, params SyntaxKind[] modifier)
        {
            return syntax.AddModifiers(modifier.Select(Token).ToArray());
        }

        public static PropertyDeclarationSyntax AddAttributeListAttribute(this PropertyDeclarationSyntax syntax, AttributeSyntax attribute)
        {
            return syntax.AddAttributeLists(AttributeList(SingletonSeparatedList(attribute)));
        }

        public static MethodDeclarationSyntax AddAttributeListAttributes(this MethodDeclarationSyntax syntax, params AttributeSyntax[] attributes)
        {
            return syntax.AddAttributeLists(AttributeList(SeparatedList(attributes.ToArray())));
        }

        public static PropertyDeclarationSyntax AddAttributeLists(this PropertyDeclarationSyntax syntax, IEnumerable<AttributeListSyntax> lists)
        {
            return syntax.AddAttributeLists(lists.ToArray());
        }

        public static ClassDeclarationSyntax AddAttributeLists(this ClassDeclarationSyntax syntax, IEnumerable<AttributeListSyntax> lists)
        {
            return syntax.AddAttributeLists(lists.ToArray());
        }

        public static StructDeclarationSyntax AddAttributeLists(this StructDeclarationSyntax syntax, IEnumerable<AttributeListSyntax> lists)
        {
            return syntax.AddAttributeLists(lists.ToArray());
        }

        public static MethodDeclarationSyntax AddAttributeLists(this MethodDeclarationSyntax syntax, IEnumerable<AttributeListSyntax> lists)
        {
            return syntax.AddAttributeLists(lists.ToArray());
        }

        public static ConstructorDeclarationSyntax AddParameterListParameters(this ConstructorDeclarationSyntax syntax, IEnumerable<ParameterSyntax> parameters)
        {
            return syntax.AddParameterListParameters(parameters.ToArray());
        }

        public static MethodDeclarationSyntax AddParameterListParameters(this MethodDeclarationSyntax syntax, IEnumerable<ParameterSyntax> parameters)
        {
            return syntax.AddParameterListParameters(parameters.ToArray());
        }

        public static ObjectCreationExpressionSyntax AddArgumentListArguments(this ObjectCreationExpressionSyntax syntax, IEnumerable<ArgumentSyntax> arguments)
        {
            return syntax.AddArgumentListArguments(arguments.ToArray());
        }

        public static InvocationExpressionSyntax AddArgumentListArguments(this InvocationExpressionSyntax syntax, IEnumerable<ArgumentSyntax> arguments)
        {
            return syntax.AddArgumentListArguments(arguments.ToArray());
        }

        public static ConstructorInitializerSyntax AddArgumentListArguments(this ConstructorInitializerSyntax syntax, IEnumerable<ArgumentSyntax> arguments)
        {
            return syntax.AddArgumentListArguments(arguments.ToArray());
        }

        public static MethodDeclarationSyntax WithExpressionBodyFull(this MethodDeclarationSyntax syntax, ExpressionSyntax body)
        {
            return
                syntax
                .WithExpressionBody(
                    ArrowExpressionClause(body))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken));
        }

        public static PropertyDeclarationSyntax WithExpressionBodyFull(this PropertyDeclarationSyntax syntax, ExpressionSyntax body)
        {
            return
                syntax
                .WithExpressionBody(
                    ArrowExpressionClause(body))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken));
        }

        public static IndexerDeclarationSyntax WithExpressionBodyFull(this IndexerDeclarationSyntax syntax, ExpressionSyntax body)
        {
            return
                syntax
                .WithExpressionBody(
                    ArrowExpressionClause(body))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken));
        }

        public static AccessorDeclarationSyntax WithExpressionBodyFull(this AccessorDeclarationSyntax syntax, ExpressionSyntax body)
        {
            return
                syntax
                .WithExpressionBody(
                    ArrowExpressionClause(body))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken));
        }

        public static MethodDeclarationSyntax AddBodyStatements(this MethodDeclarationSyntax syntax, IEnumerable<StatementSyntax> parameters)
        {
            return syntax.AddBodyStatements(parameters.ToArray());
        }

        public static ConstructorDeclarationSyntax AddBodyStatements(this ConstructorDeclarationSyntax syntax, IEnumerable<StatementSyntax> parameters)
        {
            return syntax.AddBodyStatements(parameters.ToArray());
        }

        public static AccessorDeclarationSyntax WithSemicolonTokenDefault(this AccessorDeclarationSyntax syntax)
        {
            return syntax.WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        public static MethodDeclarationSyntax WithSemicolonTokenDefault(this MethodDeclarationSyntax syntax)
        {
            return syntax.WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        public static ExpressionSyntax MemberAccess(this ExpressionSyntax expr, SimpleNameSyntax name)
        {
            return MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expr, name);
        }

        public static ExpressionSyntax AsCast(this ExpressionSyntax expr, TypeSyntax type)
        {
            return BinaryExpression(SyntaxKind.AsExpression, expr, type);
        }

        public static ExpressionSyntax Cast(this ExpressionSyntax expr, TypeSyntax type)
        {
            return CastExpression(type, expr);
        }

        public static ExpressionSyntax ConditionalMemberAccess(this ExpressionSyntax expr, SimpleNameSyntax name)
        {
            return ConditionalAccessExpression(expr, MemberBindingExpression(name));
        }

        public static ExpressionSyntax Coalesce(this ExpressionSyntax expr, ExpressionSyntax ifNullExpr)
        {
            return BinaryExpression(SyntaxKind.CoalesceExpression, expr, ifNullExpr);
        }

        public static ExpressionSyntax InvokeWithArguments(this ExpressionSyntax expr, params ExpressionSyntax[] args)
        {
            return InvocationExpression(expr).AddArgumentListArguments(args.Select(Argument).ToArray());
        }

        public static ExpressionSyntax InvokeWithArguments(this ExpressionSyntax expr, IEnumerable<ExpressionSyntax> args)
        {
            return InvocationExpression(expr).AddArgumentListArguments(args.Select(Argument).ToArray());
        }

        public static ObjectCreationExpressionSyntax WithArguments(this ObjectCreationExpressionSyntax expr, params ExpressionSyntax[] args)
        {
            return expr.AddArgumentListArguments(args.Select(Argument));
        }

        public static ObjectCreationExpressionSyntax WithArguments(this ObjectCreationExpressionSyntax expr, IEnumerable<ExpressionSyntax> args)
        {
            return expr.AddArgumentListArguments(args.Select(Argument));
        }

        public static ParenthesizedExpressionSyntax WrapInParentheses(this ExpressionSyntax expr)
        {
            return ParenthesizedExpression(expr);
        }

        public static ExpressionStatementSyntax AsStatement(this ExpressionSyntax expr)
        {
            return ExpressionStatement(expr);
        }

        public static ArrayTypeSyntax ToArrayType(this TypeSyntax identifier)
        {
            return ArrayType(identifier).AddRankSpecifiers(ArrayRankSpecifier());
        }

        public static AssignmentExpressionSyntax AssignTo(this ExpressionSyntax right, ExpressionSyntax left)
        {
            return AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right);
        }

        public static AssignmentExpressionSyntax Assign(this ExpressionSyntax left, ExpressionSyntax right)
        {
            return AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right);
        }

        public static WithExpressionSyntax With(this ExpressionSyntax expr, IEnumerable<ExpressionSyntax> initializers) =>
            WithExpression(expr, InitializerExpression(SyntaxKind.WithInitializerExpression, SeparatedList(initializers)));

        public static WithExpressionSyntax With(this ExpressionSyntax expr, params ExpressionSyntax[] initializers) =>
            expr.With(initializers.AsEnumerable());

        public static ObjectCreationExpressionSyntax ObjectCreationWithInitializer(this TypeSyntax type, params ExpressionSyntax[] initializers) =>
            ObjectCreationExpression(type).WithInitializer(InitializerExpression(SyntaxKind.ObjectInitializerExpression, SeparatedList(initializers)));

        public static bool IsNamed(this AttributeSyntax attribute, string name)
        {
            return attribute.Name is IdentifierNameSyntax id && (id.Identifier.Text == name || id.Identifier.Text == name + "Attribute");
        }

        public static SyntaxToken WithoutTrivia(this SyntaxToken syntax)
        {
            return syntax.WithLeadingTrivia(TriviaList()).WithTrailingTrivia(TriviaList());
        }

        public static NameSyntax GetTypeSyntax(this TypeDeclarationSyntax typeDeclaration)
        {
            var identifier = typeDeclaration.Identifier.WithoutTrivia();
            var typeParamList = typeDeclaration.TypeParameterList;
            if (typeParamList == null)
            {
                return IdentifierName(identifier);
            }
            return
                GenericName(identifier)
                .AddTypeArgumentListArguments(
                    typeParamList.Parameters
                    .Select(param => IdentifierName(param.Identifier.WithoutTrivia()))
                    .ToArray());
        }

        public static TypeSyntax WithNamespace(this SimpleNameSyntax name, string @namespace)
        {
            return QualifiedName(ParseName(@namespace), name);
        }

        /// <summary>
        /// Returns result of invoking <paramref name="mutation"/> on <paramref name="original"/>
        /// if <paramref name="condition"/> is true, else <paramref name="original"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <param name="condition"></param>
        /// <param name="mutation"></param>
        /// <returns></returns>
        public static T MutateIf<T>(this T original, bool condition, Func<T, T> mutation)
        {
            return condition ? mutation(original) : original;
        }

        public static T MutateIf<T>(this T original, bool condition, Func<T, T> mutationIf, Func<T, T> mutationElse)
        {
            return condition ? mutationIf(original) : mutationElse(original);
        }

        public static T Mutate<T>(this T original, Func<T, T> mutation) => mutation(original);

        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
        {
            key = pair.Key;
            value = pair.Value;
        }
    }
}
