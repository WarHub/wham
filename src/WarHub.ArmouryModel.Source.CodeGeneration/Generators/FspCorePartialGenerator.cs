using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        private static MemberDeclarationSyntax GetToSerializationProxyMethod()
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
            const string PropertyName = "Immutable";

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
                const string ParamName = "immutable";
                return
                    ConstructorDeclaration(Names.FastSerializationProxy)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .AddParameterListParameters(
                        Parameter(
                            Identifier(ParamName))
                        .WithType(Descriptor.CoreType))
                    .AddBodyStatements(
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName(PropertyName),
                                IdentifierName(ParamName))));
            }
            MemberDeclarationSyntax CreateBackingProperty()
            {
                return
                    PropertyDeclaration(
                        Descriptor.CoreType,
                        PropertyName)
                    .AddModifiers(SyntaxKind.InternalKeyword)
                    .AddAccessorListAccessors(
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonTokenDefault());
            }

            static PropertyDeclarationSyntax CreateProperty(CoreDescriptor.Entry entry)
            {
                var propertyType = entry is CoreDescriptor.CollectionEntry collectionEntry
                    ? QualifiedName(collectionEntry.CollectionTypeParameter, IdentifierName(Names.FastSerializationEnumerable))
                    : entry is CoreDescriptor.ComplexEntry complexEntry
                    ? QualifiedName(complexEntry.NameSyntax, IdentifierName(Names.FastSerializationProxy))
                    : entry.Type;
                return
                    PropertyDeclaration(propertyType, entry.Identifier)
                    .AddAttributeLists(entry.AttributeLists)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .AddAccessorListAccessors(
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithExpressionBodyFull(
                            IdentifierName(PropertyName)
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
                                    IdentifierName(Names.NotSupportedException))
                                .WithArgumentList(
                                    ArgumentList()))));
            }

            static PropertyDeclarationSyntax CreateSpecifiedProperty(CoreDescriptor.Entry entry)
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
                                    IdentifierName(Names.XmlIgnore)))))
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
