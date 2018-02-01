using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class NodeCollectionConvenienceMethodsGenerator : NodePartialGeneratorBase
    {
        protected NodeCollectionConvenienceMethodsGenerator(CoreDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        public static TypeDeclarationSyntax Generate(CoreDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new NodeCollectionConvenienceMethodsGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        protected override IEnumerable<MemberDeclarationSyntax> GenerateMembers()
        {
            return Descriptor.Entries
                .OfType<CoreDescriptor.CollectionEntry>()
                .SelectMany(GenerateConvenienceMutators);
        }

        private IEnumerable<MethodDeclarationSyntax> GenerateConvenienceMutators(CoreDescriptor.CollectionEntry entry)
        {
            const string nodesParamName = "nodes";
            var nodeType = Descriptor.GetNodeTypeIdentifierName();
            var entryNodeType = entry.GetNodeTypeIdentifierName();
            yield return CreateWithParams();
            yield return CreateAddEnumerable();
            yield return CreateAddParams();
            MethodDeclarationSyntax CreateWithParams()
            {
                return CreateMutator(Names.WithPrefix, CreateParameters(), CreateNodesExpression());
                IEnumerable<ParameterSyntax> CreateParameters()
                {
                    yield return
                        Parameter(
                            Identifier(nodesParamName))
                        .AddModifiers(SyntaxKind.ParamsKeyword)
                        .WithType(
                            ArrayType(entryNodeType)
                            .AddRankSpecifiers(
                                ArrayRankSpecifier()));
                }
                ExpressionSyntax CreateNodesExpression()
                {
                    return
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(nodesParamName),
                                IdentifierName(Names.ToNodeList)));
                }
            }
            MethodDeclarationSyntax CreateAddParams()
            {
                return CreateMutator(Names.Add, CreateParameters(), CreateNodesExpression());
                IEnumerable<ParameterSyntax> CreateParameters()
                {
                    yield return
                        Parameter(
                            Identifier(nodesParamName))
                        .AddModifiers(SyntaxKind.ParamsKeyword)
                        .WithType(
                            ArrayType(entryNodeType)
                            .AddRankSpecifiers(
                                ArrayRankSpecifier()));
                }
                ExpressionSyntax CreateNodesExpression()
                {
                    return
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                entry.IdentifierName,
                                IdentifierName(Names.AddRange)))
                        .AddArgumentListArguments(
                            Argument(
                                IdentifierName(nodesParamName)));
                }
            }
            MethodDeclarationSyntax CreateAddEnumerable()
            {
                return CreateMutator(Names.Add, CreateParameters(), CreateNodesExpression());
                IEnumerable<ParameterSyntax> CreateParameters()
                {
                    yield return
                        Parameter(
                            Identifier(nodesParamName))
                        .WithType(
                            entryNodeType.ToIEnumerableType());
                }
                ExpressionSyntax CreateNodesExpression()
                {
                    return
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                entry.IdentifierName,
                                IdentifierName(Names.AddRange)),
                            ArgumentList(
                                SingletonSeparatedList(
                                    Argument(
                                        IdentifierName(nodesParamName)))));
                }
            }
            MethodDeclarationSyntax CreateMutator(string prefix, IEnumerable<ParameterSyntax> parameters, ExpressionSyntax nodesExpression)
            {
                return
                    MethodDeclaration(nodeType, prefix + entry.Identifier)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .MutateIf(IsDerived, x => x.AddModifiers(SyntaxKind.NewKeyword))
                    .AddParameterListParameters(parameters)
                    .AddBodyStatements(
                        ReturnStatement(
                            InvocationExpression(
                                IdentifierName(Names.WithPrefix + entry.Identifier))
                            .AddArgumentListArguments(
                                Argument(nodesExpression))));
            }
        }
    }
}
