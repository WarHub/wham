using System;
using System.Collections.Generic;
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

        private static TypeSyntax GetNodePropertyType(CoreDescriptor.Entry entry) => entry switch
        {
            CoreDescriptor.SimpleEntry => entry.Type,
            CoreDescriptor.ComplexEntry complex => complex.GetNodeTypeIdentifierName(),
            CoreDescriptor.CollectionEntry collection => collection.GetListNodeTypeIdentifierName(),
            _ => throw new InvalidOperationException("Unknown descriptor entry type.")
        };

        private ConstructorDeclarationSyntax GenerateConstructor()
        {
            const string CoreLocal = "core";
            const string ParentLocal = "parent";
            var coreLocalIdentifierName = IdentifierName(CoreLocal);
            if (IsAbstract)
            {
                return
                    ConstructorDeclaration(
                        Descriptor.GetNodeTypeName())
                    .AddModifiers(SyntaxKind.ProtectedKeyword, SyntaxKind.InternalKeyword)
                    .AddParameterListParameters(
                        Parameter(
                            Identifier(ParentLocal))
                        .WithType(
                            NullableType(
                                IdentifierName(Names.SourceNode))))
                    .WithInitializer(
                        ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                        .AddArgumentListArguments(
                            Argument(
                                IdentifierName(ParentLocal))))
                    .AddBodyStatements();
            }
            else
            {
                return
                    ConstructorDeclaration(
                        Descriptor.GetNodeTypeName())
                    .AddModifiers(SyntaxKind.InternalKeyword)
                    .AddParameterListParameters(
                        Parameter(
                                Identifier(CoreLocal))
                            .WithType(Descriptor.CoreType),
                        Parameter(
                            Identifier(ParentLocal))
                        .WithType(
                            NullableType(
                                IdentifierName(Names.SourceNode))))
                    .WithInitializer(
                        ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                        .AddArgumentListArguments(
                            Argument(
                                IdentifierName(ParentLocal))))
                    .AddBodyStatements(
                        GetBodyStatements());
            }
            IEnumerable<StatementSyntax> GetBodyStatements()
            {
                yield return
                    CorePropertyIdentifierName
                    .Assign(coreLocalIdentifierName)
                    .AsStatement();
                foreach (var entry in Descriptor.Entries.Where(x => !x.IsSimple))
                {
                    yield return CreateNotSimpleInitialization(entry);
                }
            }

            StatementSyntax CreateNotSimpleInitialization(CoreDescriptor.Entry entry)
            {
                return
                    coreLocalIdentifierName
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
            if (!IsAbstract)
            {
                foreach (var entry in Descriptor.Entries)
                {
                    yield return
                        PropertyDeclaration(
                            GetNodePropertyType(entry),
                            entry.Identifier)
                        .AddModifiers(SyntaxKind.PublicKeyword)
                        .MutateIf(entry.IsDerived, x => x.AddModifiers(SyntaxKind.OverrideKeyword))
                        .Mutate(x => entry is CoreDescriptor.SimpleEntry
                        ? x.WithExpressionBodyFull(
                            CorePropertyIdentifierName
                            .MemberAccess(entry.IdentifierName))
                        : x.AddAccessorListAccessors(
                            AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonTokenDefault()));
                }
            }
            else
            {
                foreach (var entry in Descriptor.DeclaredEntries)
                {
                    yield return
                        PropertyDeclaration(
                            GetNodePropertyType(entry),
                            entry.Identifier)
                        .AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.AbstractKeyword)
                        .AddAccessorListAccessors(
                            AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonTokenDefault());
                }
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
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .MutateIf(IsAbstract, x => x.AddModifiers(SyntaxKind.AbstractKeyword))
                    .MutateIf(IsDerived, x => x.AddModifiers(SyntaxKind.OverrideKeyword))
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
        }

        private IEnumerable<MethodDeclarationSyntax> GenerateMutatorMethods()
        {
            const string CoreParameter = "core";
            if (!IsAbstract)
            {
                yield return UpdateWithMethod();
            }
            foreach (var entry in Descriptor.Entries)
            {
                yield return WithMethod(entry);
            }
            MethodDeclarationSyntax UpdateWithMethod()
            {
                return
                    MethodDeclaration(
                        Descriptor.GetNodeTypeIdentifierName(),
                        Names.UpdateWith)
                    .AddModifiers(SyntaxKind.InternalKeyword)
                    .AddParameterListParameters(
                        Parameter(
                            Identifier(CoreParameter))
                        .WithType(Descriptor.CoreType))
                    .WithExpressionBodyFull(
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
                                LiteralExpression(SyntaxKind.NullLiteralExpression))));
            }
            MethodDeclarationSyntax WithMethod(CoreDescriptor.Entry entry)
            {
                var signature =
                    MethodDeclaration(
                        Descriptor.GetNodeTypeIdentifierName(),
                        Names.WithPrefix + entry.Identifier)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .MutateIf(IsAbstract, x => x.AddModifiers(SyntaxKind.AbstractKeyword))
                    .MutateIf(entry.IsDerived, x => x.AddModifiers(SyntaxKind.OverrideKeyword))
                    .AddParameterListParameters(
                        Parameter(ValueParamToken)
                        .WithType(GetNodePropertyType(entry)));
                if (IsAbstract)
                {
                    return signature.WithSemicolonTokenDefault();
                }
                var coreWithArg = entry switch
                {
                    CoreDescriptor.SimpleEntry => ValueParamSyntax,
                    CoreDescriptor.ComplexEntry => ValueParamSyntax.MemberAccess(CorePropertyIdentifierName),
                    CoreDescriptor.CollectionEntry =>
                        ValueParamSyntax.MemberAccess(
                            IdentifierName(Names.ToCoreArray))
                        .InvokeWithArguments(),
                    _ => throw new NotImplementedException()
                };
                return signature
                    .WithExpressionBodyFull(
                        IdentifierName(Names.UpdateWith)
                        .InvokeWithArguments(
                            CorePropertyIdentifierName
                            .MemberAccess(
                                IdentifierName(Names.WithPrefix + entry.Identifier))
                            .InvokeWithArguments(coreWithArg)));
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
