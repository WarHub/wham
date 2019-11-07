﻿using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            yield return GenerateCoreArrayToNodeList();
            yield return GenerateCoreArrayToListNode();
            yield return GenerateNodeListToListNode();
            yield return GenerateToImmutableRecursive();
            yield return GenerateToBuildersList();
        }

        private MemberDeclarationSyntax GenerateListNodeToCoreArray()
        {
            const string List = "list";
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
                        Identifier(List))
                    .AddModifiers(SyntaxKind.ThisKeyword)
                    .WithType(
                        Descriptor.GetListNodeTypeIdentifierName());
            }

            static ExpressionSyntax CreateReturnExpression()
            {
                return
                    IdentifierName(List)
                    .MemberAccess(
                        IdentifierName(Names.NodeList))
                    .MemberAccess(
                        IdentifierName(Names.ToCoreArray))
                    .InvokeWithArguments();
            }
        }

        private MemberDeclarationSyntax GenerateNodeListToCoreArray()
        {
            const string Nodes = "nodes";
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
                        Identifier(Nodes))
                    .AddModifiers(SyntaxKind.ThisKeyword)
                    .WithType(
                        Descriptor.GetNodeTypeIdentifierName().ToNodeListType());
            }
            ExpressionSyntax CreateReturnExpression()
            {
                return
                    IdentifierName(Nodes)
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
            const string Cores = "cores";
            const string Parent = "parent";
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
                        Identifier(Cores))
                    .AddModifiers(SyntaxKind.ThisKeyword)
                    .WithType(
                        Descriptor.CoreType.ToImmutableArrayType());
                yield return
                    Parameter(
                        Identifier(Parent))
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
                            IdentifierName(Cores)),
                        Argument(IdentifierName(Parent)));
            }
        }

        private MemberDeclarationSyntax GenerateCoreArrayToNodeList()
        {
            const string Cores = "cores";
            const string Parent = "parent";
            return
                MethodDeclaration(
                    Descriptor.GetNodeTypeIdentifierName().ToNodeListType(),
                    Names.ToNodeList)
                .AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
                .AddParameterListParameters(
                    CreateParameters())
                .AddBodyStatements(
                    ReturnStatement(
                        CreateToNodeListExpression()));
            IEnumerable<ParameterSyntax> CreateParameters()
            {
                yield return
                    Parameter(
                        Identifier(Cores))
                    .AddModifiers(SyntaxKind.ThisKeyword)
                    .WithType(
                        Descriptor.CoreType.ToImmutableArrayType());
                yield return
                    Parameter(
                        Identifier(Parent))
                    .WithType(
                        IdentifierName(Names.SourceNode));
            }
            ExpressionSyntax CreateToNodeListExpression()
            {
                return
                    IdentifierName(Cores)
                    .MemberAccess(
                        GenericName(
                            Identifier(Names.ToNodeList))
                        .AddTypeArgumentListArguments(
                            Descriptor.GetNodeTypeIdentifierName(),
                            Descriptor.CoreType))
                    .InvokeWithArguments(
                            IdentifierName(Parent));
            }
        }

        private MemberDeclarationSyntax GenerateNodeListToListNode()
        {
            const string Nodes = "nodes";
            const string Parent = "parent";
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
                            Identifier(Nodes))
                        .AddModifiers(SyntaxKind.ThisKeyword)
                        .WithType(
                            Descriptor.GetNodeTypeIdentifierName().ToNodeListType());
                yield return
                    Parameter(
                            Identifier(Parent))
                        .WithType(
                            IdentifierName(Names.SourceNode))
                        .WithDefault(
                            EqualsValueClause(
                                LiteralExpression(SyntaxKind.NullLiteralExpression)));
            }

            static ExpressionSyntax CreateReturnExpression()
            {
                return
                    IdentifierName(Nodes)
                        .MemberAccess(
                            IdentifierName(Names.ToCoreArray))
                        .InvokeWithArguments()
                        .MemberAccess(
                            IdentifierName(Names.ToListNode))
                        .InvokeWithArguments(
                            IdentifierName(Parent));
            }
        }

        private MemberDeclarationSyntax GenerateToImmutableRecursive()
        {
            const string Builders = "builders";
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
                        Identifier(Builders))
                    .AddModifiers(SyntaxKind.ThisKeyword)
                    .WithType(
                        Descriptor.CoreType.ToListOfBuilderType());
            }
            ExpressionSyntax CreateReturnExpression()
            {
                return
                    IdentifierName(Builders)
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
            const string Cores = "cores";
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
                        Identifier(Cores))
                    .AddModifiers(SyntaxKind.ThisKeyword)
                    .WithType(
                        Descriptor.CoreType.ToImmutableArrayType());
            }
            ExpressionSyntax CreateReturnExpression()
            {
                return
                    IdentifierName(Cores)
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
