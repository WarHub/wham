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
        public static readonly IdentifierNameSyntax Core = IdentifierName(CoreProperty);
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

        private static TypeSyntax GetNodePropertyType(CoreChildBase entry) => entry switch
        {
            CoreObjectChild { Symbol: { Type: { NullableAnnotation: NullableAnnotation.Annotated } } } complex =>
                complex.GetNodeTypeIdentifierName().Nullable(),
            CoreObjectChild complex => complex.GetNodeTypeIdentifierName(),
            CoreListChild collection => collection.GetListNodeTypeIdentifierName(),
            _ => entry.Type
        };

        private ConstructorDeclarationSyntax GenerateConstructor()
        {
            const string CoreLocal = "core";
            const string ParentLocal = "parent";
            var core = IdentifierName(CoreLocal);
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
                    Core
                    .Assign(core)
                    .AsStatement();
                foreach (var entry in Descriptor.Entries.Where(x => x is not CoreValueChild))
                {
                    yield return CreateNotSimpleInitialization(entry);
                }
            }

            StatementSyntax CreateNotSimpleInitialization(CoreChildBase entry)
            {
                var value = entry switch
                {
                    CoreObjectChild { IsNullable: true } =>
                        core.Dot(entry.IdentifierName).QuestionDot(Names.ToNode).Invoke(ThisExpression()),
                    CoreObjectChild => core.Dot(entry.IdentifierName).Dot(Names.ToNode).Invoke(ThisExpression()),
                    CoreListChild => core.Dot(entry.IdentifierName).Dot(Names.ToListNode).Invoke(ThisExpression()),
                    _ => throw new InvalidOperationException("Object or List child only.")
                };
                return entry.IdentifierName.Assign(value).AsStatement();
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
                        .MutateIf(entry.Symbol.IsOverride || !entry.IsDeclared, x => x.AddModifiers(SyntaxKind.OverrideKeyword))
                        .Mutate(x => entry is CoreValueChild
                        ? x.WithExpressionBodyFull(
                            Core
                            .Dot(entry.IdentifierName))
                        : x.AddAccessorListAccessors(
                            AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken()));
                }
            }
            else
            {
                foreach (var entry in Descriptor.Entries.Where(x => x.IsDeclared))
                {
                    yield return
                        PropertyDeclaration(
                            GetNodePropertyType(entry),
                            entry.Identifier)
                        .AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.AbstractKeyword)
                        .AddAccessorListAccessors(
                            AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken());
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
                        .Dot(
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
                        .WithSemicolonToken())
                    .MutateIf(IsAbstract, x =>
                        x.AddAttributeListAttributes(DebuggerBrowsableNeverAttribute));
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
                        Core)
                    .AddAttributeListAttributes(DebuggerBrowsableNeverAttribute);
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
                                ThisExpression().Dot(Core),
                                IdentifierName(CoreParameter)),
                            ThisExpression(),
                            ObjectCreationExpression(
                                Descriptor.GetNodeTypeIdentifierName())
                            .Invoke(
                                IdentifierName(CoreParameter),
                                LiteralExpression(SyntaxKind.NullLiteralExpression))));
            }
            MethodDeclarationSyntax WithMethod(CoreChildBase entry)
            {
                var signature =
                    MethodDeclaration(
                        Descriptor.GetNodeTypeIdentifierName(),
                        Names.WithPrefix + entry.Identifier)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .MutateIf(IsAbstract, x => x.AddModifiers(SyntaxKind.AbstractKeyword))
                    .MutateIf(entry.Symbol.IsOverride || !entry.IsDeclared, x => x.AddModifiers(SyntaxKind.OverrideKeyword))
                    .AddParameterListParameters(
                        Parameter(ValueParamToken)
                        .WithType(GetNodePropertyType(entry)));
                if (IsAbstract)
                {
                    return signature.WithSemicolonToken();
                }
                var coreWithArg = entry switch
                {
                    CoreObjectChild { IsNullable: true } => ValueParamSyntax.QuestionDot(Core),
                    CoreObjectChild => ValueParamSyntax.Dot(Core),
                    CoreListChild =>
                        ValueParamSyntax.Dot(
                            IdentifierName(Names.ToCoreArray))
                        .Invoke(),
                    _ => ValueParamSyntax
                };
                return signature
                    .WithExpressionBodyFull(
                        IdentifierName(Names.UpdateWith)
                        .Invoke(
                            Core
                            .With(entry.IdentifierName.Assign(coreWithArg))));
            }
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateSourceNodeOverrideMethods()
        {
            if (IsAbstract)
            {
                yield break;
            }
            var children = Descriptor.Entries.Where(x => x is not CoreValueChild).ToImmutableArray();
            var anyNullable = children.Any(x => x is CoreObjectChild { IsNullable: true });
            if (!anyNullable)
            {
                // otherwise default SourceNode implementation is good enough
                yield return ChildrenCount();
            }
            if (children.IsEmpty)
            {
                yield break;
            }
            yield return ChildrenInfos();
            yield return Children();
            if (!anyNullable)
            {
                // otherwise default SourceNode implementation is good enough
                yield return GetChild();
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
                    .WithExpressionBodyFull(children.Length.ToLiteralExpression());
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

                StatementSyntax CreateStatement(CoreChildBase entry)
                {
                    var yieldReturn =
                        YieldStatement(
                            SyntaxKind.YieldReturnStatement,
                            ObjectCreationExpression(
                                IdentifierName(Names.ChildInfo))
                            .Invoke(
                                IdentifierName("nameof")
                                .Invoke(entry.IdentifierName),
                                entry.IdentifierName));
                    return entry switch
                    {
                        CoreObjectChild { IsNullable: true } =>
                            IfStatement(
                                entry.IdentifierName.IsNot(ConstantPattern(Null)),
                                yieldReturn),
                        _ => yieldReturn
                    };
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

                StatementSyntax CreateStatement(CoreChildBase entry)
                {
                    var yieldReturn =
                        YieldStatement(
                            SyntaxKind.YieldReturnStatement,
                            entry.IdentifierName);
                    return entry switch
                    {
                        CoreObjectChild { IsNullable: true } =>
                            IfStatement(
                                entry.IdentifierName.IsNot(ConstantPattern(Null)),
                                yieldReturn),
                        _ => yieldReturn
                    };
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
                                    index.ToLiteralExpression()))
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
                            IdentifierName(Names.ArgumentOutOfRangeException))
                        .AddArgumentListArguments(
                            Argument(
                                InvocationExpression(
                                    IdentifierName("nameof"))
                                .AddArgumentListArguments(
                                    Argument(indexIdentifierName)))));
            }

            static NameSyntax CreateIEnumerableOf(TypeSyntax typeParameter)
            {
                return
                    GenericName(
                        Identifier(Names.IEnumerable))
                    .AddTypeArgumentListArguments(typeParameter);
            }
        }
    }
}
