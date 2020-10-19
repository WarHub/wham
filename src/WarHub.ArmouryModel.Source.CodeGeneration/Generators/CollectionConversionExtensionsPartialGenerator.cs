using System.Collections.Generic;
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
                    Descriptor.ImmutableArrayOfCoreType,
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
                    .Dot(
                        IdentifierName(Names.NodeList))
                    .Dot(
                        IdentifierName(Names.ToCoreArray))
                    .Invoke();
            }
        }

        private MemberDeclarationSyntax GenerateNodeListToCoreArray()
        {
            const string Nodes = "nodes";
            return
                MethodDeclaration(
                    Descriptor.ImmutableArrayOfCoreType,
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
                    .Dot(
                        GenericName(
                            Identifier(Names.ToCoreArray))
                        .AddTypeArgumentListArguments(
                            Descriptor.CoreType,
                            Descriptor.GetNodeTypeIdentifierName()))
                    .Invoke();
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
                        Descriptor.ImmutableArrayOfCoreType);
                yield return
                    Parameter(
                        Identifier(Parent))
                    .WithType(
                        NullableType(
                            IdentifierName(Names.SourceNode)));
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
                        Descriptor.ImmutableArrayOfCoreType);
                yield return
                    Parameter(
                        Identifier(Parent))
                    .WithType(
                        NullableType(
                            IdentifierName(Names.SourceNode)));
            }
            ExpressionSyntax CreateToNodeListExpression()
            {
                return
                    IdentifierName(Cores)
                    .Dot(
                        GenericName(
                            Identifier(Names.ToNodeList))
                        .AddTypeArgumentListArguments(
                            Descriptor.GetNodeTypeIdentifierName(),
                            Descriptor.CoreType))
                    .Invoke(
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
                            NullableType(
                                IdentifierName(Names.SourceNode)))
                        .WithDefault(
                            EqualsValueClause(
                                LiteralExpression(SyntaxKind.NullLiteralExpression)));
            }

            static ExpressionSyntax CreateReturnExpression()
            {
                return
                    IdentifierName(Nodes)
                        .Dot(
                            IdentifierName(Names.ToCoreArray))
                        .Invoke()
                        .Dot(
                            IdentifierName(Names.ToListNode))
                        .Invoke(
                            IdentifierName(Parent));
            }
        }

        private MemberDeclarationSyntax GenerateToImmutableRecursive()
        {
            const string Builders = "builders";
            return
                MethodDeclaration(
                    Descriptor.ImmutableArrayOfCoreType,
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
                        Descriptor.ListOfCoreBuilderType);
            }
            ExpressionSyntax CreateReturnExpression()
            {
                return
                    IdentifierName(Builders)
                    .Dot(
                        GenericName(
                            Identifier(Names.ToImmutableRecursive))
                        .AddTypeArgumentListArguments(
                            Descriptor.CoreBuilderType,
                            Descriptor.CoreType))
                    .Invoke();
            }
        }

        private MemberDeclarationSyntax GenerateToBuildersList()
        {
            const string Cores = "cores";
            return
                MethodDeclaration(
                    Descriptor.ListOfCoreBuilderType,
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
                        Descriptor.ImmutableArrayOfCoreType);
            }
            ExpressionSyntax CreateReturnExpression()
            {
                return
                    IdentifierName(Cores)
                    .Dot(
                        GenericName(
                            Identifier(Names.ToBuildersList))
                        .AddTypeArgumentListArguments(
                            Descriptor.CoreType,
                            Descriptor.CoreBuilderType))
                    .Invoke();
            }
        }
    }
}
