using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class NodeFactoryPartialGenerator : PartialGeneratorBase
    {
        protected NodeFactoryPartialGenerator(CoreDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        public static TypeDeclarationSyntax Generate(CoreDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new NodeFactoryPartialGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        protected override SyntaxToken GenerateTypeIdentifier()
        {
            return Identifier("NodeFactory");
        }

        protected override IEnumerable<MemberDeclarationSyntax> GenerateMembers()
        {
            return List<MemberDeclarationSyntax>()
                .AddRange(GenerateFactoryMethods());
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateFactoryMethods()
        {
            yield return CreateFull();
            MethodDeclarationSyntax CreateFull()
            {
                return
                    MethodDeclaration(
                        Descriptor.GetNodeTypeIdentifierName(),
                        Descriptor.CoreTypeIdentifier.ValueText.StripSuffixes())
                    .AddModifiers(
                        SyntaxKind.PublicKeyword,
                        SyntaxKind.StaticKeyword)
                    .AddParameterListParameters(
                        CreateParameters())
                    .AddBodyStatements(
                        CreateReturnStatement());
                IEnumerable<ParameterSyntax> CreateParameters()
                {
                    return
                        Descriptor.Entries
                        .OfType<CoreDescriptor.SimpleEntry>()
                        .Select(CreateSimpleParameter)
                        .Concat(
                            Descriptor.Entries
                            .OfType<CoreDescriptor.CollectionEntry>()
                            .Select(CreateCollectionParameter));
                }
                ParameterSyntax CreateSimpleParameter(CoreDescriptor.SimpleEntry entry)
                {
                    return
                        Parameter(entry.CamelCaseIdentifier)
                        .WithType(entry.Type);
                }
                ParameterSyntax CreateCollectionParameter(CoreDescriptor.CollectionEntry entry)
                {
                    var type = entry.GetNodeTypeIdentifierName().ToNodeListType();
                    return
                        Parameter(entry.CamelCaseIdentifier)
                        .WithType(type)
                        .WithDefault(
                            EqualsValueClause(
                                DefaultExpression(type)));
                }
                StatementSyntax CreateReturnStatement()
                {
                    return
                        ReturnStatement(
                            InvocationExpression(
                                CreateToNodeMemberAccess()));
                }
                ExpressionSyntax CreateToNodeMemberAccess()
                {
                    return
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            CreateObjectCreation(),
                            IdentifierName(Names.ToNode));
                }
                ExpressionSyntax CreateObjectCreation()
                {
                    return
                        ObjectCreationExpression(Descriptor.CoreType)
                        .AddArgumentListArguments(
                            Descriptor.Entries.Select(CreateArgument));
                    ArgumentSyntax CreateArgument(CoreDescriptor.Entry entry)
                    {
                        var argName = entry.CamelCaseIdentifierName;
                        return entry is CoreDescriptor.SimpleEntry
                            ? Argument(argName)
                            : Argument(
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        argName,
                                        IdentifierName(Names.ToCoreArray))));
                    }
                }
            }
        }
    }
}
