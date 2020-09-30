using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class NodeExtensionsGenerator : PartialGeneratorBase
    {
        public static readonly SyntaxToken NodeExtensionsIdentifier = Identifier("NodeExtensions");
        public static readonly SyntaxToken ThisParameterToken = Identifier("@this");
        public static readonly IdentifierNameSyntax ThisParameterSyntax = IdentifierName(ThisParameterToken);

        protected NodeExtensionsGenerator(CoreDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        public static TypeDeclarationSyntax Generate(CoreDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new NodeExtensionsGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        protected override SyntaxToken GenerateTypeIdentifier() => NodeExtensionsIdentifier;

        protected override SyntaxTokenList GenerateModifiers()
        {
            return
                TokenList(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.StaticKeyword),
                    Token(SyntaxKind.PartialKeyword)
                );
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
            yield return CreateWithNodeList();
            yield return CreateWithParams();
            yield return CreateAddEnumerable();
            yield return CreateAddParams();
            MethodDeclarationSyntax CreateWithNodeList()
            {
                var parameter =
                    Parameter(
                        Identifier(NodesParamName))
                    .WithType(
                        entry.GetNodeTypeIdentifierName().ToNodeListType());
                var withArg =
                    IdentifierName(NodesParamName)
                    .MemberAccess(
                        IdentifierName(Names.ToListNode))
                    .InvokeWithArguments();
                return CreateMutator(Names.WithPrefix, parameter, withArg);
            }
            MethodDeclarationSyntax CreateWithParams()
            {
                var parameter =
                    Parameter(
                        Identifier(NodesParamName))
                    .AddModifiers(SyntaxKind.ParamsKeyword)
                    .WithType(
                        entryNodeType.ToArrayType());
                var withArg =
                    IdentifierName(NodesParamName)
                    .MemberAccess(
                        IdentifierName(Names.ToNodeList))
                    .InvokeWithArguments();
                return CreateMutator(Names.WithPrefix, parameter, withArg);
            }
            MethodDeclarationSyntax CreateAddParams()
            {
                var parameter =
                    Parameter(
                        Identifier(NodesParamName))
                    .AddModifiers(SyntaxKind.ParamsKeyword)
                    .WithType(
                        entryNodeType.ToArrayType());
                var withArg =
                    ThisParameterSyntax
                    .MemberAccess(entry.IdentifierName)
                    .MemberAccess(
                        IdentifierName(Names.NodeList))
                    .MemberAccess(
                        IdentifierName(Names.AddRange))
                    .InvokeWithArguments(
                        IdentifierName(NodesParamName));
                return CreateMutator(Names.Add, parameter, withArg);
            }
            MethodDeclarationSyntax CreateAddEnumerable()
            {
                var parameter =
                    Parameter(
                        Identifier(NodesParamName))
                    .WithType(
                        entryNodeType.ToIEnumerableType());
                var withArg =
                    ThisParameterSyntax
                    .MemberAccess(entry.IdentifierName)
                    .MemberAccess(
                        IdentifierName(Names.NodeList))
                    .MemberAccess(
                        IdentifierName(Names.AddRange))
                    .InvokeWithArguments(
                        IdentifierName(NodesParamName));
                return CreateMutator(Names.Add, parameter, withArg);
            }
            MethodDeclarationSyntax CreateMutator(string prefix, ParameterSyntax parameter, ExpressionSyntax nodesExpression)
            {
                return
                    MethodDeclaration(nodeType, prefix + entry.Identifier)
                    .AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
                    .AddParameterListParameters(
                        Parameter(ThisParameterToken)
                        .AddModifiers(SyntaxKind.ThisKeyword)
                        .WithType(Descriptor.GetNodeTypeIdentifierName()),
                        parameter)
                    .AddBodyStatements(
                        ReturnStatement(
                            ThisParameterSyntax
                            .MemberAccess(
                                IdentifierName(Names.WithPrefix + entry.Identifier))
                            .InvokeWithArguments(nodesExpression)));
            }
        }
    }
}
