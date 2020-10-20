using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            return Identifier(Names.NodeFactory);
        }

        protected override IEnumerable<MemberDeclarationSyntax> GenerateMembers()
        {
            return List<MemberDeclarationSyntax>()
                .AddRange(GenerateFactoryMethods());
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateFactoryMethods()
        {
            const string List = "list";
            var methodName = Descriptor.RawModelName;
            yield return CreateForNode(
                CreateNodeListParameter,
                CreateNodeListReturnStatement);
            if (Descriptor.Entries.Any(x => x is CoreListChild))
            {
                yield return CreateForNode(
                    CreateListNodeParameter,
                    CreateListNodeReturnStatement);
            }
            // NodeList parameter
            yield return CreateForList(
                Parameter(
                    Identifier(List))
                .WithType(
                    Descriptor.GetNodeTypeIdentifierName().ToNodeListType()),
                ObjectCreationExpression(
                    Descriptor.GetListNodeTypeIdentifierName())
                .Invoke(
                    IdentifierName(List)
                        .Dot(
                            IdentifierName(Names.ToCoreArray))
                        .Invoke(),
                    LiteralExpression(SyntaxKind.NullLiteralExpression)));
            // params parameter
            yield return CreateForList(
                Parameter(
                    Identifier(List))
                .AddModifiers(SyntaxKind.ParamsKeyword)
                .WithType(
                    Descriptor.GetNodeTypeIdentifierName().ToArrayType()),
                ObjectCreationExpression(
                    Descriptor.GetListNodeTypeIdentifierName())
                .Invoke(
                    IdentifierName(List)
                        .Dot(
                            IdentifierName(Names.ToNodeList))
                        .Invoke()
                        .Dot(
                            IdentifierName(Names.ToCoreArray))
                        .Invoke(),
                    LiteralExpression(SyntaxKind.NullLiteralExpression)));

            static ParameterSyntax CreateListNodeParameter(CoreListChild entry)
            {
                return
                    Parameter(entry.CamelCaseIdentifier)
                        .WithType(entry.GetListNodeTypeIdentifierName());
            }

            static ParameterSyntax CreateNodeListParameter(CoreListChild entry)
            {
                var type = entry.GetNodeTypeIdentifierName().ToNodeListType();
                return
                    Parameter(entry.CamelCaseIdentifier)
                        .WithType(type)
                        .WithDefault(
                            EqualsValueClause(
                                LiteralExpression(SyntaxKind.DefaultLiteralExpression)));
            }
            StatementSyntax CreateListNodeReturnStatement()
            {
                return
                    ReturnStatement(
                        methodName
                        .Invoke(
                            Descriptor.Entries
                            .Where(x => x is not CoreListChild)
                            .Select(e => e.CamelCaseIdentifierName)
                            .Concat(
                                Descriptor.Entries
                                .OfType<CoreListChild>()
                                .Select(CreateCollectionArgument))
                            .ToArray()));

                static ExpressionSyntax CreateCollectionArgument(CoreListChild entry)
                {
                    return
                        entry.CamelCaseIdentifierName
                        .Dot(
                            IdentifierName(Names.NodeList));
                }
            }
            StatementSyntax CreateNodeListReturnStatement()
            {
                return
                    ReturnStatement(
                        Descriptor.CoreType.ObjectCreationWithInitializer(
                            Descriptor.Entries.Select(CreateInitializer)
                            .ToArray())
                        .Dot(
                            IdentifierName(Names.ToNode))
                        .Invoke());

                static ExpressionSyntax CreateInitializer(CoreChildBase entry) => (entry switch
                {
                    CoreObjectChild =>
                        entry.CamelCaseIdentifierName
                        .Dot(
                            IdentifierName(Names.Core)),
                    CoreListChild =>
                        entry.CamelCaseIdentifierName
                        .Dot(
                            IdentifierName(Names.ToCoreArray))
                        .Invoke(),
                    _ => entry.CamelCaseIdentifierName
                })
                .AssignTo(entry.IdentifierName);
            }
            MethodDeclarationSyntax CreateForNode(
                Func<CoreListChild, ParameterSyntax> createCollectionParameter,
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
                        .Where(x => x is not CoreListChild)
                        .Select(CreateSimpleParameter, CreateComplexParameter, null!)
                        .Concat(
                            Descriptor.Entries
                            .OfType<CoreListChild>()
                            .Select(createCollectionParameter));
                }

                static ParameterSyntax CreateSimpleParameter(CoreValueChild entry)
                {
                    return
                        Parameter(entry.CamelCaseIdentifier)
                        .WithType(entry.Type);
                }

                static ParameterSyntax CreateComplexParameter(CoreObjectChild entry)
                {
                    var type = entry.GetNodeTypeIdentifierName();
                    return
                        Parameter(entry.CamelCaseIdentifier)
                        .WithType(type);
                }
            }
            MethodDeclarationSyntax CreateForList(ParameterSyntax parameter, ExpressionSyntax returnExpression)
            {
                return
                    MethodDeclaration(
                        Descriptor.GetListNodeTypeIdentifierName(),
                        Descriptor.RawModelName + Names.ListSuffix)
                    .AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
                    .AddParameterListParameters(parameter)
                    .AddBodyStatements(
                        ReturnStatement(returnExpression));
            }
        }
    }
}
