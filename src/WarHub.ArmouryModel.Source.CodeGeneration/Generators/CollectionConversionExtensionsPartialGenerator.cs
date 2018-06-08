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
            yield return GenerateCoreArrayToNodeList();
            yield return GenerateCoreArrayToListNode();
            yield return GenerateNodeListToListNode();
            yield return GenerateToImmutableRecursive();
            yield return GenerateToBuildersList();
        }

        private MemberDeclarationSyntax GenerateListNodeToCoreArray()
        {
            const string list = "list";
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
                        Identifier(list))
                    .AddModifiers(SyntaxKind.ThisKeyword)
                    .WithType(
                        Descriptor.GetListNodeTypeIdentifierName());
            }
            ExpressionSyntax CreateReturnExpression()
            {
                return
                    IdentifierName(list)
                    .MemberAccess(
                        IdentifierName(Names.NodeList))
                    .MemberAccess(
                        IdentifierName(Names.ToCoreArray))
                    .InvokeWithArguments();
            }
        }

        private MemberDeclarationSyntax GenerateNodeListToCoreArray()
        {
            const string nodes = "nodes";
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
                        Identifier(nodes))
                    .AddModifiers(SyntaxKind.ThisKeyword)
                    .WithType(
                        Descriptor.GetNodeTypeIdentifierName().ToNodeListType());
            }
            ExpressionSyntax CreateReturnExpression()
            {
                return
                    IdentifierName(nodes)
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
            const string cores = "cores";
            const string parent = "parent";
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
                        Identifier(cores))
                    .AddModifiers(SyntaxKind.ThisKeyword)
                    .WithType(
                        Descriptor.CoreType.ToImmutableArrayType());
                yield return
                    Parameter(
                        Identifier(parent))
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
                            IdentifierName(cores)),
                        Argument(IdentifierName(parent)));
            }
        }

        private MemberDeclarationSyntax GenerateCoreArrayToNodeList()
        {
            const string cores = "cores";
            const string parent = "parent";
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
                        Identifier(cores))
                    .AddModifiers(SyntaxKind.ThisKeyword)
                    .WithType(
                        Descriptor.CoreType.ToImmutableArrayType());
                yield return
                    Parameter(
                        Identifier(parent))
                    .WithType(
                        IdentifierName(Names.SourceNode));
            }
            ExpressionSyntax CreateToNodeListExpression()
            {
                return
                    IdentifierName(cores)
                    .MemberAccess(
                        GenericName(
                            Identifier(Names.ToNodeList))
                        .AddTypeArgumentListArguments(
                            Descriptor.GetNodeTypeIdentifierName(),
                            Descriptor.CoreType))
                    .InvokeWithArguments(
                            IdentifierName(parent));
            }
        }

        private MemberDeclarationSyntax GenerateNodeListToListNode()
        {
            const string nodes = "nodes";
            const string parent = "parent";
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
                            Identifier(nodes))
                        .AddModifiers(SyntaxKind.ThisKeyword)
                        .WithType(
                            Descriptor.GetNodeTypeIdentifierName().ToNodeListType());
                yield return
                    Parameter(
                            Identifier(parent))
                        .WithType(
                            IdentifierName(Names.SourceNode))
                        .WithDefault(
                            EqualsValueClause(
                                LiteralExpression(SyntaxKind.NullLiteralExpression)));
            }
            ExpressionSyntax CreateReturnExpression()
            {
                return
                    IdentifierName(nodes)
                        .MemberAccess(
                            IdentifierName(Names.ToCoreArray))
                        .InvokeWithArguments()
                        .MemberAccess(
                            IdentifierName(Names.ToListNode))
                        .InvokeWithArguments(
                            IdentifierName(parent));
            }
        }

        private MemberDeclarationSyntax GenerateToImmutableRecursive()
        {
            const string builders = "builders";
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
                        Identifier(builders))
                    .AddModifiers(SyntaxKind.ThisKeyword)
                    .WithType(
                        Descriptor.CoreType.ToListOfBuilderType());
            }
            ExpressionSyntax CreateReturnExpression()
            {
                return
                    IdentifierName(builders)
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
            const string cores = "cores";
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
                        Identifier(cores))
                    .AddModifiers(SyntaxKind.ThisKeyword)
                    .WithType(
                        Descriptor.CoreType.ToImmutableArrayType());
            }
            ExpressionSyntax CreateReturnExpression()
            {
                return
                    IdentifierName(cores)
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
