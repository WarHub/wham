using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class FseCorePartialGenerator : CorePartialGeneratorBase
    {
        protected FseCorePartialGenerator(CoreDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        public static TypeDeclarationSyntax Generate(CoreDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new FseCorePartialGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        protected override IEnumerable<MemberDeclarationSyntax> GenerateMembers()
        {
            yield return GenerateFastSerializationEnumerable();
        }

        private MemberDeclarationSyntax GenerateFastSerializationEnumerable()
        {
            return
                StructDeclaration(Names.FastSerializationEnumerable)
                .AddModifiers(SyntaxKind.PublicKeyword)
                .WithBaseList(
                    GenerateFseBaseList())
                .AddMembers(
                    GenerateFseMembers());
        }

        private static BaseListSyntax GenerateFseBaseList()
        {
            return BaseList()
                .AddTypes(
                    SimpleBaseType(
                        GenericName(
                            Identifier(Names.IEnumerable))
                        .AddTypeArgumentListArguments(
                            IdentifierName(Names.FastSerializationProxy))));
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateFseMembers()
        {
            const string EnumerablePropName = "Enumerable";
            var collectionType = Descriptor.ImmutableArrayOfCoreType;
            yield return CreateConstructor();
            yield return CreateBackingProperty();
            yield return CreateCountProperty();
            yield return CreateIndexer();
            yield return CreateAddMethod();
            yield return CreateGetEnumeratorMethod();
            yield return CreateExplicitInterafceGetEnumeratorMethod();
            yield return CreateImplicitCastOperator();

            MemberDeclarationSyntax CreateConstructor()
            {
                const string ParamName = "enumerable";
                return
                    ConstructorDeclaration(Names.FastSerializationEnumerable)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .AddParameterListParameters(
                        Parameter(
                            Identifier(ParamName))
                        .WithType(collectionType))
                    .AddBodyStatements(
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName(EnumerablePropName),
                                IdentifierName(ParamName))));
            }
            MemberDeclarationSyntax CreateBackingProperty()
            {
                return
                    PropertyDeclaration(
                        collectionType,
                        EnumerablePropName)
                    .AddModifiers(SyntaxKind.PrivateKeyword)
                    .AddAccessorListAccessors(
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonTokenDefault());
            }

            static MemberDeclarationSyntax CreateCountProperty()
            {
                return
                    PropertyDeclaration(
                        PredefinedType(
                            Token(SyntaxKind.IntKeyword)),
                        nameof(Names.Count))
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .WithExpressionBodyFull(
                        ConditionalExpression(
                            IdentifierName(EnumerablePropName)
                            .MemberAccess(
                                IdentifierName(nameof(System.Collections.Immutable.ImmutableArray<int>.IsDefault))),
                            LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0)),
                            IdentifierName(EnumerablePropName)
                            .MemberAccess(
                                IdentifierName(nameof(System.Collections.Immutable.ImmutableArray<int>.Length)))));
            }

            static MemberDeclarationSyntax CreateIndexer()
            {
                const string IndexParamName = "index";
                return
                    IndexerDeclaration(
                        IdentifierName(Names.FastSerializationProxy))
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .AddParameterListParameters(
                        Parameter(
                            Identifier(IndexParamName))
                        .WithType(
                            PredefinedType(
                                Token(SyntaxKind.IntKeyword))))
                    .WithExpressionBodyFull(
                        ElementAccessExpression(
                            IdentifierName(EnumerablePropName))
                        .AddArgumentListArguments(
                            Argument(
                                IdentifierName(IndexParamName)))
                        .MemberAccess(
                            IdentifierName(Names.ToSerializationProxy))
                        .InvokeWithArguments());
            }

            static MemberDeclarationSyntax CreateAddMethod()
            {
                const string ParamName = "item";
                return
                    MethodDeclaration(
                        PredefinedType(
                            Token(SyntaxKind.VoidKeyword)),
                        Names.Add)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .AddAttributeLists(
                        AttributeList(
                            SingletonSeparatedList(
                                CreateObsoleteAttribute())))
                    .AddParameterListParameters(
                        Parameter(
                            Identifier(ParamName))
                        .WithType(
                            IdentifierName(Names.FastSerializationProxy)))
                    .WithExpressionBodyFull(
                        ThrowExpression(
                            ObjectCreationExpression(
                                IdentifierName(Names.NotSupportedException))
                            .AddArgumentListArguments()));

                static AttributeSyntax CreateObsoleteAttribute()
                {
                    return
                        Attribute(
                            IdentifierName(Names.Obsolete))
                        .AddArgumentListArguments(
                            AttributeArgument(
                                LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    Literal("Implemented only to make XML serialization work. Throws System.NotSupportedException."))));
                }
            }

            static MemberDeclarationSyntax CreateGetEnumeratorMethod()
            {
                const string ItemVarName = "item";
                return
                    MethodDeclaration(
                        GenericName(
                            Identifier(Names.IEnumerator))
                        .AddTypeArgumentListArguments(
                            IdentifierName(Names.FastSerializationProxy)),
                        Names.GetEnumerator)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .AddBodyStatements(
                        IfStatement(
                            IdentifierName(EnumerablePropName)
                            .MemberAccess(
                                IdentifierName(nameof(System.Collections.Immutable.ImmutableArray<int>.IsDefaultOrEmpty))),
                            YieldStatement(SyntaxKind.YieldBreakStatement)),
                        ForEachStatement(
                            IdentifierName("var"),
                            Identifier(ItemVarName),
                            IdentifierName(EnumerablePropName),
                            YieldStatement(
                                SyntaxKind.YieldReturnStatement,
                                IdentifierName(ItemVarName)
                                .MemberAccess(
                                    IdentifierName(Names.ToSerializationProxy))
                                .InvokeWithArguments())));
            }

            static MemberDeclarationSyntax CreateExplicitInterafceGetEnumeratorMethod()
            {
                return
                    MethodDeclaration(
                        IdentifierName(Names.IEnumerator),
                        Identifier(Names.GetEnumerator))
                    .WithExplicitInterfaceSpecifier(
                        ExplicitInterfaceSpecifier(
                            IdentifierName(Names.IEnumerable)))
                    .WithExpressionBodyFull(
                        InvocationExpression(
                            IdentifierName(Names.GetEnumerator)));
            }
            MemberDeclarationSyntax CreateImplicitCastOperator()
            {
                const string ParamName = "enumerable";
                return
                    ConversionOperatorDeclaration(
                        Token(SyntaxKind.ImplicitKeyword),
                        IdentifierName(Names.FastSerializationEnumerable))
                    .AddModifiers(
                        SyntaxKind.PublicKeyword,
                        SyntaxKind.StaticKeyword)
                    .AddParameterListParameters(
                        Parameter(
                            Identifier(ParamName))
                        .WithType(collectionType))
                    .AddBodyStatements(
                        ReturnStatement(
                            ObjectCreationExpression(
                                IdentifierName(Names.FastSerializationEnumerable))
                            .AddArgumentListArguments(
                                Argument(
                                    IdentifierName(ParamName)))));
            }
        }
    }
}
