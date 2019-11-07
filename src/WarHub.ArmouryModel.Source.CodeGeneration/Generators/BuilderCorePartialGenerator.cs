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
                .AddAttributeLists(Descriptor.CoreTypeAttributeLists)
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

        private IEnumerable<MemberDeclarationSyntax> GetPropertyMembers(CoreDescriptor.Entry entry)
        {
            return entry is CoreDescriptor.CollectionEntry collectionEntry
                ? CreateArrayProperty()
                : entry is CoreDescriptor.ComplexEntry complexEntry
                ? CreateComplexProperty()
                : CreateSimpleProperty();

            IEnumerable<PropertyDeclarationSyntax> CreateSimpleProperty()
            {
                yield return
                    PropertyDeclaration(entry.Type, entry.Identifier)
                    .AddAttributeLists(entry.AttributeLists)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .AddAccessorListAccessors(
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonTokenDefault(),
                        AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonTokenDefault());
            }
            IEnumerable<PropertyDeclarationSyntax> CreateComplexProperty()
            {
                yield return
                    PropertyDeclaration(
                        complexEntry.Type.ToNestedBuilderType(),
                        entry.Identifier)
                    .AddAttributeLists(entry.AttributeLists)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .AddAccessorListAccessors(
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonTokenDefault(),
                        AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonTokenDefault());
            }
            IEnumerable<MemberDeclarationSyntax> CreateArrayProperty()
            {
                var propertyName = collectionEntry.Identifier.ValueText;
                var fieldName = $"_{char.ToLowerInvariant(propertyName[0])}{propertyName.Substring(1)}";
                yield return
                    FieldDeclaration(
                        VariableDeclaration(
                            collectionEntry.ToListOfBuilderType())
                        .AddVariables(
                            VariableDeclarator(fieldName)))
                    .AddModifiers(SyntaxKind.PrivateKeyword);

                yield return
                    PropertyDeclaration(
                        collectionEntry.ToListOfBuilderType(),
                        collectionEntry.Identifier)
                    .AddAttributeLists(entry.AttributeLists)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .AddAccessorListAccessors(
                        CreateGetter(),
                        CreateSetter());
                AccessorDeclarationSyntax CreateGetter()
                {
                    return
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithExpressionBodyFull(
                            BinaryExpression(
                                SyntaxKind.CoalesceExpression,
                                IdentifierName(fieldName),
                                ParenthesizedExpression(
                                    AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        IdentifierName(fieldName),
                                        ObjectCreationExpression(
                                            collectionEntry.ToListOfBuilderType())
                                        .AddArgumentListArguments()))));
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
            return
                MethodDeclaration(
                    Descriptor.CoreType,
                    Names.ToImmutable)
                .AddModifiers(SyntaxKind.PublicKeyword)
                .AddBodyStatements(
                    ReturnStatement(
                        ObjectCreationExpression(Descriptor.CoreType)
                        .AddArgumentListArguments(
                            Descriptor.Entries.Select(
                                simpleEntry => Argument(simpleEntry.IdentifierName),
                                CreateComplexArgument,
                                CreateCollectionArgument))));

            static ArgumentSyntax CreateComplexArgument(CoreDescriptor.Entry entry)
            {
                return
                    Argument(
                        entry.IdentifierName
                        .ConditionalMemberAccess(
                            IdentifierName(Names.ToImmutable))
                        .InvokeWithArguments());
            }

            static ArgumentSyntax CreateCollectionArgument(CoreDescriptor.Entry entry)
            {
                return
                    Argument(
                        entry.IdentifierName
                        .MemberAccess(
                            IdentifierName(Names.ToImmutableRecursive))
                        .InvokeWithArguments());
            }
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

            static ExpressionSyntax CreateSimpleInitializer(CoreDescriptor.Entry entry)
            {
                return
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        entry.IdentifierName,
                        entry.IdentifierName);
            }

            static ExpressionSyntax CreateComplexInitializer(CoreDescriptor.ComplexEntry entry)
            {
                return
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        entry.IdentifierName,
                        entry.IdentifierName
                        .ConditionalMemberAccess(
                            IdentifierName(Names.ToBuilder))
                        .InvokeWithArguments());
            }

            static ExpressionSyntax CreateCollectionInitializer(CoreDescriptor.CollectionEntry entry)
            {
                return
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        entry.IdentifierName,
                        entry.IdentifierName
                        .MemberAccess(
                            IdentifierName(Names.ToBuildersList))
                        .InvokeWithArguments());
            }
        }
    }
}
