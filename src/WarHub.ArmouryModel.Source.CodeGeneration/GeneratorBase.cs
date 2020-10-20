using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal abstract class GeneratorBase
    {
        protected LiteralExpressionSyntax Null { get; } =
            LiteralExpression(SyntaxKind.NullLiteralExpression, Token(SyntaxKind.NullKeyword));

        protected LiteralExpressionSyntax True { get; } =
            LiteralExpression(SyntaxKind.TrueLiteralExpression, Token(SyntaxKind.TrueKeyword));

        protected LiteralExpressionSyntax False { get; } =
            LiteralExpression(SyntaxKind.FalseLiteralExpression, Token(SyntaxKind.FalseKeyword));

        protected LiteralExpressionSyntax Zero { get; } =
            LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0));

        protected PredefinedTypeSyntax Long { get; } = PredefinedType(Token(SyntaxKind.LongKeyword));
        protected PredefinedTypeSyntax Bool { get; } = PredefinedType(Token(SyntaxKind.BoolKeyword));
        protected PredefinedTypeSyntax Void { get; } = PredefinedType(Token(SyntaxKind.VoidKeyword));
        protected PredefinedTypeSyntax String { get; } = PredefinedType(Token(SyntaxKind.StringKeyword));
        protected PredefinedTypeSyntax Object { get; } = PredefinedType(Token(SyntaxKind.ObjectKeyword));

        protected static ExpressionSyntax Not(ExpressionSyntax e) => PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, e);

        protected static SyntaxTrivia Error(string message) =>
            Trivia(
                ErrorDirectiveTrivia(true)
                .WithEndOfDirectiveToken(
                    Token(
                        TriviaList(
                            PreprocessingMessage(message)),
                        SyntaxKind.EndOfDirectiveToken,
                        TriviaList())));
    }
}
