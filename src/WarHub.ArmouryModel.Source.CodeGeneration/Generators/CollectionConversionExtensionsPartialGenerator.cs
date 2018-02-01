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
            yield return GenerateToCoreArray();
            yield return GenerateToNodeList();
            if (IsAbstract)
            {
                yield break;
            }
            yield return GenerateToImmutableRecursive();
            yield return GenerateToBuildersList();
        }

        private MemberDeclarationSyntax GenerateToCoreArray()
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
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(paramName),
                            GenericName(
                                Identifier(Names.ToCoreArray))
                            .AddTypeArgumentListArguments(
                                Descriptor.CoreType,
                                Descriptor.GetNodeTypeIdentifierName())));
            }
        }

        private MemberDeclarationSyntax GenerateToNodeList()
        {
            const string coresParamName = "cores";
            const string parentParamName = "parent";
            return
                MethodDeclaration(
                    Descriptor.GetNodeTypeIdentifierName().ToNodeListType(),
                    Names.ToNodeList)
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
                yield return
                    Parameter(
                        Identifier(parentParamName))
                    .WithType(
                        IdentifierName(Names.SourceNode));
            }
            ExpressionSyntax CreateReturnExpression()
            {
                return
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(coresParamName),
                            GenericName(
                                Identifier(Names.ToNodeList))
                            .AddTypeArgumentListArguments(
                                Descriptor.GetNodeTypeIdentifierName(),
                                Descriptor.CoreType)))
                    .AddArgumentListArguments(
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
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(buildersParamName),
                            GenericName(
                                Identifier(Names.ToImmutableRecursive))
                            .AddTypeArgumentListArguments(
                                Descriptor.CoreType.ToNestedBuilderType(),
                                Descriptor.CoreType)));
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
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(coresParamName),
                            GenericName(
                                Identifier(Names.ToBuildersList))
                            .AddTypeArgumentListArguments(
                                Descriptor.CoreType,
                                Descriptor.CoreType.ToNestedBuilderType())));
            }
        }
    }
}
