using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class NodeGenerator : NodePartialGeneratorBase
    {
        public const string CoreProperty = "Core";
        public static readonly SyntaxToken CorePropertyIdentifier = Identifier(CoreProperty);
        public static readonly IdentifierNameSyntax CorePropertyIdentifierName = IdentifierName(CoreProperty);

        protected NodeGenerator(CoreDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        public static TypeDeclarationSyntax Generate(CoreDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new NodeGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        protected override SyntaxTokenList GenerateModifiers()
        {
            var modifiers = base.GenerateModifiers();
            return IsAbstract
                ? TokenList(Token(SyntaxKind.AbstractKeyword)).AddRange(modifiers)
                : modifiers;
        }

        protected override IEnumerable<BaseTypeSyntax> GenerateBaseTypes()
        {
            yield return
                SimpleBaseType(
                    GenericName(
                        Identifier(Names.INodeWithCore))
                    .AddTypeArgumentListArguments(Descriptor.CoreType));
        }

        protected override IEnumerable<MemberDeclarationSyntax> GenerateMembers()
        {
            return
                List<MemberDeclarationSyntax>()
                .Add(
                    GenerateConstructor())
                .AddRange(
                    GenerateProperties())
                .AddRange(
                    GenerateMutatorMethods())
                .AddRange(
                    GenerateSyntaxNodeOverrideMethods());
        }

        private ConstructorDeclarationSyntax GenerateConstructor()
        {
            const string coreLocal = "core";
            const string parentLocal = "parent";
            return
                ConstructorDeclaration(
                    Descriptor.GetNodeTypeName())
                .AddModifiers(SyntaxKind.InternalKeyword)
                .AddParameterListParameters(
                    Parameter(
                        Identifier(coreLocal))
                    .WithType(Descriptor.CoreType),
                    Parameter(
                        Identifier(parentLocal))
                    .WithType(
                        IdentifierName(Names.SourceNode)))
                .WithInitializer(
                    ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                    .AddArgumentListArguments(
                        Argument(
                            IdentifierName(coreLocal)),
                        Argument(
                            IdentifierName(parentLocal))))
                .WithBody(
                    Block(
                        GetBodyStatements()));
            IEnumerable<StatementSyntax> GetBodyStatements()
            {
                yield return
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            CorePropertyIdentifierName,
                            IdentifierName(coreLocal)));
                foreach (var entry in Descriptor.DeclaredEntries.OfType<CoreDescriptor.CollectionEntry>())
                {
                    yield return CreateCollectionInitialization(entry);
                }
                if (!IsAbstract)
                {
                    yield return ChildrenCountInitialization();
                }
            }
            StatementSyntax ChildrenCountInitialization()
            {
                var complexCount = Descriptor.Entries.Where(x => x.IsComplex).Count();
                var entries = Descriptor.Entries.OfType<CoreDescriptor.CollectionEntry>().ToImmutableArray();
                return ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName(Names.ChildrenCount),
                        SumNodeListLengthExpression()));

                ExpressionSyntax SumNodeListLengthExpression()
                {
                    if (entries.Length == 0)
                    {
                        return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(complexCount));
                    }
                    return
                        entries
                        .Select(collection => CountExpression(collection.IdentifierName))
                        .MutateIf(
                            complexCount > 0,
                            x => x.Append(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(complexCount))))
                        .Aggregate(
                            (left, right) => BinaryExpression(SyntaxKind.AddExpression, left, right));
                }
            }
            StatementSyntax CreateCollectionInitialization(CoreDescriptor.CollectionEntry entry)
            {
                return
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            entry.IdentifierName,
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        CorePropertyIdentifierName,
                                        entry.IdentifierName),
                                    IdentifierName(Names.ToNodeList)))
                            .AddArgumentListArguments(
                                Argument(
                                    ThisExpression()))));
            }
        }

        private IEnumerable<PropertyDeclarationSyntax> GenerateProperties()
        {
            if (!IsAbstract)
            {
                yield return CreateKindProperty();
            }
            yield return CreateCoreProperty();
            yield return CreateExplicitInterfaceCoreProperty();
            foreach (var property in Descriptor.DeclaredEntries.Select(CreateSimpleProperty, CreateComplexProperty, CreateCollectionProperty))
            {
                yield return property;
            }
            PropertyDeclarationSyntax CreateKindProperty()
            {
                var kindString = Descriptor.RawModelName;
                return
                    PropertyDeclaration(
                        IdentifierName(Names.SourceKind),
                        Names.Kind)
                    .AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.OverrideKeyword)
                    .WithExpressionBodyFull(
                        IdentifierName(Names.SourceKind)
                        .MemberAccess(
                            IdentifierName(kindString)));
            }
            PropertyDeclarationSyntax CreateCoreProperty()
            {
                return
                    PropertyDeclaration(
                        Descriptor.CoreType,
                        CorePropertyIdentifier)
                    .AddModifiers(SyntaxKind.InternalKeyword, SyntaxKind.NewKeyword)
                    .AddAccessorListAccessors(
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonTokenDefault());
            }
            PropertyDeclarationSyntax CreateExplicitInterfaceCoreProperty()
            {
                return 
                    PropertyDeclaration(
                        Descriptor.CoreType,
                        CorePropertyIdentifier)
                    .WithExplicitInterfaceSpecifier(
                        ExplicitInterfaceSpecifier(
                            GenericName(
                                Identifier(Names.INodeWithCore))
                            .AddTypeArgumentListArguments(
                                Descriptor.CoreType)))
                    .WithExpressionBodyFull(
                        CorePropertyIdentifierName);
            }
            PropertyDeclarationSyntax CreateSimpleProperty(CoreDescriptor.SimpleEntry entry)
            {
                return
                    PropertyDeclaration(
                        entry.Type,
                        entry.Identifier)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .WithExpressionBodyFull(
                        CorePropertyIdentifierName
                        .MemberAccess(entry.IdentifierName));

            }
            PropertyDeclarationSyntax CreateComplexProperty(CoreDescriptor.ComplexEntry entry)
            {
                return
                    PropertyDeclaration(
                        entry.GetNodeTypeIdentifierName(),
                        entry.Identifier)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .AddAccessorListAccessors(
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonTokenDefault());

            }
            PropertyDeclarationSyntax CreateCollectionProperty(CoreDescriptor.CollectionEntry entry)
            {
                return
                    PropertyDeclaration(
                        entry.GetNodeTypeIdentifierName().ToNodeListType(),
                        entry.Identifier)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .AddAccessorListAccessors(
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonTokenDefault());
            }
                
        }

        private IEnumerable<MethodDeclarationSyntax> GenerateMutatorMethods()
        {
            const string coreParameter = "core";
            yield return UpdateWithMethod();
            if (IsDerived)
            {
                yield return DerivedUpdateWithMethod();
            }
            foreach (var withMethod in Descriptor.Entries.Select(WithForSimpleEntry, WithForComplexEntry, WithForCollectionEntry))
            {
                yield return withMethod;
            }
            MethodDeclarationSyntax UpdateWithMethod()
            {
                var methodSignatureBase =
                    MethodDeclaration(
                        Descriptor.GetNodeTypeIdentifierName(),
                        Names.UpdateWith)
                    .AddModifiers(SyntaxKind.InternalKeyword)
                    .AddParameterListParameters(
                        Parameter(
                            Identifier(coreParameter))
                        .WithType(Descriptor.CoreType));
                if (IsAbstract)
                {
                    return methodSignatureBase
                        .AddModifiers(SyntaxKind.AbstractKeyword)
                        .WithSemicolonTokenDefault();
                }
                return methodSignatureBase
                    .AddBodyStatements(
                        ReturnStatement(
                            ObjectCreationExpression(
                                Descriptor.GetNodeTypeIdentifierName())
                            .AddArgumentListArguments(
                                Argument(IdentifierName(coreParameter)),
                                Argument(
                                    LiteralExpression(SyntaxKind.NullLiteralExpression)))));
            }
            MethodDeclarationSyntax DerivedUpdateWithMethod()
            {
                return
                    MethodDeclaration(
                        IdentifierName(BaseType.Name.GetNodeTypeNameCore()),
                        Names.UpdateWith)
                    .AddModifiers(SyntaxKind.InternalKeyword, SyntaxKind.SealedKeyword, SyntaxKind.OverrideKeyword)
                    .AddParameterListParameters(
                        Parameter(
                            Identifier(coreParameter))
                        .WithType(
                            IdentifierName(BaseType.Name)))
                    .AddBodyStatements(
                        ReturnStatement(
                            IdentifierName(Names.UpdateWith)
                            .InvokeWithArguments(
                                CastExpression(
                                    Descriptor.CoreType,
                                    IdentifierName(coreParameter)))));
            }
            MethodDeclarationSyntax WithBasicPart(CoreDescriptor.Entry entry)
            {
                return
                    MethodDeclaration(
                        Descriptor.GetNodeTypeIdentifierName(),
                        Names.WithPrefix + entry.Identifier)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .MutateIf(
                        IsDerived && entry.Symbol.ContainingType != Descriptor.TypeSymbol,
                        x => x.AddModifiers(SyntaxKind.NewKeyword));
            }
            MethodDeclarationSyntax WithForSimpleEntry(CoreDescriptor.SimpleEntry entry)
            {
                return 
                    WithBasicPart(entry)
                    .AddParameterListParameters(
                        Parameter(entry.Identifier)
                        .WithType(entry?.Type))
                    .WithExpressionBodyFull(
                        IdentifierName(Names.UpdateWith)
                        .InvokeWithArguments(
                            CorePropertyIdentifierName
                            .MemberAccess(
                                IdentifierName(Names.WithPrefix + entry.Identifier))
                            .InvokeWithArguments(entry.IdentifierName)));
            }
            MethodDeclarationSyntax WithForComplexEntry(CoreDescriptor.ComplexEntry entry)
            {
                return
                    WithBasicPart(entry)
                    .AddParameterListParameters(
                        Parameter(entry.Identifier)
                        .WithType(entry.GetNodeTypeIdentifierName()))
                    .WithExpressionBodyFull(
                        IdentifierName(Names.UpdateWith)
                        .InvokeWithArguments(
                            CorePropertyIdentifierName
                            .MemberAccess(
                                IdentifierName(Names.WithPrefix + entry.Identifier))
                            .InvokeWithArguments(
                                entry.IdentifierName
                                .ConditionalMemberAccess(CorePropertyIdentifierName))));
            }
            MethodDeclarationSyntax WithForCollectionEntry(CoreDescriptor.CollectionEntry entry)
            {
                return
                    WithBasicPart(entry)
                    .AddParameterListParameters(
                        Parameter(entry.Identifier)
                        .WithType(
                            entry.GetNodeTypeIdentifierName().ToNodeListType()))
                    .AddBodyStatements(
                        ReturnStatement(
                            IdentifierName(Names.UpdateWith)
                            .InvokeWithArguments(
                                CorePropertyIdentifierName
                                .MemberAccess(
                                    IdentifierName(Names.WithPrefix + entry.Identifier))
                                .InvokeWithArguments(
                                    entry.IdentifierName
                                    .MemberAccess(
                                        IdentifierName(Names.ToCoreArray))
                                    .InvokeWithArguments()))));
            }
        }


        private IEnumerable<MemberDeclarationSyntax> GenerateSyntaxNodeOverrideMethods()
        {
            if (IsAbstract)
            {
                yield break;
            }
            yield return ChildrenLists();
            yield return ChildrenCount();
            yield return GetChild();
            MemberDeclarationSyntax ChildrenLists()
            {
                return
                    MethodDeclaration(
                        CreateIEnumerableOf(
                            IdentifierName(Names.NodeChildUnion)),
                        Names.ChildrenLists)
                    .AddModifiers(
                        SyntaxKind.ProtectedKeyword,
                        SyntaxKind.InternalKeyword,
                        SyntaxKind.OverrideKeyword)
                    .AddBodyStatements(
                        Descriptor.Entries
                        .Where(x => !x.IsSimple)
                        .Select(CreateStatement)
                        .DefaultIfEmpty(
                            YieldStatement(SyntaxKind.YieldBreakStatement)));
                QualifiedNameSyntax CreateIEnumerableOf(TypeSyntax typeParameter)
                {
                    return
                        QualifiedName(
                            ParseName(Names.IEnumerableGenericNamespace),
                            GenericName(
                                Identifier(Names.IEnumerableGeneric))
                            .AddTypeArgumentListArguments(typeParameter));
                }
                StatementSyntax CreateStatement(CoreDescriptor.Entry entry)
                {
                    return YieldStatement(SyntaxKind.YieldReturnStatement, entry.IdentifierName);
                }
            }
            MemberDeclarationSyntax ChildrenCount()
            {
                return
                    PropertyDeclaration(
                        PredefinedType(Token(SyntaxKind.IntKeyword)),
                        Names.ChildrenCount)
                    .AddModifiers(
                        SyntaxKind.ProtectedKeyword,
                        SyntaxKind.InternalKeyword,
                        SyntaxKind.OverrideKeyword)
                    .AddAccessorListAccessors(
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonTokenDefault());
            }
            MemberDeclarationSyntax GetChild()
            {
                const string indexParam = "index";
                var indexIdentifierName = IdentifierName(indexParam);
                var collectionEntries = Descriptor.Entries.OfType<CoreDescriptor.CollectionEntry>().ToImmutableList();
                return
                    MethodDeclaration(
                        IdentifierName(Names.SourceNode),
                        Names.GetChild)
                    .AddModifiers(
                        SyntaxKind.ProtectedKeyword,
                        SyntaxKind.InternalKeyword,
                        SyntaxKind.OverrideKeyword)
                    .AddParameterListParameters(
                        Parameter(
                            Identifier(indexParam))
                        .WithType(
                            PredefinedType(
                                Token(SyntaxKind.IntKeyword))))
                    .AddBodyStatements(
                        collectionEntries
                        .Select(ReturnIfInRangeExpression)
                        .Append(
                            collectionEntries.Count == 0
                            ? ReturnStatement(
                                LiteralExpression(SyntaxKind.NullLiteralExpression))
                            : ThrowArgumentOutOfRangeStatement()));
                StatementSyntax ThrowArgumentOutOfRangeStatement()
                    =>
                    ThrowStatement(
                        ObjectCreationExpression(
                            ParseName(Names.ArgumentOutOfRangeExceptionFull))
                        .AddArgumentListArguments(
                            Argument(
                                InvocationExpression(
                                    IdentifierName("nameof"))
                                .AddArgumentListArguments(
                                    Argument(indexIdentifierName)))));
                StatementSyntax ReturnIfInRangeExpression(CoreDescriptor.CollectionEntry entry)
                    =>
                    IfStatement(
                        BinaryExpression(
                            SyntaxKind.LessThanExpression,
                            ParenthesizedExpression(
                                AssignmentExpression(
                                    SyntaxKind.SubtractAssignmentExpression,
                                    indexIdentifierName,
                                    CountExpression(entry.IdentifierName))),
                            LiteralExpression(
                                SyntaxKind.NumericLiteralExpression,
                                Literal(0))),
                        ReturnStatement(
                            IndexerOfNodeListExpression(entry.IdentifierName)));
                ExpressionSyntax IndexerOfNodeListExpression(IdentifierNameSyntax identifier)
                    =>
                    ElementAccessExpression(identifier)
                    .AddArgumentListArguments(
                        Argument(
                            BinaryExpression(
                                SyntaxKind.AddExpression,
                                indexIdentifierName,
                                CountExpression(identifier))));
            }
        }

        private ExpressionSyntax CountExpression(IdentifierNameSyntax listName)
        {
            return
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    listName,
                    IdentifierName(Names.Count));
        }
    }
}
