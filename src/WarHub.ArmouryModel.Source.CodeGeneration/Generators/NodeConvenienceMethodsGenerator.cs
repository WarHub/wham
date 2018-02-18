using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class NodeConvenienceMethodsGenerator : NodePartialGeneratorBase
    {
        protected NodeConvenienceMethodsGenerator(CoreDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        public static TypeDeclarationSyntax Generate(CoreDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new NodeConvenienceMethodsGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        protected override IEnumerable<MemberDeclarationSyntax> GenerateMembers()
        {
            if (Descriptor.DeclaredEntries.Length > 0)
            {
                yield return GenerateDeconstruct();
            }
        }

        private MemberDeclarationSyntax GenerateDeconstruct()
        {
            return
                MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.VoidKeyword)),
                    Names.Deconstruct)
                .AddModifiers(SyntaxKind.PublicKeyword)
                .AddParameterListParameters(
                    Descriptor.Entries.Select(CreateParameter))
                .AddBodyStatements(
                    Descriptor.Entries.Select(CreateAssignment));
            ParameterSyntax CreateParameter(CoreDescriptor.Entry entry)
            {
                var type = entry is CoreDescriptor.CollectionEntry collectionEntry
                    ? collectionEntry.GetNodeTypeIdentifierName().ToNodeListType()
                    : entry is CoreDescriptor.ComplexEntry complexEntry
                    ? complexEntry.GetNodeTypeIdentifierName()
                    : entry.Type;
                return
                    Parameter(entry.CamelCaseIdentifier)
                    .WithType(type)
                    .AddModifiers(SyntaxKind.OutKeyword);
            }
            StatementSyntax CreateAssignment(CoreDescriptor.Entry entry)
            {
                return
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            entry.CamelCaseIdentifierName,
                            entry.IdentifierName));
            }
        }
    }
}
