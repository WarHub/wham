﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MoreLinq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class NodeGenerator : NodePartialGeneratorBase
    {
        public const string CoreProperty = "Core";
        public static readonly SyntaxToken CorePropertyIdentifier = Identifier(CoreProperty);
        public static readonly IdentifierNameSyntax CorePropertyIdentifierName = IdentifierName(CoreProperty);
        public static readonly SyntaxToken ValueParamToken = Identifier("value");
        public static readonly IdentifierNameSyntax ValueParamSyntax = IdentifierName(ValueParamToken);

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
            return
                TokenList(Token(IsAbstract ? SyntaxKind.AbstractKeyword : SyntaxKind.SealedKeyword))
                .AddRange(modifiers);
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
                    GenerateSourceNodeOverrideMethods());
        }

        private ConstructorDeclarationSyntax GenerateConstructor()
        {
            const string CoreLocal = "core";
            const string ParentLocal = "parent";
            return
                ConstructorDeclaration(
                    Descriptor.GetNodeTypeName())
                .MutateIf(IsAbstract, x => x.AddModifiers(SyntaxKind.ProtectedKeyword))
                .AddModifiers(SyntaxKind.InternalKeyword)
                .AddParameterListParameters(
                    Parameter(
                        Identifier(CoreLocal))
                    .WithType(Descriptor.CoreType),
                    Parameter(
                        Identifier(ParentLocal))
                    .WithType(
                        IdentifierName(Names.SourceNode)))
                .WithInitializer(
                    ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                    .AddArgumentListArguments(
                        GetBaseArguments()))
                .AddBodyStatements(
                    GetBodyStatements());
            IEnumerable<ArgumentSyntax> GetBaseArguments()
            {
                if (IsDerived)
                {
                    yield return Argument(IdentifierName(CoreLocal));
                }
                yield return Argument(IdentifierName(ParentLocal));
            }
            IEnumerable<StatementSyntax> GetBodyStatements()
            {
                yield return
                    CorePropertyIdentifierName
                    .Assign(
                        IdentifierName(CoreLocal))
                    .AsStatement();
                foreach (var entry in Descriptor.DeclaredEntries.Where(x => !x.IsSimple))
                {
                    yield return CreateNotSimpleInitialization(entry);
                }
            }

            static StatementSyntax CreateNotSimpleInitialization(CoreDescriptor.Entry entry)
            {
                return
                    CorePropertyIdentifierName
                    .MemberAccess(entry.IdentifierName)
                    .MemberAccess(
                        IdentifierName(entry.IsCollection ? Names.ToListNode : Names.ToNode))
                    .InvokeWithArguments(
                        ThisExpression())
                    .AssignTo(entry.IdentifierName)
                    .AsStatement();
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
                    .MutateIf(IsAbstract, x => x.AddModifiers(SyntaxKind.ProtectedKeyword))
                    .AddModifiers(SyntaxKind.InternalKeyword)
                    .MutateIf(IsDerived, x => x.AddModifiers(SyntaxKind.NewKeyword))
                    .AddAccessorListAccessors(
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonTokenDefault())
                    .MutateIf(IsAbstract, x =>
                        x.AddAttributeListAttribute(DebuggerBrowsableNeverAttribute));
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
                        CorePropertyIdentifierName)
                    .AddAttributeListAttribute(DebuggerBrowsableNeverAttribute);
            }

            PropertyDeclarationSyntax CreateSimpleProperty(CoreDescriptor.SimpleEntry entry)
            {
                return
                    PropertyDeclaration(
                        entry.Type,
                        entry.Identifier)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .MutateIf(IsAbstract, x => x.AddModifiers(SyntaxKind.VirtualKeyword))
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
                    .MutateIf(IsAbstract, x => x.AddModifiers(SyntaxKind.VirtualKeyword))
                    .AddAccessorListAccessors(
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonTokenDefault());
            }

            PropertyDeclarationSyntax CreateCollectionProperty(CoreDescriptor.CollectionEntry entry)
            {
                return
                    PropertyDeclaration(
                        entry.GetListNodeTypeIdentifierName(),
                        entry.Identifier)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .MutateIf(IsAbstract, x => x.AddModifiers(SyntaxKind.VirtualKeyword))
                    .AddAccessorListAccessors(
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonTokenDefault());
            }
        }

        private IEnumerable<MethodDeclarationSyntax> GenerateMutatorMethods()
        {
            const string CoreParameter = "core";
            yield return UpdateWithMethod();
            if (IsDerived)
            {
                yield return DerivedUpdateWithMethod();
            }
            foreach (var withMethod in Descriptor.Entries
                .Select(WithForSimpleEntry, WithForComplexEntry, WithForCollectionEntry))
            {
                yield return withMethod;
            }
            MethodDeclarationSyntax UpdateWithMethod()
            {
                var methodSignatureBase =
                    MethodDeclaration(
                        Descriptor.GetNodeTypeIdentifierName(),
                        Names.UpdateWith)
                    .MutateIf(IsAbstract, x => x.AddModifiers(SyntaxKind.ProtectedKeyword))
                    .AddModifiers(SyntaxKind.InternalKeyword)
                    .AddParameterListParameters(
                        Parameter(
                            Identifier(CoreParameter))
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
                            ConditionalExpression(
                                BinaryExpression(
                                    SyntaxKind.EqualsExpression,
                                    ThisExpression().MemberAccess(CorePropertyIdentifierName),
                                    IdentifierName(CoreParameter)),
                                ThisExpression(),
                                ObjectCreationExpression(
                                    Descriptor.GetNodeTypeIdentifierName())
                                .InvokeWithArguments(
                                    IdentifierName(CoreParameter),
                                    LiteralExpression(SyntaxKind.NullLiteralExpression)))));
            }
            MethodDeclarationSyntax DerivedUpdateWithMethod()
            {
                return
                    MethodDeclaration(
                        IdentifierName(BaseType.Name.GetNodeTypeNameCore()),
                        Names.UpdateWith)
                    .AddModifiers(
                        SyntaxKind.ProtectedKeyword,
                        SyntaxKind.InternalKeyword,
                        SyntaxKind.SealedKeyword,
                        SyntaxKind.OverrideKeyword)
                    .AddParameterListParameters(
                        Parameter(
                            Identifier(CoreParameter))
                        .WithType(
                            IdentifierName(BaseType.Name)))
                    .AddBodyStatements(
                        ReturnStatement(
                            IdentifierName(Names.UpdateWith)
                            .InvokeWithArguments(
                                CastExpression(
                                    Descriptor.CoreType,
                                    IdentifierName(CoreParameter)))));
            }
            MethodDeclarationSyntax WithBasicPart(CoreDescriptor.Entry entry)
            {
                return
                    MethodDeclaration(
                        Descriptor.GetNodeTypeIdentifierName(),
                        Names.WithPrefix + entry.Identifier)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .MutateIf(
                        IsDerived && !entry.Symbol.ContainingType.Equals(Descriptor.TypeSymbol, SymbolEqualityComparer.Default),
                        x => x.AddModifiers(SyntaxKind.NewKeyword));
            }
            MethodDeclarationSyntax WithForSimpleEntry(CoreDescriptor.SimpleEntry entry)
            {
                return
                    WithBasicPart(entry)
                    .AddParameterListParameters(
                        Parameter(ValueParamToken)
                        .WithType(entry.Type))
                    .WithExpressionBodyFull(
                        IdentifierName(Names.UpdateWith)
                        .InvokeWithArguments(
                            CorePropertyIdentifierName
                            .MemberAccess(
                                IdentifierName(Names.WithPrefix + entry.Identifier))
                            .InvokeWithArguments(ValueParamSyntax)));
            }
            MethodDeclarationSyntax WithForComplexEntry(CoreDescriptor.ComplexEntry entry)
            {
                return
                    WithBasicPart(entry)
                    .AddParameterListParameters(
                        Parameter(ValueParamToken)
                        .WithType(entry.GetNodeTypeIdentifierName()))
                    .WithExpressionBodyFull(
                        IdentifierName(Names.UpdateWith)
                        .InvokeWithArguments(
                            CorePropertyIdentifierName
                            .MemberAccess(
                                IdentifierName(Names.WithPrefix + entry.Identifier))
                            .InvokeWithArguments(
                                ValueParamSyntax.ConditionalMemberAccess(CorePropertyIdentifierName))));
            }
            MethodDeclarationSyntax WithForCollectionEntry(CoreDescriptor.CollectionEntry entry)
            {
                return
                    WithBasicPart(entry)
                    .AddParameterListParameters(
                        Parameter(ValueParamToken)
                        .WithType(
                            entry.GetListNodeTypeIdentifierName()))
                    .AddBodyStatements(
                        ReturnStatement(
                            IdentifierName(Names.UpdateWith)
                            .InvokeWithArguments(
                                CorePropertyIdentifierName
                                .MemberAccess(
                                    IdentifierName(Names.WithPrefix + entry.Identifier))
                                .InvokeWithArguments(
                                    ValueParamSyntax
                                    .MemberAccess(
                                        IdentifierName(Names.ToCoreArray))
                                    .InvokeWithArguments()))));
            }
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateSourceNodeOverrideMethods()
        {
            if (IsAbstract)
            {
                yield break;
            }
            var children = Descriptor.Entries.Where(x => !x.IsSimple).ToImmutableArray();
            yield return ChildrenCount();
            if (children.IsEmpty)
            {
                yield break;
            }
            yield return ChildrenInfos();
            yield return Children();
            yield return GetChild();
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
                    .WithExpressionBodyFull(
                        LiteralExpression(SyntaxKind.NumericLiteralExpression,
                        Literal(children.Length)));
            }
            MemberDeclarationSyntax ChildrenInfos()
            {
                return
                    MethodDeclaration(
                        CreateIEnumerableOf(
                            IdentifierName(Names.ChildInfo)),
                        Names.ChildrenInfos)
                    .AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.OverrideKeyword)
                    .AddBodyStatements(
                        children.Select(CreateStatement));

                static StatementSyntax CreateStatement(CoreDescriptor.Entry entry)
                {
                    return
                        YieldStatement(
                            SyntaxKind.YieldReturnStatement,
                            ObjectCreationExpression(
                                IdentifierName(Names.ChildInfo))
                            .InvokeWithArguments(
                                IdentifierName("nameof")
                                .InvokeWithArguments(entry.IdentifierName),
                                entry.IdentifierName));
                }
            }
            MemberDeclarationSyntax Children()
            {
                return
                    MethodDeclaration(
                        CreateIEnumerableOf(
                            IdentifierName(Names.SourceNode)),
                        Names.Children)
                    .AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.OverrideKeyword)
                    .AddBodyStatements(
                        children.Select(CreateStatement));

                static StatementSyntax CreateStatement(CoreDescriptor.Entry entry)
                {
                    return
                        YieldStatement(
                            SyntaxKind.YieldReturnStatement,
                            entry.IdentifierName);
                }
            }
            MemberDeclarationSyntax GetChild()
            {
                const string IndexParam = "index";
                var indexIdentifierName = IdentifierName(IndexParam);
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
                            Identifier(IndexParam))
                        .WithType(
                            PredefinedType(
                                Token(SyntaxKind.IntKeyword))))
                    .AddBodyStatements(
                        /* parens required for Roslyn 3.1 until https://github.com/dotnet/roslyn/pull/37301 gets published */
                        SwitchStatement(indexIdentifierName)
                        .WithOpenParenToken(Token(SyntaxKind.OpenParenToken))
                        .WithCloseParenToken(Token(SyntaxKind.CloseParenToken))
                        .AddSections(
                            CreateSwitchSections()
                            .ToArray()));
                IEnumerable<SwitchSectionSyntax> CreateSwitchSections()
                {
                    foreach (var (index, child) in children.Index())
                    {
                        yield return
                            SwitchSection()
                            .AddLabels(
                                CaseSwitchLabel(
                                    LiteralExpression(
                                        SyntaxKind.NumericLiteralExpression,
                                        Literal(index))))
                            .AddStatements(
                                ReturnStatement(
                                    child.IdentifierName));
                    }
                    yield return
                        SwitchSection()
                        .AddLabels(
                            DefaultSwitchLabel())
                        .AddStatements(
                            ThrowArgumentOutOfRangeStatement());
                }
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
            }

            static QualifiedNameSyntax CreateIEnumerableOf(TypeSyntax typeParameter)
            {
                return
                    QualifiedName(
                        ParseName(Names.IEnumerableGenericNamespace),
                        GenericName(
                            Identifier(Names.IEnumerableGeneric))
                        .AddTypeArgumentListArguments(typeParameter));
            }
        }
    }
}
