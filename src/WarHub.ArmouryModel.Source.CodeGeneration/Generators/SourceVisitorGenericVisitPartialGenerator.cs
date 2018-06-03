﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class SourceVisitorGenericVisitPartialGenerator : PartialGeneratorBase
    {
        protected SourceVisitorGenericVisitPartialGenerator(CoreDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        public static TypeDeclarationSyntax Generate(CoreDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new SourceVisitorGenericVisitPartialGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        protected override SyntaxToken GenerateTypeIdentifier() => Identifier(Names.SourceVisitor);

        protected override IEnumerable<TypeParameterSyntax> GenerateTypeParameters()
        {
            yield return TypeParameter(Names.SourceVisitorTypeParameter);
        }

        protected override IEnumerable<MemberDeclarationSyntax> GenerateMembers()
        {
            const string node = "node";
            yield return CreateVisitMethod(
                Names.Visit + Descriptor.RawModelName,
                Descriptor.GetNodeTypeIdentifierName());
            yield return CreateVisitMethod(
                Names.Visit + Descriptor.RawModelName + Names.ListSuffix,
                Descriptor.GetListNodeTypeIdentifierName());
            MethodDeclarationSyntax CreateVisitMethod(string methodName, IdentifierNameSyntax type)
            {
                return
                    MethodDeclaration(
                        IdentifierName(Names.SourceVisitorTypeParameter),
                        methodName)
                    .AddModifiers(
                        SyntaxKind.PublicKeyword,
                        SyntaxKind.VirtualKeyword)
                    .AddParameterListParameters(
                        Parameter(
                            Identifier(node))
                        .WithType(
                            type))
                    .AddBodyStatements(
                        ReturnStatement(
                            ThisExpression()
                            .MemberAccess(
                                IdentifierName(Names.DefaultVisit))
                            .InvokeWithArguments(
                                IdentifierName(node))));
            }
        }
    }
}
