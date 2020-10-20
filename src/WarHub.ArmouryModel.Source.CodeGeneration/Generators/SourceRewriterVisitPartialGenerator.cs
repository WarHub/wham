using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class SourceRewriterVisitPartialGenerator : PartialGeneratorBase
    {
        protected SourceRewriterVisitPartialGenerator(CoreDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        public static TypeDeclarationSyntax Generate(CoreDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new SourceRewriterVisitPartialGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        protected override SyntaxToken GenerateTypeIdentifier() => Identifier(Names.SourceRewriter);

        protected override IEnumerable<MemberDeclarationSyntax> GenerateMembers()
        {
            const string Node = "node";
            // VisitXyzList
            yield return
                CreateVisitMethodBase(
                        Names.Visit + Descriptor.RawModelName + Names.ListSuffix,
                        Descriptor.GetListNodeTypeIdentifierName())
                    .AddBodyStatements(
                        ReturnStatement(
                            IdentifierName(Names.VisitListNode)
                                .Invoke(
                                    IdentifierName(Node))));
            // VisitXyz
            var nonSimpleEntries = Descriptor.Entries.Where(x => x is not CoreValueChild).ToImmutableArray();
            yield return
                CreateVisitMethodBase(
                        Names.Visit + Descriptor.RawModelName,
                        Descriptor.GetNodeTypeIdentifierName())
                    .MutateIf(
                        nonSimpleEntries.IsEmpty,
                        // short cirtuiting until visiting Tokens (simple properties) gets added
                        x =>
                            x.AddBodyStatements(
                                ReturnStatement(IdentifierName(Node))),
                        x =>
                            x.AddBodyStatements(
                                nonSimpleEntries
                                    .Select(CreateChildVisitStatement)
                                    .Append(
                                        CreateNodeVisitReturnStatement())));

            static MethodDeclarationSyntax CreateVisitMethodBase(string methodName, TypeSyntax type)
            {
                return
                    MethodDeclaration(
                            IdentifierName(Names.SourceNode),
                            methodName)
                        .AddModifiers(
                            SyntaxKind.PublicKeyword,
                            SyntaxKind.OverrideKeyword)
                        .AddParameterListParameters(
                            Parameter(
                                    Identifier(Node))
                                .WithType(type));
            }

            static StatementSyntax CreateChildVisitStatement(CoreChildBase entry)
            {
                var initializerExpression = entry switch
                {
                    CoreListChild collectionEntry =>
                        IdentifierName(Names.Visit)
                        .Invoke(
                            IdentifierName(Node)
                            .Dot(entry.IdentifierName))
                        .Cast(
                            NullableType(
                                collectionEntry.GetListNodeTypeIdentifierName()))
                        .WrapInParens()
                        .QuestionDot(
                            IdentifierName(Names.NodeList))
                        .QuestionQuestion(
                            LiteralExpression(SyntaxKind.DefaultLiteralExpression)),
                    CoreObjectChild complex =>
                        IdentifierName(Names.Visit)
                        .Invoke(
                            IdentifierName(Node)
                            .Dot(entry.IdentifierName))
                        .Cast(
                            NullableType(
                                complex.GetNodeTypeIdentifierName()))
                        .WrapInParens()
                        .MutateIf(
                            complex.Symbol.Type.NullableAnnotation != NullableAnnotation.Annotated,
                            x => x.QuestionQuestion(
                                IdentifierName(Names.NodeFactory)
                                .Dot(
                                    IdentifierName(
                                        complex.NameSyntax.ToString().StripSuffixes()))
                                .Invoke())),
                    _ => throw new NotSupportedException("Cannot visit child of simple type")
                };
                return CreateLocalDeclaration(entry, initializerExpression);
            }

            static StatementSyntax CreateLocalDeclaration(CoreChildBase entry, ExpressionSyntax initializerExpression)
            {
                return
                    LocalDeclarationStatement(
                        VariableDeclaration(
                                IdentifierName("var"))
                            .AddVariables(
                                VariableDeclarator(
                                        entry.CamelCaseIdentifier)
                                    .WithInitializer(
                                        EqualsValueClause(initializerExpression))));
            }

            StatementSyntax CreateNodeVisitReturnStatement()
            {
                return
                    ReturnStatement(
                        IdentifierName(Node)
                            .Dot(
                                IdentifierName(Names.UpdateWith))
                            .Invoke(
                                IdentifierName(Node)
                                .Dot(
                                    IdentifierName(Names.Core))
                                .With(
                                    nonSimpleEntries.Select(CreateAssignment))));

                static ExpressionSyntax CreateAssignment(CoreChildBase entry) => (entry switch
                {
                    CoreObjectChild { Symbol: { Type: { NullableAnnotation: NullableAnnotation.Annotated } } } =>
                        entry.CamelCaseIdentifierName
                        .QuestionDot(
                            IdentifierName(Names.Core)),
                    CoreObjectChild =>
                        entry.CamelCaseIdentifierName
                        .Dot(
                            IdentifierName(Names.Core)),
                    CoreListChild =>
                        entry.CamelCaseIdentifierName
                        .Dot(
                            IdentifierName(Names.ToCoreArray))
                        .Invoke(),
                    _ => throw new InvalidOperationException("This only supports non-simple entries")
                })
                .AssignTo(entry.IdentifierName);
            }
        }
    }
}
