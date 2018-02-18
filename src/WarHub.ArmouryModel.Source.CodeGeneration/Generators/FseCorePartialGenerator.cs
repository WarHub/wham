using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Threading;
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
        private BaseListSyntax GenerateFseBaseList()
        {
            return BaseList()
                .AddTypes(
                    SimpleBaseType(
                        GenericName(
                            Identifier(Names.IEnumerableGeneric))
                        .AddTypeArgumentListArguments(
                            IdentifierName(Names.FastSerializationProxy))
                        .WithNamespace(Names.IEnumerableGenericNamespace)));
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateFseMembers()
        {
            const string enumerablePropName = "Enumerable";
            TypeSyntax collectionType = Descriptor.CoreType.ToImmutableArrayType();
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
                const string paramName = "enumerable";
                return
                    ConstructorDeclaration(Names.FastSerializationEnumerable)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .AddParameterListParameters(
                        Parameter(
                            Identifier(paramName))
                        .WithType(collectionType))
                    .AddBodyStatements(
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName(enumerablePropName),
                                IdentifierName(paramName))));
            }
            MemberDeclarationSyntax CreateBackingProperty()
            {
                return
                    PropertyDeclaration(
                        collectionType,
                        enumerablePropName)
                    .AddModifiers(SyntaxKind.PrivateKeyword)
                    .AddAccessorListAccessors(
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonTokenDefault());
            }
            MemberDeclarationSyntax CreateCountProperty()
            {
                return
                    PropertyDeclaration(
                        PredefinedType(
                            Token(SyntaxKind.IntKeyword)),
                        nameof(Names.Count))
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .WithExpressionBodyFull(
                        ConditionalExpression(
                            IdentifierName(enumerablePropName)
                            .MemberAccess(
                                IdentifierName(nameof(System.Collections.Immutable.ImmutableArray<int>.IsDefault))),
                            LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0)),
                            IdentifierName(enumerablePropName)
                            .MemberAccess(
                                IdentifierName(nameof(System.Collections.Immutable.ImmutableArray<int>.Length)))));
            }
            MemberDeclarationSyntax CreateIndexer()
            {
                const string indexParamName = "index";
                return
                    IndexerDeclaration(
                        IdentifierName(Names.FastSerializationProxy))
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .AddParameterListParameters(
                        Parameter(
                            Identifier(indexParamName))
                        .WithType(
                            PredefinedType(
                                Token(SyntaxKind.IntKeyword))))
                    .WithExpressionBodyFull(
                        ElementAccessExpression(
                            IdentifierName(enumerablePropName))
                        .AddArgumentListArguments(
                            Argument(
                                IdentifierName(indexParamName)))
                        .MemberAccess(
                            IdentifierName(Names.ToSerializationProxy))
                        .InvokeWithArguments());
            }
            MemberDeclarationSyntax CreateAddMethod()
            {
                const string paramName = "item";
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
                            Identifier(paramName))
                        .WithType(
                            IdentifierName(Names.FastSerializationProxy)))
                    .WithExpressionBodyFull(
                        ThrowExpression(
                            ObjectCreationExpression(
                                ParseName(Names.NotSupportedExceptionFull))
                            .AddArgumentListArguments()));
                AttributeSyntax CreateObsoleteAttribute()
                {
                    return
                        Attribute(
                            ParseName(Names.ObsoleteFull))
                        .AddArgumentListArguments(
                            AttributeArgument(
                                LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    Literal("Implemented only to make XML serialization work. Throws System.NotSupportedException."))));
                }
            }
            MemberDeclarationSyntax CreateGetEnumeratorMethod()
            {
                const string itemVarName = "item";
                return
                    MethodDeclaration(
                        GenericName(
                            Identifier(Names.IEnumeratorGeneric))
                        .AddTypeArgumentListArguments(
                            IdentifierName(Names.FastSerializationProxy))
                        .WithNamespace(Names.IEnumeratorGenericNamespace),
                        Names.GetEnumerator)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .AddBodyStatements(
                        IfStatement(
                            IdentifierName(enumerablePropName)
                            .MemberAccess(
                                IdentifierName(nameof(System.Collections.Immutable.ImmutableArray<int>.IsDefaultOrEmpty))),
                            YieldStatement(SyntaxKind.YieldBreakStatement)),
                        ForEachStatement(
                            IdentifierName("var"),
                            Identifier(itemVarName),
                            IdentifierName(enumerablePropName),
                            YieldStatement(
                                SyntaxKind.YieldReturnStatement,
                                IdentifierName(itemVarName)
                                .MemberAccess(
                                    IdentifierName(Names.ToSerializationProxy))
                                .InvokeWithArguments())));
            }
            MemberDeclarationSyntax CreateExplicitInterafceGetEnumeratorMethod()
            {
                return
                    MethodDeclaration(
                        ParseTypeName(Names.IEnumeratorNonGenericFull),
                        Identifier(Names.GetEnumerator))
                    .WithExplicitInterfaceSpecifier(
                        ExplicitInterfaceSpecifier(
                            ParseName(Names.IEnumerableNonGenericFull)))
                    .WithExpressionBodyFull(
                        InvocationExpression(
                            IdentifierName(Names.GetEnumerator)));
            }
            MemberDeclarationSyntax CreateImplicitCastOperator()
            {
                const string paramName = "enumerable";
                return
                    ConversionOperatorDeclaration(
                        Token(SyntaxKind.ImplicitKeyword),
                        IdentifierName(Names.FastSerializationEnumerable))
                    .AddModifiers(
                        SyntaxKind.PublicKeyword,
                        SyntaxKind.StaticKeyword)
                    .AddParameterListParameters(
                        Parameter(
                            Identifier(paramName))
                        .WithType(collectionType))
                    .AddBodyStatements(
                        ReturnStatement(
                            ObjectCreationExpression(
                                IdentifierName(Names.FastSerializationEnumerable))
                            .AddArgumentListArguments(
                                Argument(
                                    IdentifierName(paramName)))));
            }
        }
    }
}
