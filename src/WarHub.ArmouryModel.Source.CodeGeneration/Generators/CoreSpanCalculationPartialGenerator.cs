using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class CoreSpanCalculationPartialGenerator : CorePartialGeneratorBase
    {
        protected CoreSpanCalculationPartialGenerator(CoreDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        public static TypeDeclarationSyntax Generate(CoreDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new CoreSpanCalculationPartialGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        protected override IEnumerable<MemberDeclarationSyntax> GenerateMembers()
        {
            yield return
                MethodDeclaration(IdentifierName("int"), Names.CalculateDescendantSpanLength)
                .AddModifiers(SyntaxKind.ProtectedKeyword, SyntaxKind.OverrideKeyword)
                .WithBody(
                    Block(GetStatements()));
            IEnumerable<StatementSyntax> GetStatements()
            {
                var countVar = IdentifierName(Identifier("count"));
                var literalOne = LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(1));
                yield return
                    LocalDeclarationStatement(
                        VariableDeclaration(IdentifierName("var"))
                            .AddVariables(
                                VariableDeclarator(
                                    countVar.Identifier,
                                    argumentList: null,
                                    initializer: EqualsValueClause(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0))))));
                foreach (var child in Descriptor.Entries)
                {
                    if (child is CoreValueChild)
                    {
                        yield return
                            ExpressionStatement(
                                AssignmentExpression(SyntaxKind.AddAssignmentExpression, countVar, literalOne))
                            /*.WithLeadingTrivia(Comment("for " + child.Identifier.Text))*/;
                    }
                    else if (child is CoreObjectChild)
                    {
                        // count += ({child}?.GetSpanLength()) ?? 0;
                        var callGetSpan =
                            child.IdentifierName.QuestionDot(Names.GetSpanLength).Invoke().WrapInParens().QuestionQuestion(Zero);
                        yield return
                            ExpressionStatement(
                                AssignmentExpression(
                                    SyntaxKind.AddAssignmentExpression,
                                    countVar,
                                    callGetSpan));
                    }
                    else if (child is CoreListChild)
                    {
                        // foreach (var item in {child})
                        //     count += item.GetSpanLength();
                        var itemVar = IdentifierName("item");
                        var callGetSpan =
                            itemVar.Dot(Names.GetSpanLength).Invoke();
                        var sumExpression =
                            ExpressionStatement(
                                AssignmentExpression(SyntaxKind.AddAssignmentExpression, countVar, callGetSpan));
                        yield return
                                ForEachStatement(IdentifierName("var"), itemVar.Identifier, child.IdentifierName, sumExpression);

                    }
                }
                yield return ReturnStatement(countVar);
            }
        }
    }
}
