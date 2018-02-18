using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class CoreToNodeMethodsCorePartialGenerator : CorePartialGeneratorBase
    {
        protected CoreToNodeMethodsCorePartialGenerator(CoreDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        public static TypeDeclarationSyntax Generate(CoreDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new CoreToNodeMethodsCorePartialGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        protected override IEnumerable<MemberDeclarationSyntax> GenerateMembers()
        {
            return GetToNodeMethods();
        }

        protected override IEnumerable<BaseTypeSyntax> GenerateBaseTypes()
        {
            if (!IsDerived)
            {
                yield return
                    SimpleBaseType(
                        IdentifierName(Names.NodeCore));
            }
            yield return
                SimpleBaseType(
                    GenericName(
                        Identifier(Names.ICore))
                    .AddTypeArgumentListArguments(
                        Descriptor.GetNodeTypeIdentifierName()));
        }

        private IEnumerable<MethodDeclarationSyntax> GetToNodeMethods()
        {
            const string parentLocal = "parent";
            var nodeTypeIdentifierName = Descriptor.GetNodeTypeIdentifierName();
            var derivedToNodeName = (IsDerived ? BaseType.Name : "") + Names.ToNode;
            var abstractToNodeName = Descriptor.CoreTypeIdentifier.Text + Names.ToNode;
            var thisToNodeName = IsAbstract ? abstractToNodeName : Names.ToNode;
            if (IsDerived)
            {
                yield return DerivedToNodeMethod();
            }
            if (!IsAbstract)
            {
                yield return ToNodeCoreMethod();
            }
            if (IsAbstract)
            {
                yield return AbstractToNodeMethod();
            }
            yield return ToNodeMethod();
            yield return ToNodeExplicitInterfaceMethod();
            MethodDeclarationSyntax DerivedToNodeMethod()
            {
                return
                    MethodDeclaration(
                        IdentifierName(BaseType.Name.GetNodeTypeNameCore()),
                        derivedToNodeName)
                    .AddModifiers(SyntaxKind.ProtectedKeyword, SyntaxKind.SealedKeyword, SyntaxKind.OverrideKeyword)
                    .Mutate(AddToNodeParameters)
                    .Mutate(x => ForwardExpressionToNode(x, Names.ToNode));
            }
            MethodDeclarationSyntax ToNodeCoreMethod()
            {
                return
                    MethodDeclaration(
                        IdentifierName(Names.SourceNode),
                        Names.ToNodeCore)
                    .AddModifiers(SyntaxKind.ProtectedKeyword, SyntaxKind.SealedKeyword, SyntaxKind.OverrideKeyword)
                    .AddParameterListParameters(
                        Parameter(
                            Identifier(parentLocal))
                        .WithType(
                            IdentifierName(Names.SourceNode)))
                    .Mutate(x => ForwardExpressionToNode(x, Names.ToNode));
            }
            MethodDeclarationSyntax AbstractToNodeMethod()
            {
                return
                    MethodDeclaration(nodeTypeIdentifierName, abstractToNodeName)
                    .AddModifiers(SyntaxKind.ProtectedKeyword, SyntaxKind.AbstractKeyword)
                    .Mutate(AddToNodeParameters)
                    .WithSemicolonTokenDefault();
            }
            MethodDeclarationSyntax ToNodeMethod()
            {
                return
                    MethodDeclaration(nodeTypeIdentifierName, Names.ToNode)
                    .AddModifiers(
                        SyntaxKind.PublicKeyword,
                        SyntaxKind.NewKeyword)
                    .Mutate(AddToNodeParameters)
                    .MutateIf(
                        IsAbstract,
                        x => ForwardExpressionToNode(x, abstractToNodeName),
                        x => x
                        .WithExpressionBodyFull(
                            ObjectCreationExpression(nodeTypeIdentifierName)
                            .AddArgumentListArguments(
                                Argument(
                                    ThisExpression()),
                                Argument(
                                    IdentifierName(parentLocal)))));
            }
            MethodDeclarationSyntax ToNodeExplicitInterfaceMethod()
            {
                return
                    MethodDeclaration(nodeTypeIdentifierName, Names.ToNode)
                    .WithExplicitInterfaceSpecifier(
                        ExplicitInterfaceSpecifier(
                            GenericName(
                                Identifier(Names.ICore))
                            .AddTypeArgumentListArguments(nodeTypeIdentifierName)))
                    .AddParameterListParameters(
                        Parameter(
                            Identifier(parentLocal))
                        .WithType(
                            IdentifierName(Names.SourceNode)))
                    .Mutate(x => ForwardExpressionToNode(x, Names.ToNode));
            }
            MethodDeclarationSyntax AddToNodeParameters(MethodDeclarationSyntax method)
            {
                return method
                    .AddParameterListParameters(
                        Parameter(
                            Identifier(parentLocal))
                        .WithType(
                            IdentifierName(Names.SourceNode))
                        .WithDefault(
                            EqualsValueClause(
                                LiteralExpression(SyntaxKind.NullLiteralExpression))));
            }
            MethodDeclarationSyntax ForwardExpressionToNode(MethodDeclarationSyntax method, string targetName)
            {
                return
                    method
                    .WithExpressionBodyFull(
                        IdentifierName(targetName)
                        .InvokeWithArguments(
                            IdentifierName(parentLocal)));
            }
        }
    }
}
