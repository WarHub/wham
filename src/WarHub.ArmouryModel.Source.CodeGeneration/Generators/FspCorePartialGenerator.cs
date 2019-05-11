using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class FspCorePartialGenerator : CorePartialGeneratorBase
    {
        protected FspCorePartialGenerator(CoreDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        public static TypeDeclarationSyntax Generate(CoreDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new FspCorePartialGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        protected override IEnumerable<MemberDeclarationSyntax> GenerateMembers()
        {
            yield return GetToSerializationProxyMethod();
            yield return GenerateFastSerializationProxy();
        }

        private MemberDeclarationSyntax GenerateFastSerializationProxy()
        {
            return
                StructDeclaration(Names.FastSerializationProxy)
                .AddAttributeLists(Descriptor.CoreTypeAttributeLists)
                .AddModifiers(SyntaxKind.PublicKeyword)
                .AddMembers(
                    GetFastSerializationProxyMembers());
        }

        private MemberDeclarationSyntax GetToSerializationProxyMethod()
        {
            return
                MethodDeclaration(
                    IdentifierName(Names.FastSerializationProxy),
                    Identifier(Names.ToSerializationProxy))
                .AddModifiers(SyntaxKind.PublicKeyword)
                .WithExpressionBodyFull(
                    ObjectCreationExpression(
                        IdentifierName(Names.FastSerializationProxy))
                    .AddArgumentListArguments(
                        Argument(
                            ThisExpression())));
        }

        private IEnumerable<MemberDeclarationSyntax> GetFastSerializationProxyMembers()
        {
            const string propertyName = "Immutable";

            yield return CreateConstructor();
            yield return CreateBackingProperty();
            foreach (var entry in Descriptor.Entries)
            {
                yield return CreateProperty(entry);
                if (entry.IsCollection)
                {
                    yield return CreateSpecifiedProperty(entry);
                }
            }
            MemberDeclarationSyntax CreateConstructor()
            {
                const string paramName = "immutable";
                return
                    ConstructorDeclaration(Names.FastSerializationProxy)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .AddParameterListParameters(
                        Parameter(
                            Identifier(paramName))
                        .WithType(Descriptor.CoreType))
                    .AddBodyStatements(
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName(propertyName),
                                IdentifierName(paramName))));
            }
            MemberDeclarationSyntax CreateBackingProperty()
            {
                return
                    PropertyDeclaration(
                        Descriptor.CoreType,
                        propertyName)
                    .AddModifiers(SyntaxKind.InternalKeyword)
                    .AddAccessorListAccessors(
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonTokenDefault());
            }
            PropertyDeclarationSyntax CreateProperty(CoreDescriptor.Entry entry)
            {
                TypeSyntax propertyType = entry is CoreDescriptor.CollectionEntry collectionEntry
                    ? QualifiedName(collectionEntry.CollectionTypeParameter, IdentifierName(Names.FastSerializationEnumerable))
                    : entry is CoreDescriptor.ComplexEntry complexEntry
                    ? QualifiedName(complexEntry.Type, IdentifierName(Names.FastSerializationProxy))
                    : entry.Type;
                return
                    PropertyDeclaration(propertyType, entry.Identifier)
                    .AddAttributeLists(entry.AttributeLists)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .AddAccessorListAccessors(
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithExpressionBodyFull(
                            IdentifierName(propertyName)
                            .MemberAccess(entry.IdentifierName)
                            .MutateIf(
                                entry.IsComplex,
                                x => x.MemberAccess(
                                    IdentifierName(Names.ToSerializationProxy))
                                    .InvokeWithArguments())),
                        AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithExpressionBodyFull(
                            ThrowExpression(
                                ObjectCreationExpression(
                                    ParseTypeName(Names.NotSupportedExceptionFull))
                                .WithArgumentList(
                                    ArgumentList()))));
            }
            PropertyDeclarationSyntax CreateSpecifiedProperty(CoreDescriptor.Entry entry)
            {
                return
                    PropertyDeclaration(
                        PredefinedType(
                            Token(SyntaxKind.BoolKeyword)),
                        Identifier(entry.Identifier.Text + Names.SpecifiedSuffix))
                    .AddAttributeLists(
                        AttributeList(
                            SingletonSeparatedList(
                                Attribute(
                                    ParseName(Names.XmlIgnoreQualified)))))
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .WithExpressionBodyFull(
                        BinaryExpression(
                            SyntaxKind.NotEqualsExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                entry.IdentifierName,
                                IdentifierName(Names.Count)),
                            LiteralExpression(
                                SyntaxKind.NumericLiteralExpression,
                                Literal(0))));
            }
        }
    }
}
