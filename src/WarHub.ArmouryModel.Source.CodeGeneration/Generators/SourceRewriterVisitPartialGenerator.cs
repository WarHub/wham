﻿using System;
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
                            .MemberAccess(entry.IdentifierName))
                        .Cast(
                            NullableType(
                                collectionEntry.GetListNodeTypeIdentifierName()))
                        .WrapInParentheses()
                        .ConditionalMemberAccess(
                            IdentifierName(Names.NodeList))
                        .Coalesce(
                            LiteralExpression(SyntaxKind.DefaultLiteralExpression)),
                    CoreObjectChild complex =>
                        IdentifierName(Names.Visit)
                        .Invoke(
                            IdentifierName(Node)
                            .MemberAccess(entry.IdentifierName))
                        .Cast(
                            NullableType(
                                complex.GetNodeTypeIdentifierName()))
                        .WrapInParentheses()
                        .Coalesce(
                            IdentifierName(Names.NodeFactory)
                            .MemberAccess(
                                IdentifierName(
                                    complex.NameSyntax.ToString().StripSuffixes()))
                            .Invoke()),
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
                            .MemberAccess(
                                IdentifierName(Names.UpdateWith))
                            .Invoke(
                                IdentifierName(Node)
                                .MemberAccess(
                                    IdentifierName(Names.Core))
                                .With(
                                    nonSimpleEntries.Select(CreateAssignment))));

                static ExpressionSyntax CreateAssignment(CoreChildBase entry) => (entry switch
                {
                    CoreObjectChild =>
                        entry.CamelCaseIdentifierName
                        .MemberAccess(
                            IdentifierName(Names.Core)),
                    CoreListChild =>
                        entry.CamelCaseIdentifierName
                        .MemberAccess(
                            IdentifierName(Names.ToCoreArray))
                        .Invoke(),
                    _ => throw new InvalidOperationException("This only supports non-simple entries")
                })
                .AssignTo(entry.IdentifierName);
            }
        }
    }
}
