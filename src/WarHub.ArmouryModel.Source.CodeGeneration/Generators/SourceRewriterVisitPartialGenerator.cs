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
            const string node = "node";
            // VisitXyzList
            yield return
                CreateVisitMethodBase(
                        Names.Visit + Descriptor.RawModelName + Names.ListSuffix,
                        Descriptor.GetListNodeTypeIdentifierName())
                    .AddBodyStatements(
                        ReturnStatement(
                            IdentifierName(Names.VisitNodeList)
                                .InvokeWithArguments(
                                    IdentifierName(node)
                                        .MemberAccess(
                                            IdentifierName(Names.NodeList)))
                                .MemberAccess(
                                    IdentifierName(Names.ToListNode))
                                .InvokeWithArguments()));
            // VisitXyz
            var nonSimpleEntries = Descriptor.Entries.Where(x => !x.IsSimple).ToImmutableArray();
            yield return
                CreateVisitMethodBase(
                        Names.Visit + Descriptor.RawModelName,
                        Descriptor.GetNodeTypeIdentifierName())
                    .MutateIf(
                        nonSimpleEntries.IsEmpty,
                        // short cirtuiting until visiting Tokens (simple properties) gets added
                        x =>
                            x.AddBodyStatements(
                                ReturnStatement(IdentifierName(node))),
                        x =>
                            x.AddBodyStatements(
                                nonSimpleEntries
                                    .Select(CreateChildVisitStatement)
                                    .Append(
                                        CreateNodeVisitReturnStatement())));

            MethodDeclarationSyntax CreateVisitMethodBase(string methodName, TypeSyntax type)
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
                                    Identifier(node))
                                .WithType(type));
            }

            StatementSyntax CreateChildVisitStatement(CoreDescriptor.Entry entry)
            {
                var targetType = entry is CoreDescriptor.CollectionEntry collectionEntry
                    ? collectionEntry.GetListNodeTypeIdentifierName()
                    : ((CoreDescriptor.ComplexEntry)entry).GetNodeTypeIdentifierName();
                return
                    CreateLocalDeclaration(
                        entry,
                        IdentifierName(Names.Visit)
                            .InvokeWithArguments(
                                IdentifierName(node)
                                    .MemberAccess(entry.IdentifierName))
                            .Cast(targetType));
            }

            StatementSyntax CreateLocalDeclaration(CoreDescriptor.Entry entry, ExpressionSyntax initializerExpression)
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
                        IdentifierName(node)
                            .MemberAccess(
                                IdentifierName(Names.UpdateWith))
                            .InvokeWithArguments(
                                IdentifierName(node)
                                    .MemberAccess(
                                        IdentifierName(Names.Core))
                                    .MemberAccess(
                                        IdentifierName(Names.Update))
                                    .InvokeWithArguments(
                                        Descriptor.Entries
                                            .Select(SimpleArgument, ComplexArgument, CollectionArgument))));

                ExpressionSyntax SimpleArgument(CoreDescriptor.SimpleEntry entry)
                {
                    return
                        IdentifierName(node)
                            .MemberAccess(
                                IdentifierName(Names.Core))
                            .MemberAccess(entry.IdentifierName);
                }
                ExpressionSyntax ComplexArgument(CoreDescriptor.ComplexEntry entry)
                {
                    return
                        entry.CamelCaseIdentifierName.MemberAccess(
                            IdentifierName(Names.Core));
                }
                ExpressionSyntax CollectionArgument(CoreDescriptor.CollectionEntry entry)
                {
                    return
                        entry.CamelCaseIdentifierName
                            .MemberAccess(
                                IdentifierName(Names.ToCoreArray))
                            .InvokeWithArguments();
                }
            }
        }
    }
}