using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class NodeFactoryPartialGenerator : PartialGeneratorBase
    {
        protected NodeFactoryPartialGenerator(CoreDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        public static TypeDeclarationSyntax Generate(CoreDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new NodeFactoryPartialGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        protected override SyntaxToken GenerateTypeIdentifier()
        {
            return Identifier("NodeFactory");
        }

        protected override IEnumerable<MemberDeclarationSyntax> GenerateMembers()
        {
            return List<MemberDeclarationSyntax>()
                .AddRange(GenerateFactoryMethods());
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateFactoryMethods()
        {
            string methodName = Descriptor.RawModelName;
            if (Descriptor.Entries.Any(x => x.IsCollection))
            {
                yield return CreateFull(
                    CreateListNodeParameter,
                    CreateListNodeReturnStatement);
            }
            yield return CreateFull(
                CreateNodeListParameter,
                CreateNodeListReturnStatement);
            ParameterSyntax CreateListNodeParameter(CoreDescriptor.CollectionEntry entry)
            {
                var type = entry.GetListNodeTypeIdentifierName();
                return
                    Parameter(entry.CamelCaseIdentifier)
                    .WithType(type)
                    .WithDefault(
                        EqualsValueClause(
                            DefaultExpression(type)));
            }
            ParameterSyntax CreateNodeListParameter(CoreDescriptor.CollectionEntry entry)
            {
                return
                    Parameter(entry.CamelCaseIdentifier)
                    .WithType(
                        entry.GetNodeTypeIdentifierName()
                        .ToNodeListType());
            }
            StatementSyntax CreateListNodeReturnStatement()
            {
                return
                    ReturnStatement(
                        IdentifierName(methodName)
                        .InvokeWithArguments(
                            Descriptor.Entries
                            .Where(x => !x.IsCollection)
                            .Select(e => e.CamelCaseIdentifierName)
                            .Cast<ExpressionSyntax>()
                            .Concat(
                                Descriptor.Entries
                                .OfType<CoreDescriptor.CollectionEntry>()
                                .Select(CreateCollectionArgument))));
                ExpressionSyntax CreateCollectionArgument(CoreDescriptor.CollectionEntry entry)
                {
                    return
                        entry.CamelCaseIdentifierName
                        .ConditionalMemberAccess(
                            IdentifierName(Names.NodeList))
                        .Coalesce(
                            DefaultExpression(
                                entry.GetNodeTypeIdentifierName().ToNodeListType()));
                }
            }
            StatementSyntax CreateNodeListReturnStatement()
            {
                return
                    ReturnStatement(
                        CreateObjectCreation()
                        .MemberAccess(
                            IdentifierName(Names.ToNode))
                        .InvokeWithArguments());
                ExpressionSyntax CreateObjectCreation()
                {
                    return
                        ObjectCreationExpression(Descriptor.CoreType)
                        .AddArgumentListArguments(
                            Descriptor.Entries.Select(
                                CreateSimpleArgument,
                                CreateComplexArgument,
                                CreateCollectionArgument));
                    ArgumentSyntax CreateSimpleArgument(CoreDescriptor.Entry entry)
                    {
                        return Argument(entry.CamelCaseIdentifierName);
                    }
                    ArgumentSyntax CreateComplexArgument(CoreDescriptor.Entry entry)
                    {
                        var argName = entry.CamelCaseIdentifierName;
                        return
                            Argument(
                                argName.ConditionalMemberAccess(IdentifierName(Names.Core)));
                    }
                    ArgumentSyntax CreateCollectionArgument(CoreDescriptor.CollectionEntry entry)
                    {
                        var argName = entry.CamelCaseIdentifierName;
                        return
                            Argument(
                                argName
                                .MemberAccess(IdentifierName(Names.ToCoreArray))
                                .InvokeWithArguments());
                    }
                }
            }
            MethodDeclarationSyntax CreateFull(
                Func<CoreDescriptor.CollectionEntry, ParameterSyntax> createCollectionParameter,
                Func<StatementSyntax> createReturnStatement)
            {
                return
                    MethodDeclaration(
                        Descriptor.GetNodeTypeIdentifierName(),
                        methodName)
                    .AddModifiers(
                        SyntaxKind.PublicKeyword,
                        SyntaxKind.StaticKeyword)
                    .AddParameterListParameters(
                        CreateParameters())
                    .AddBodyStatements(
                        createReturnStatement());
                IEnumerable<ParameterSyntax> CreateParameters()
                {
                    return
                        Descriptor.Entries
                        .Where(x => !x.IsCollection)
                        .Select(CreateSimpleParameter, CreateComplexParameter, null)
                        .Concat(
                            Descriptor.Entries
                            .OfType<CoreDescriptor.CollectionEntry>()
                            .Select(createCollectionParameter));
                }
                ParameterSyntax CreateSimpleParameter(CoreDescriptor.SimpleEntry entry)
                {
                    return
                        Parameter(entry.CamelCaseIdentifier)
                        .WithType(entry.Type);
                }
                ParameterSyntax CreateComplexParameter(CoreDescriptor.ComplexEntry entry)
                {
                    var type = entry.GetNodeTypeIdentifierName();
                    return
                        Parameter(entry.CamelCaseIdentifier)
                        .WithType(type);
                }
            }
        }
    }
}
