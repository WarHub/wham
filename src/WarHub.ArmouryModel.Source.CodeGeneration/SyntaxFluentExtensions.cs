using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal static class SyntaxFluentExtensions
    {
        public static ExpressionSyntax And(this ExpressionSyntax left, ExpressionSyntax right) =>
            BinaryExpression(SyntaxKind.LogicalAndExpression, left, right);

        public static AssignmentExpressionSyntax AssignTo(this ExpressionSyntax right, ExpressionSyntax left) =>
            AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right);

        public static AssignmentExpressionSyntax Assign(this ExpressionSyntax left, ExpressionSyntax right) =>
            AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right);

        public static ExpressionStatementSyntax AsStatement(this ExpressionSyntax expr) =>
            ExpressionStatement(expr);

        public static LocalDeclarationStatementSyntax AsStatement(this VariableDeclarationSyntax declaration) =>
            LocalDeclarationStatement(declaration);

        public static ExpressionSyntax Cast(this ExpressionSyntax expr, TypeSyntax type) =>
            CastExpression(type, expr);

        public static ExpressionSyntax CastAs(this ExpressionSyntax expr, TypeSyntax type) =>
            BinaryExpression(SyntaxKind.AsExpression, expr, type);

        public static ExpressionSyntax Dot(this ExpressionSyntax expr, string name) =>
            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expr, IdentifierName(name));

        public static ExpressionSyntax Dot(this ExpressionSyntax expr, SimpleNameSyntax name) =>
            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expr, name);

        public static ExpressionSyntax ElementAccess(this ExpressionSyntax expr, params ExpressionSyntax[] indexerArg) =>
            ElementAccessExpression(expr, BracketedArgumentList(SeparatedList(indexerArg.Select(Argument))));

        public static VariableDeclarationSyntax InitVar(this SyntaxToken identifier, ExpressionSyntax value) =>
            VariableDeclaration(
                IdentifierName("var"),
                SingletonSeparatedList(
                    VariableDeclarator(
                        identifier,
                        argumentList: null,
                        initializer: EqualsValueClause(value))));

        public static VariableDeclarationSyntax InitVar(this IdentifierNameSyntax identifierName, ExpressionSyntax value) =>
            identifierName.Identifier.InitVar(value);

        public static StatementSyntax InitVarStatement(this IdentifierNameSyntax identifierName, ExpressionSyntax value) =>
            identifierName.Identifier.InitVar(value).AsStatement();

        public static ExpressionSyntax Invoke(this string identifier, params ExpressionSyntax[] args) =>
            IdentifierName(identifier).Invoke(args);

        public static ExpressionSyntax Invoke(this ExpressionSyntax expr, params ExpressionSyntax[] args) =>
            InvocationExpression(expr).AddArgumentListArguments(args.Select(Argument).ToArray());

        public static ExpressionSyntax Is(this ExpressionSyntax expr, PatternSyntax pattern) =>
            IsPatternExpression(expr, pattern);

        public static ExpressionSyntax IsNot(this ExpressionSyntax expr, PatternSyntax pattern) =>
            IsPatternExpression(expr, UnaryPattern(Token(SyntaxKind.NotKeyword), pattern));

        public static NullableTypeSyntax Nullable(this TypeSyntax @this) => NullableType(@this);

        public static ObjectCreationExpressionSyntax ObjectCreationWithInitializer(this TypeSyntax type, params ExpressionSyntax[] initializers) =>
            ObjectCreationExpression(type).WithInitializer(InitializerExpression(SyntaxKind.ObjectInitializerExpression, SeparatedList(initializers)));

        public static ExpressionSyntax OpAdd(this ExpressionSyntax left, ExpressionSyntax right) =>
            BinaryExpression(SyntaxKind.AddExpression, left, right);

        public static ExpressionSyntax OpEquals(this ExpressionSyntax left, ExpressionSyntax right) =>
            BinaryExpression(SyntaxKind.EqualsExpression, left, right);

        public static ExpressionSyntax OpLessThan(this ExpressionSyntax left, ExpressionSyntax right) =>
            BinaryExpression(SyntaxKind.LessThanExpression, left, right);

        public static ExpressionSyntax OpNotEquals(this ExpressionSyntax left, ExpressionSyntax right) =>
            BinaryExpression(SyntaxKind.NotEqualsExpression, left, right);

        public static ExpressionSyntax Or(this ExpressionSyntax left, ExpressionSyntax right) =>
            BinaryExpression(SyntaxKind.LogicalOrExpression, left, right);

        public static BinaryPatternSyntax Or(this PatternSyntax left, PatternSyntax right) =>
            BinaryPattern(SyntaxKind.OrPattern, left, right);

        public static ExpressionSyntax QuestionDot(this ExpressionSyntax expr, string name) =>
            ConditionalAccessExpression(expr, MemberBindingExpression(IdentifierName(name)));

        public static ExpressionSyntax QuestionDot(this ExpressionSyntax expr, SimpleNameSyntax name) =>
            ConditionalAccessExpression(expr, MemberBindingExpression(name));

        public static ExpressionSyntax QuestionQuestion(this ExpressionSyntax expr, ExpressionSyntax ifNullExpr) =>
            BinaryExpression(SyntaxKind.CoalesceExpression, expr, ifNullExpr);

        public static ExpressionSyntax SuppressNullableWarning(this ExpressionSyntax expr) =>
            PostfixUnaryExpression(SyntaxKind.SuppressNullableWarningExpression, expr);

        public static LiteralExpressionSyntax ToLiteralExpression(this string value) =>
            LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value));

        public static LiteralExpressionSyntax ToLiteralExpression(this int value) =>
            LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));

        public static ArrayTypeSyntax ToArrayType(this TypeSyntax identifier) =>
            ArrayType(identifier).AddRankSpecifiers(ArrayRankSpecifier());

        public static WithExpressionSyntax With(this ExpressionSyntax expr, IEnumerable<ExpressionSyntax> initializers) =>
            WithExpression(expr, InitializerExpression(SyntaxKind.WithInitializerExpression, SeparatedList(initializers)));

        public static WithExpressionSyntax With(this ExpressionSyntax expr, params ExpressionSyntax[] initializers) =>
            WithExpression(expr, InitializerExpression(SyntaxKind.WithInitializerExpression, SeparatedList(initializers)));

        public static AccessorDeclarationSyntax WithExpressionBodyFull(this AccessorDeclarationSyntax syntax, ExpressionSyntax body) =>
            syntax.WithExpressionBody(ArrowExpressionClause(body)).WithSemicolonToken();

        public static IndexerDeclarationSyntax WithExpressionBodyFull(this IndexerDeclarationSyntax syntax, ExpressionSyntax body) =>
            syntax.WithExpressionBody(ArrowExpressionClause(body)).WithSemicolonToken();

        public static MethodDeclarationSyntax WithExpressionBodyFull(this MethodDeclarationSyntax syntax, ExpressionSyntax body) =>
            syntax.WithExpressionBody(ArrowExpressionClause(body)).WithSemicolonToken();

        public static PropertyDeclarationSyntax WithExpressionBodyFull(this PropertyDeclarationSyntax syntax, ExpressionSyntax body) =>
            syntax.WithExpressionBody(ArrowExpressionClause(body)).WithSemicolonToken();

        public static SyntaxToken WithoutTrivia(this SyntaxToken syntax) =>
            syntax.WithLeadingTrivia(TriviaList()).WithTrailingTrivia(TriviaList());

        public static ExpressionSyntax WrapInParens(this ExpressionSyntax expr) =>
            ParenthesizedExpression(expr);
    }
}
