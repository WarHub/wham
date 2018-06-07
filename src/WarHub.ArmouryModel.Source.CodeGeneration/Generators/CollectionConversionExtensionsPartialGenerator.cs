using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class CollectionConversionExtensionsPartialGenerator : ModelExtensionsPartialGeneratorBase
    {
        protected CollectionConversionExtensionsPartialGenerator(CoreDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        public static TypeDeclarationSyntax Generate(CoreDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new CollectionConversionExtensionsPartialGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        protected override IEnumerable<MemberDeclarationSyntax> GenerateMembers()
        {
            if (!IsAbstract)
            {
                yield return GenerateListNodeToCoreArray();
            }
            yield return GenerateNodeListToCoreArray();
            if (IsAbstract)
            {
                yield break;
            }
            yield return GenerateCoreArrayToListNode();
            yield return GenerateNodeListToListNode();
            yield return GenerateToImmutableRecursive();
            yield return GenerateToBuildersList();
        }

        private MemberDeclarationSyntax GenerateListNodeToCoreArray()
        {
            const string paramName = "list";
            return
                MethodDeclaration(
                    Descriptor.CoreType.ToImmutableArrayType(),
                    Names.ToCoreArray)
                .AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
                .AddParameterListParameters(
                    CreateParameters())
                .AddBodyStatements(
                    ReturnStatement(
                        CreateReturnExpression()));
            IEnumerable<ParameterSyntax> CreateParameters()
            {
                yield return
                    Parameter(
                        Identifier(paramName))
                    .AddModifiers(SyntaxKind.ThisKeyword)
                    .WithType(
                        Descriptor.GetListNodeTypeIdentifierName());
            }
            ExpressionSyntax CreateReturnExpression()
            {
                return
                    IdentifierName(paramName)
                    .MemberAccess(
                        IdentifierName(Names.NodeList))
                    .MemberAccess(
                        IdentifierName(Names.ToCoreArray))
                    .InvokeWithArguments();
            }
        }

        private MemberDeclarationSyntax GenerateNodeListToCoreArray()
        {
            const string paramName = "nodes";
            return
                MethodDeclaration(
                    Descriptor.CoreType.ToImmutableArrayType(),
                    Names.ToCoreArray)
                .AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
                .AddParameterListParameters(
                    CreateParameters())
                .AddBodyStatements(
                    ReturnStatement(
                        CreateReturnExpression()));
            IEnumerable<ParameterSyntax> CreateParameters()
            {
                yield return
                    Parameter(
                        Identifier(paramName))
                    .AddModifiers(SyntaxKind.ThisKeyword)
                    .WithType(
                        Descriptor.GetNodeTypeIdentifierName().ToNodeListType());
            }
            ExpressionSyntax CreateReturnExpression()
            {
                return
                    IdentifierName(paramName)
                    .MemberAccess(
                        GenericName(
                            Identifier(Names.ToCoreArray))
                        .AddTypeArgumentListArguments(
                            Descriptor.CoreType,
                            Descriptor.GetNodeTypeIdentifierName()))
                    .InvokeWithArguments();
            }
        }

        private MemberDeclarationSyntax GenerateCoreArrayToListNode()
        {
            const string coresParamName = "cores";
            const string parentParamName = "parent";
            return
                MethodDeclaration(
                    Descriptor.GetListNodeTypeIdentifierName(),
                    Names.ToListNode)
                .AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
                .AddParameterListParameters(
                    CreateParameters())
                .AddBodyStatements(
                    ReturnStatement(
                        CreateListNodeCreationExpression()));
            IEnumerable<ParameterSyntax> CreateParameters()
            {
                yield return
                    Parameter(
                        Identifier(coresParamName))
                    .AddModifiers(SyntaxKind.ThisKeyword)
                    .WithType(
                        Descriptor.CoreType.ToImmutableArrayType());
                yield return
                    Parameter(
                        Identifier(parentParamName))
                    .WithType(
                        IdentifierName(Names.SourceNode));
            }
            ExpressionSyntax CreateListNodeCreationExpression()
            {
                return
                    ObjectCreationExpression(
                        Descriptor.GetListNodeTypeIdentifierName())
                    .AddArgumentListArguments(
                        Argument(
                            CreateToNodeListExpression()),
                        Argument(IdentifierName(parentParamName)));
            }
            ExpressionSyntax CreateToNodeListExpression()
            {
                return
                    IdentifierName(coresParamName)
                    .MemberAccess(
                        GenericName(
                            Identifier(Names.ToNodeList))
                        .AddTypeArgumentListArguments(
                            Descriptor.GetNodeTypeIdentifierName(),
                            Descriptor.CoreType))
                    .InvokeWithArguments(
                            IdentifierName(parentParamName));
            }
        }

        private MemberDeclarationSyntax GenerateNodeListToListNode()
        {
            const string nodesParamName = "nodes";
            const string parentParamName = "parent";
            return
                MethodDeclaration(
                        Descriptor.GetListNodeTypeIdentifierName(),
                        Names.ToListNode)
                    .AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
                    .AddParameterListParameters(
                        CreateParameters())
                    .AddBodyStatements(
                        ReturnStatement(
                            CreateReturnExpression()));
            IEnumerable<ParameterSyntax> CreateParameters()
            {
                yield return
                    Parameter(
                            Identifier(nodesParamName))
                        .AddModifiers(SyntaxKind.ThisKeyword)
                        .WithType(
                            Descriptor.GetNodeTypeIdentifierName().ToNodeListType());
                yield return
                    Parameter(
                            Identifier(parentParamName))
                        .WithType(
                            IdentifierName(Names.SourceNode))
                        .WithDefault(
                            EqualsValueClause(
                                LiteralExpression(SyntaxKind.NullLiteralExpression)));
            }
            ExpressionSyntax CreateReturnExpression()
            {
                return
                    ObjectCreationExpression(
                            Descriptor.GetListNodeTypeIdentifierName())
                        .AddArgumentListArguments(
                            Argument(
                                IdentifierName(nodesParamName)),
                            Argument(
                                IdentifierName(parentParamName)));
            }
        }

        private MemberDeclarationSyntax GenerateToImmutableRecursive()
        {
            const string buildersParamName = "builders";
            return
                MethodDeclaration(
                    Descriptor.CoreType.ToImmutableArrayType(),
                    Names.ToImmutableRecursive)
                .AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
                .AddParameterListParameters(
                    CreateParameters())
                .AddBodyStatements(
                    ReturnStatement(
                        CreateReturnExpression()));
            IEnumerable<ParameterSyntax> CreateParameters()
            {
                yield return
                    Parameter(
                        Identifier(buildersParamName))
                    .AddModifiers(SyntaxKind.ThisKeyword)
                    .WithType(
                        Descriptor.CoreType.ToListOfBuilderType());
            }
            ExpressionSyntax CreateReturnExpression()
            {
                return
                    IdentifierName(buildersParamName)
                    .MemberAccess(
                        GenericName(
                            Identifier(Names.ToImmutableRecursive))
                        .AddTypeArgumentListArguments(
                            Descriptor.CoreType.ToNestedBuilderType(),
                            Descriptor.CoreType))
                    .InvokeWithArguments();
            }
        }

        private MemberDeclarationSyntax GenerateToBuildersList()
        {
            const string coresParamName = "cores";
            return
                MethodDeclaration(
                    Descriptor.CoreType.ToListOfBuilderType(),
                    Names.ToBuildersList)
                .AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
                .AddParameterListParameters(
                    CreateParameters())
                .AddBodyStatements(
                    ReturnStatement(
                        CreateReturnExpression()));
            IEnumerable<ParameterSyntax> CreateParameters()
            {
                yield return
                    Parameter(
                        Identifier(coresParamName))
                    .AddModifiers(SyntaxKind.ThisKeyword)
                    .WithType(
                        Descriptor.CoreType.ToImmutableArrayType());
            }
            ExpressionSyntax CreateReturnExpression()
            {
                return
                    IdentifierName(coresParamName)
                    .MemberAccess(
                        GenericName(
                            Identifier(Names.ToBuildersList))
                        .AddTypeArgumentListArguments(
                            Descriptor.CoreType,
                            Descriptor.CoreType.ToNestedBuilderType()))
                    .InvokeWithArguments();
            }
        }
    }
}
