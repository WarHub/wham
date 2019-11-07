using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            const string NodesParamName = "nodes";
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
                            Identifier(NodesParamName))
                        .AddModifiers(SyntaxKind.ParamsKeyword)
                        .WithType(
                            entryNodeType.ToArrayType());
                }

                static ExpressionSyntax CreateNodesExpression()
                {
                    return
                        IdentifierName(NodesParamName)
                        .MemberAccess(
                            IdentifierName(Names.ToNodeList))
                        .InvokeWithArguments();
                }
            }
            MethodDeclarationSyntax CreateAddParams()
            {
                return CreateMutator(Names.Add, CreateParameters(), CreateNodesExpression());
                IEnumerable<ParameterSyntax> CreateParameters()
                {
                    yield return
                        Parameter(
                            Identifier(NodesParamName))
                        .AddModifiers(SyntaxKind.ParamsKeyword)
                        .WithType(
                            entryNodeType.ToArrayType());
                }
                ExpressionSyntax CreateNodesExpression()
                {
                    return
                        entry.IdentifierName
                        .MemberAccess(
                            IdentifierName(Names.NodeList))
                        .MemberAccess(
                            IdentifierName(Names.AddRange))
                        .InvokeWithArguments(
                            IdentifierName(NodesParamName));
                }
            }
            MethodDeclarationSyntax CreateAddEnumerable()
            {
                return CreateMutator(Names.Add, CreateParameters(), CreateNodesExpression());
                IEnumerable<ParameterSyntax> CreateParameters()
                {
                    yield return
                        Parameter(
                            Identifier(NodesParamName))
                        .WithType(
                            entryNodeType.ToIEnumerableType());
                }
                ExpressionSyntax CreateNodesExpression()
                {
                    return
                        entry.IdentifierName
                        .MemberAccess(
                            IdentifierName(Names.NodeList))
                        .MemberAccess(
                            IdentifierName(Names.AddRange))
                        .InvokeWithArguments(
                            IdentifierName(NodesParamName));
                }
            }
            MethodDeclarationSyntax CreateMutator(string prefix, IEnumerable<ParameterSyntax> parameters, ExpressionSyntax nodesExpression)
            {
                return
                    MethodDeclaration(nodeType, prefix + entry.Identifier)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .MutateIf(Descriptor.DerivedEntries.Contains(entry), x => x.AddModifiers(SyntaxKind.NewKeyword))
                    .AddParameterListParameters(parameters)
                    .AddBodyStatements(
                        ReturnStatement(
                            IdentifierName(Names.WithPrefix + entry.Identifier)
                            .InvokeWithArguments(nodesExpression)));
            }
        }
    }
}
