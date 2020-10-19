using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            const string ParentLocal = "parent";
            var nodeTypeIdentifierName = Descriptor.GetNodeTypeIdentifierName();
            var parentParameterBase =
                Parameter(
                    Identifier(ParentLocal))
                .WithType(
                    NullableType(
                        IdentifierName(Names.SourceNode)));
            yield return ToNodeMethod();
            yield return ToNodeExplicitInterfaceMethod();
            MethodDeclarationSyntax ToNodeMethod()
            {
                return
                    MethodDeclaration(nodeTypeIdentifierName, Names.ToNode)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .MutateIf(IsAbstract, x => x.AddModifiers(SyntaxKind.AbstractKeyword))
                    .AddModifiers(SyntaxKind.OverrideKeyword)
                    .AddParameterListParameters(
                        parentParameterBase
                        .WithDefault(
                            EqualsValueClause(
                                LiteralExpression(SyntaxKind.NullLiteralExpression))))
                    .MutateIf(
                        IsAbstract,
                        x => x.WithSemicolonTokenDefault(),
                        x => x
                        .WithExpressionBodyFull(
                            ObjectCreationExpression(nodeTypeIdentifierName)
                            .AddArgumentListArguments(
                                Argument(
                                    ThisExpression()),
                                Argument(
                                    IdentifierName(ParentLocal)))));
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
                    .AddParameterListParameters(parentParameterBase)
                    .WithExpressionBodyFull(
                        IdentifierName(Names.ToNode)
                        .Invoke(
                            IdentifierName(ParentLocal)));
            }
        }
    }
}
