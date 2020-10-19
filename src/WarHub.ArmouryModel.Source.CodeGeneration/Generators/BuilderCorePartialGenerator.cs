using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class BuilderCorePartialGenerator : CorePartialGeneratorBase
    {
        protected BuilderCorePartialGenerator(CoreDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        public static TypeDeclarationSyntax Generate(CoreDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new BuilderCorePartialGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        protected override IEnumerable<MemberDeclarationSyntax> GenerateMembers()
        {
            yield return GenerateToBuilderMethod();
            yield return GenerateBuilder();
        }

        protected override IEnumerable<BaseTypeSyntax> GenerateBaseTypes()
        {
            yield return
                SimpleBaseType(
                    GenericName(
                        Identifier(Names.IBuildable))
                    .AddTypeArgumentListArguments(
                        Descriptor.CoreType,
                        QualifiedName(
                            Descriptor.CoreType,
                            IdentifierName(Names.Builder))));
        }

        private ClassDeclarationSyntax GenerateBuilder()
        {
            return
                ClassDeclaration(Names.Builder)
                .AddAttributeLists(Descriptor.XmlAttributeLists)
                .AddModifiers(
                    SyntaxKind.PublicKeyword,
                    SyntaxKind.PartialKeyword)
                .WithBaseList(
                    GetBuilderBaseList())
                .WithMembers(
                    GenerateBuilderMembers());

            SyntaxList<MemberDeclarationSyntax> GenerateBuilderMembers()
            {
                return List<MemberDeclarationSyntax>()
                    .AddRange(Descriptor.Entries.SelectMany(GetPropertyMembers))
                    .Add(GetBuilderToImmutableMethod());
            }
            BaseListSyntax GetBuilderBaseList()
            {
                return BaseList()
                    .AddTypes(
                    SimpleBaseType(
                        GenericName(
                            Identifier(Names.IBuilder))
                        .AddTypeArgumentListArguments(Descriptor.CoreType)));
            }
        }

        private IEnumerable<MemberDeclarationSyntax> GetPropertyMembers(CoreChildBase entry)
        {
            return entry is CoreListChild collectionEntry
                ? CreateLazyProperty(collectionEntry.Identifier, collectionEntry.ToListOfBuilderType())
                : entry is CoreObjectChild complexEntry
                ? CreateLazyProperty(complexEntry.Identifier, complexEntry.BuilderType)
                : CreateSimpleProperty();

            IEnumerable<PropertyDeclarationSyntax> CreateSimpleProperty()
            {
                yield return
                    PropertyDeclaration(entry.Type, entry.Identifier)
                    .AddAttributeLists(entry.XmlAttributeLists)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .AddAccessorListAccessors(
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(),
                        AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken());
            }
            IEnumerable<MemberDeclarationSyntax> CreateLazyProperty(SyntaxToken identifier, TypeSyntax type)
            {
                var propertyName = identifier.ValueText;
                var fieldName = $"_{char.ToLowerInvariant(propertyName[0])}{propertyName.Substring(1)}";
                yield return
                    FieldDeclaration(
                        VariableDeclaration(
                            NullableType(type))
                        .AddVariables(
                            VariableDeclarator(fieldName)))
                    .AddModifiers(SyntaxKind.PrivateKeyword);

                yield return
                    PropertyDeclaration(type, identifier)
                    .AddAttributeLists(entry.XmlAttributeLists)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .AddAccessorListAccessors(
                        CreateGetter(),
                        CreateSetter());
                AccessorDeclarationSyntax CreateGetter()
                {
                    return
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithExpressionBodyFull(
                            AssignmentExpression(
                                SyntaxKind.CoalesceAssignmentExpression,
                                IdentifierName(fieldName),
                                ObjectCreationExpression(type)
                                .AddArgumentListArguments()));
                }
                AccessorDeclarationSyntax CreateSetter()
                {
                    return
                        AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithExpressionBodyFull(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName(fieldName),
                                IdentifierName("value")));
                }
            }
        }

        private MethodDeclarationSyntax GetBuilderToImmutableMethod()
        {
            var initExpressions =
                Descriptor.Entries
                .Select(x => AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, x.IdentifierName, GetCorePropValue(x)))
                .ToArray();
            return
                MethodDeclaration(
                    Descriptor.CoreType,
                    Names.ToImmutable)
                .AddModifiers(SyntaxKind.PublicKeyword)
                .AddBodyStatements(
                    ReturnStatement(
                        Descriptor.CoreType.ObjectCreationWithInitializer(initExpressions)));
            static ExpressionSyntax GetCorePropValue(CoreChildBase entry) => entry switch
            {
                CoreListChild => entry.IdentifierName.Dot(IdentifierName(Names.ToImmutableRecursive)).Invoke(),
                CoreObjectChild => entry.IdentifierName.Dot(IdentifierName(Names.ToImmutable)).Invoke(),
                _ => entry.IdentifierName,
            };
        }

        private MethodDeclarationSyntax GenerateToBuilderMethod()
        {
            return
                MethodDeclaration(
                    IdentifierName(Names.Builder),
                    Names.ToBuilder)
                .AddModifiers(SyntaxKind.PublicKeyword)
                .AddBodyStatements(
                    ReturnStatement(
                        ObjectCreationExpression(
                            IdentifierName(Names.Builder))
                        .WithInitializer(
                            InitializerExpression(SyntaxKind.ObjectInitializerExpression)
                            .AddExpressions(
                                Descriptor.Entries
                                .Select(
                                    CreateSimpleInitializer,
                                    CreateComplexInitializer,
                                    CreateCollectionInitializer)
                                .ToArray()))));

            static ExpressionSyntax CreateSimpleInitializer(CoreChildBase entry)
            {
                return
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        entry.IdentifierName,
                        entry.IdentifierName);
            }

            static ExpressionSyntax CreateComplexInitializer(CoreObjectChild entry)
            {
                return
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        entry.IdentifierName,
                        entry.IdentifierName
                        .Dot(
                            IdentifierName(Names.ToBuilder))
                        .Invoke());
            }

            static ExpressionSyntax CreateCollectionInitializer(CoreListChild entry)
            {
                return
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        entry.IdentifierName,
                        entry.IdentifierName
                        .Dot(
                            IdentifierName(Names.ToBuildersList))
                        .Invoke());
            }
        }
    }
}
