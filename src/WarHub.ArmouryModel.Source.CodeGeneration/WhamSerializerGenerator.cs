using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MoreLinq.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class WhamSerializerGenerator
    {
        private const string WhamCoreXmlSerializationReaderName = "WhamCoreXmlSerializationReader";
        private const string WhamCoreXmlSerializationWriterName = "WhamCoreXmlSerializationWriter";
        private const string XmlEnumAttributeMetadataName = "System.Xml.Serialization.XmlEnumAttribute";

        public WhamSerializerGenerator(Compilation compilation, ImmutableArray<CoreDescriptor> descriptors)
        {
            Compilation = compilation;
            Descriptors = descriptors;
            SealedCores = descriptors.Where(x => x.TypeSymbol.IsSealed).ToImmutableArray();
            RootCores = SealedCores
                .Where(x => x.Xml.IsRoot)
                .ToImmutableArray();
            XmlEnumSymbol = Compilation.GetTypeByMetadataNameOrThrow(XmlEnumAttributeMetadataName);
        }

        public Compilation Compilation { get; }

        public ImmutableArray<CoreDescriptor> Descriptors { get; }

        public ImmutableArray<CoreDescriptor> SealedCores { get; }

        public ImmutableArray<CoreDescriptor> RootCores { get; }
        public INamedTypeSymbol XmlEnumSymbol { get; }

        internal static CompilationUnitSyntax Generate(Compilation compilation, ImmutableArray<CoreDescriptor> descriptors)
        {
            var generator = new WhamSerializerGenerator(compilation, descriptors);
            return
                CompilationUnit()
                .AddUsings(
                    new[]
                    {
                        Names.NamespaceSystem,
                        Names.NamespaceSystemCollections,
                        Names.NamespaceSystemCollectionsGeneric,
                        Names.NamespaceSystemCollectionsImmutable,
                        Names.NamespaceSystemDiagnostics,
                        Names.NamespaceSystemDiagnosticsCodeAnalysis,
                        Names.NamespaceSystemXmlSerialization,
                        "WarHub.ArmouryModel.Source"
                    }
                    .Select(x => UsingDirective(IdentifierName(x)))
                    .ToArray())
                .AddMembers(
                    NamespaceDeclaration(
                        IdentifierName("WarHub.ArmouryModel.Source.XmlFormat"))
                    .WithLeadingTrivia(
                        Trivia(
                            PragmaWarningDirectiveTrivia(Token(SyntaxKind.DisableKeyword), isActive: true)
                            .AddErrorCodes(DisabledErrorCodes.Select(IdentifierName).ToArray())))
                    .AddMembers(
                        generator.GenerateTypes()))
               .NormalizeWhitespace();
        }

        private static ImmutableArray<string> DisabledErrorCodes { get; } =
            new[]
            {
                "CA1062", // Validate arguments of public methods
                "CA1707", // Identifiers should not contain underscores
                "CA1801", // Review unused parameters
                "CA1822", // Mark members as static
            }.ToImmutableArray();

        private static string ReadRootName(CoreDescriptor core) => "ReadRoot_" + core.Xml.ElementName;

        private static string ReadCoreName(CoreDescriptor core) => "Read_" + core.CoreTypeIdentifier;

        private static string WriteRootName(CoreDescriptor core) => "WriteRoot_" + core.Xml.ElementName;

        private static string WriteCoreName(CoreDescriptor core) => "Write_" + core.CoreTypeIdentifier;

        private CoreDescriptor GetCore(CoreChildBase child)
        {
            var symbol = child switch
            {
                CoreListChild { Symbol: { Type: INamedTypeSymbol named } } => named.TypeArguments[0],
                CoreObjectChild => child.Symbol.Type,
                _ => throw new InvalidOperationException("No cores in child.")
            };
            return SealedCores.First(x => x.TypeSymbol.Equals(symbol, SymbolEqualityComparer.Default));
        }

        IEnumerable<ClassDeclarationSyntax> GenerateTypes()
        {
            yield return CreateXmlSerializationWriter();
            yield return CreateXmlSerializationReader();
            foreach (var root in RootCores)
            {
                yield return CreateXmlSerializer(root);
            }
        }

        private ClassDeclarationSyntax CreateXmlSerializer(CoreDescriptor root)
        {
            return ClassDeclaration(root.CoreTypeIdentifier + nameof(XmlSerializer))
                .AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.SealedKeyword)
                .AddBaseListTypes(
                    SimpleBaseType(
                        IdentifierName(nameof(XmlSerializer))))
                .AddMembers(
                    CanDeserialize(),
                    CreateReader(),
                    CreateWriter(),
                    Deserialize(),
                    Serialize());

            MethodDeclarationSyntax CreateReader() =>
                MethodDeclaration(
                    IdentifierName(nameof(XmlSerializationReader)),
                    "CreateReader")
                .AddModifiers(SyntaxKind.ProtectedKeyword, SyntaxKind.OverrideKeyword)
                .AddBodyStatements(
                    ReturnStatement(
                        ObjectCreationExpression(
                            IdentifierName(WhamCoreXmlSerializationReaderName))
                        .AddArgumentListArguments()));

            MethodDeclarationSyntax CreateWriter() =>
                MethodDeclaration(
                    IdentifierName(nameof(XmlSerializationWriter)),
                    "CreateWriter")
                .AddModifiers(SyntaxKind.ProtectedKeyword, SyntaxKind.OverrideKeyword)
                .AddBodyStatements(
                    ReturnStatement(
                        ObjectCreationExpression(
                            IdentifierName(WhamCoreXmlSerializationWriterName))
                        .AddArgumentListArguments()));

            MethodDeclarationSyntax CanDeserialize() =>
                MethodDeclaration(Bool, "CanDeserialize")
                .AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.OverrideKeyword)
                .AddParameterListParameters(
                    Parameter(
                        Identifier("xmlReader"))
                    .WithType(
                        ParseTypeName("System.Xml.XmlReader")))
                .AddBodyStatements(
                    ReturnStatement(
                        IdentifierName("xmlReader")
                        .MemberAccess(nameof(XmlReader.IsStartElement))
                        .InvokeWithArguments(
                            root.Xml.ElementNameLiteralExpression,
                            root.Xml.NamespaceLiteralExpression)));

            MethodDeclarationSyntax Deserialize() =>
                MethodDeclaration(Object, "Deserialize")
                .AddModifiers(SyntaxKind.ProtectedKeyword, SyntaxKind.OverrideKeyword)
                .AddParameterListParameters(
                    Parameter(
                        Identifier("reader"))
                    .WithType(
                        IdentifierName(nameof(XmlSerializationReader))))
                .AddBodyStatements(
                    ReturnStatement(
                        IdentifierName("reader")
                        .Cast(
                            IdentifierName(WhamCoreXmlSerializationReaderName))
                        .WrapInParentheses()
                        .MemberAccess(
                            ReadRootName(root))
                        .InvokeWithArguments()));

            MethodDeclarationSyntax Serialize() =>
                MethodDeclaration(Void, "Serialize")
                .AddModifiers(SyntaxKind.ProtectedKeyword, SyntaxKind.OverrideKeyword)
                .AddParameterListParameters(
                    Parameter(
                        Identifier("objectToSerialize"))
                    .WithType(Object),
                    Parameter(
                        Identifier("writer"))
                    .WithType(
                        IdentifierName(nameof(XmlSerializationWriter))))
                .AddBodyStatements(
                    IdentifierName("writer")
                    .Cast(
                        IdentifierName(WhamCoreXmlSerializationWriterName))
                    .WrapInParentheses()
                    .MemberAccess(
                        WriteRootName(root))
                    .InvokeWithArguments(
                        IdentifierName("objectToSerialize")
                        .Cast(
                            root.CoreType.Nullable()))
                    .AsStatement());
        }

        private ClassDeclarationSyntax CreateXmlSerializationReader()
        {
            return ClassDeclaration(WhamCoreXmlSerializationReaderName)
                .AddModifiers(SyntaxKind.PublicKeyword)
                .AddBaseListTypes(
                    SimpleBaseType(
                        IdentifierName(nameof(XmlSerializationReader))))
                .AddMembers(
                    MethodDeclaration(Void, "InitCallbacks")
                    .AddModifiers(SyntaxKind.ProtectedKeyword, SyntaxKind.OverrideKeyword)
                    .AddBodyStatements(),
                    MethodDeclaration(Void, "InitIDs")
                    .AddModifiers(SyntaxKind.ProtectedKeyword, SyntaxKind.OverrideKeyword)
                    .AddBodyStatements())
                .AddMembers(
                    RootCores.Select(CreateReadRootMethod))
                .AddMembers(
                    SealedCores.Select(CreateReadCoreMethod));
        }

        private ClassDeclarationSyntax CreateXmlSerializationWriter()
        {
            return ClassDeclaration(WhamCoreXmlSerializationWriterName)
                .AddModifiers(SyntaxKind.PublicKeyword)
                .AddBaseListTypes(
                    SimpleBaseType(
                        IdentifierName(nameof(XmlSerializationWriter))))
                .AddMembers(
                    MethodDeclaration(Void, "InitCallbacks")
                    .AddModifiers(SyntaxKind.ProtectedKeyword, SyntaxKind.OverrideKeyword)
                    .AddBodyStatements())
                .AddMembers(
                    RootCores.Select(CreateWriteRootMethod))
                .AddMembers(
                    SealedCores.Select(CreateWriteCoreMethod))
                .AddMembers(
                    UsedEnumSymbols.OrderBy(x => x.Name).Select(CreateWriteEnumMethod));
        }

        private MethodDeclarationSyntax CreateReadRootMethod(CoreDescriptor core)
        {
            return
                MethodDeclaration(
                    core.CoreType.Nullable(),
                    Identifier(
                        ReadRootName(core)))
                .AddModifiers(SyntaxKind.PublicKeyword)
                .AddParameterListParameters()
                .AddBodyStatements(
                    CreateBody());

            IEnumerable<StatementSyntax> CreateBody()
            {
                yield return ReturnStatement(Null);
            }
        }

        private MethodDeclarationSyntax CreateReadCoreMethod(CoreDescriptor core)
        {
            var isNullable = IdentifierName("isNullable");
            var checkType = IdentifierName("checkType");
            return
                MethodDeclaration(
                    core.CoreType.Nullable(),
                    Identifier(
                        ReadCoreName(core)))
                .AddParameterListParameters(
                    Parameter(isNullable.Identifier).WithType(Bool),
                    Parameter(checkType.Identifier).WithType(Bool))
                .AddBodyStatements(
                    CreateBody());

            IEnumerable<StatementSyntax> CreateBody()
            {
                yield return ReturnStatement(Null);
            }
        }

        private MethodDeclarationSyntax CreateWriteRootMethod(CoreDescriptor core)
        {
            var n = core.Xml.ElementNameLiteralExpression;
            var ns = core.Xml.NamespaceLiteralExpression;
            var o = IdentifierName("o");
            return
                MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.VoidKeyword)),
                    Identifier(
                        WriteRootName(core)))
                .AddModifiers(SyntaxKind.PublicKeyword)
                .AddParameterListParameters(
                    Parameter(o.Identifier).WithType(core.CoreType.Nullable()))
                .AddBodyStatements(
                    CreateBody());

            IEnumerable<StatementSyntax> CreateBody()
            {
                yield return "WriteStartDocument".Invoke().AsStatement();
                yield return
                    IfStatement(
                        o.Is(ConstantPattern(Null)),
                        Block(
                            "WriteEmptyTag".Invoke(n, ns).AsStatement(),
                            ReturnStatement()));
                yield return "TopLevelElement".Invoke().AsStatement();
                yield return WriteCoreName(core).Invoke(n, ns, o, False, False).AsStatement();
            }
        }

        private LiteralExpressionSyntax Null { get; } =
            LiteralExpression(SyntaxKind.NullLiteralExpression, Token(SyntaxKind.NullKeyword));

        private LiteralExpressionSyntax True { get; } =
            LiteralExpression(SyntaxKind.TrueLiteralExpression, Token(SyntaxKind.TrueKeyword));

        private LiteralExpressionSyntax False { get; } =
            LiteralExpression(SyntaxKind.FalseLiteralExpression, Token(SyntaxKind.FalseKeyword));

        private LiteralExpressionSyntax Zero { get; } =
            LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0));

        private PredefinedTypeSyntax Long { get; } = PredefinedType(Token(SyntaxKind.LongKeyword));
        private PredefinedTypeSyntax Bool { get; } = PredefinedType(Token(SyntaxKind.BoolKeyword));
        private PredefinedTypeSyntax Void { get; } = PredefinedType(Token(SyntaxKind.VoidKeyword));
        private PredefinedTypeSyntax String { get; } = PredefinedType(Token(SyntaxKind.StringKeyword));
        private PredefinedTypeSyntax Object { get; } = PredefinedType(Token(SyntaxKind.ObjectKeyword));
        private static ExpressionSyntax Not(ExpressionSyntax e) => PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, e);

        private MethodDeclarationSyntax CreateWriteCoreMethod(CoreDescriptor core)
        {
            var n = IdentifierName("n");
            var ns = IdentifierName("ns");
            var o = IdentifierName("o");
            var isNullable = IdentifierName("isNullable");
            var needType = IdentifierName("needType");
            return
                MethodDeclaration(Void, Identifier(WriteCoreName(core)))
                .AddParameterListParameters(
                    Parameter(n.Identifier).WithType(String),
                    Parameter(ns.Identifier).WithType(String),
                    Parameter(o.Identifier).WithType(core.CoreType.Nullable()),
                    Parameter(isNullable.Identifier).WithType(Bool),
                    Parameter(needType.Identifier).WithType(Bool))
                .AddBodyStatements(
                    CreateBody());

            IEnumerable<StatementSyntax> CreateBody()
            {
                yield return
                    IfStatement(
                        o.Is(ConstantPattern(Null)),
                        Block(
                            IfStatement(isNullable, "WriteNullTagLiteral".Invoke(n, ns).AsStatement()),
                            ReturnStatement()));
                yield return
                    IfStatement(
                        Not(needType).And(
                            o.MemberAccess(nameof(object.GetType)).InvokeWithArguments()
                            .OpNotEquals(
                                TypeOfExpression(core.CoreType))),
                        statement: ThrowStatement("CreateUnknownTypeException".Invoke(o))
                        );
                yield return "WriteStartElement".Invoke(n, ns, o, False, Null).AsStatement();
                yield return
                    IfStatement(needType, "WriteXsiType".Invoke(core.Xml.ElementNameLiteralExpression, ns).AsStatement());
                var (attributes, elements) = core.Entries.Partition(x => x.Xml.Kind == XmlNodeKind.Attribute);
                foreach (var attribute in attributes)
                {
                    var n = attribute.Xml.ElementNameLiteralExpression;
                    var ns = attribute.Xml.NamespaceLiteralExpression;
                    var value = TransformToString(attribute);
                    yield return "WriteAttribute".Invoke(n, ns, value).AsStatement();
                }
                var textCount = elements.Count(x => x.Xml.Kind == XmlNodeKind.TextContent);
                if (textCount > 0 && (textCount > 1 || elements.Count() > 1))
                    yield return
                        EmptyStatement()
                        .WithLeadingTrivia(
                            TriviaList(
                                Error("Only one property can be the text content of an element.")));
                foreach (var element in elements)
                {
                    var n = element.Xml.ElementNameLiteralExpression;
                    var value = o.MemberAccess(element.IdentifierName);
                    yield return element switch
                    {
                        { Xml: { Kind: XmlNodeKind.TextContent }, Symbol: { Type: { SpecialType: SpecialType.System_String } } } =>
                            IfStatement(
                                value.IsNot(ConstantPattern(Null)),
                                "WriteValue".Invoke(value).AsStatement()),

                        CoreValueChild { Xml: { Kind: XmlNodeKind.Element } } =>
                            "WriteElementString".Invoke(n, ns, TransformToString(element)).AsStatement(),

                        CoreObjectChild { Xml: { Kind: XmlNodeKind.Element } } =>
                            WriteCoreName(GetCore(element)).Invoke(n, ns, value, False, False).AsStatement(),

                        CoreListChild { Xml: { Kind: XmlNodeKind.Array } } list =>
                            CreateChildListWrite(list),

                        _ => EmptyStatement().WithLeadingTrivia(TriviaList(Error("Can't generate code for " + element.Identifier)))
                    };
                }
                yield return "WriteEndElement".Invoke(o).AsStatement();
            }

            ExpressionSyntax TransformToString(CoreChildBase child)
            {
                var propValue = o.MemberAccess(child.IdentifierName);
                return child.Symbol.Type switch
                {
                    { SpecialType: SpecialType.System_String } => propValue,
                    { TypeKind: TypeKind.Enum } enumSymbol => CreateWriteEnumCall(enumSymbol, propValue),
                    _ => XmlConvertToString(propValue)
                };
            }

            StatementSyntax CreateChildListWrite(CoreListChild list)
            {
                var listCore = GetCore(list);
                var n = list.Xml.ElementNameLiteralExpression;
                var subElementName = listCore.Xml.ElementNameLiteralExpression;
                var a = IdentifierName("a");
                var i = IdentifierName("i");
                return
                    Block(
                        a.Identifier.InitVar(o.MemberAccess(list.IdentifierName)).AsStatement(),
                        IfStatement(
                            Not(a.MemberAccess(nameof(ImmutableArray<int>.IsDefaultOrEmpty))),
                            Block(
                                IfNotEmptyStatements())));
                IEnumerable<StatementSyntax> IfNotEmptyStatements()
                {
                    yield return "WriteStartElement".Invoke(n, ns, Null, False).AsStatement();
                    yield return
                        ForStatement(
                            declaration: i.Identifier.InitVar(Zero),
                            initializers: default,
                            condition: i.OpLessThan(a.MemberAccess("Length")),
                            incrementors: SingletonSeparatedList<ExpressionSyntax>(
                                PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, i)),
                            statement:
                                WriteCoreName(listCore)
                                .Invoke(subElementName, ns, a.ElementAccess(i), True, False)
                                .AsStatement());
                    yield return "WriteEndElement".Invoke().AsStatement();
                }
            }
        }

        HashSet<INamedTypeSymbol> UsedEnumSymbols { get; } = new HashSet<INamedTypeSymbol>();

        static string WriteEnumName(INamedTypeSymbol symbol) => "WriteEnum_" + symbol.Name;
        static string ReadEnumName(INamedTypeSymbol symbol) => "ReadEnum_" + symbol.Name;

        private ExpressionSyntax CreateWriteEnumCall(ITypeSymbol enumSymbol, ExpressionSyntax value)
        {
            if (enumSymbol is not INamedTypeSymbol { TypeKind: TypeKind.Enum } symbol)
                throw new InvalidOperationException("Not an enum");
            UsedEnumSymbols.Add(symbol);
            return WriteEnumName(symbol).Invoke(value);
        }

        private MethodDeclarationSyntax CreateWriteEnumMethod(INamedTypeSymbol symbol)
        {
            var v = IdentifierName("v");
            var type = IdentifierName(symbol.Name);
            var valueToString = v.Cast(Long)
                .WrapInParentheses()
                .MemberAccess("ToString")
                .InvokeWithArguments(InvariantCulture);
            var typeFullName = TypeOfExpression(type).MemberAccess(nameof(Type.FullName));
            return
                MethodDeclaration(String, WriteEnumName(symbol))
                .AddParameterListParameters(
                    Parameter(v.Identifier).WithType(type))
                .WithExpressionBodyFull(
                    SwitchExpression(v)
                    .AddArms(
                        symbol.GetMembers()
                        .Where(x => x.Kind == SymbolKind.Field)
                        .Select(ToArm)
                        .ToArray())
                    .AddArms(
                        SwitchExpressionArm(
                            DiscardPattern(),
                            ThrowExpression(
                                "CreateInvalidEnumValueException".Invoke(valueToString, typeFullName)))));
            SwitchExpressionArmSyntax ToArm(ISymbol symbol)
            {
                var xmlEnum = symbol
                    .GetAttributes()
                    .FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(XmlEnumSymbol, x.AttributeClass));
                var text = xmlEnum switch
                {
                    { ConstructorArguments: { Length: 1 } } => xmlEnum.ConstructorArguments[0].GetStringOrThrow(),
                    { NamedArguments: { Length: 1 } } => xmlEnum.NamedArguments[0].Value.GetStringOrThrow(),
                    _ => symbol.Name
                } ?? throw new InvalidOperationException("XmlEnumAttribute argument must not be null");
                return
                    SwitchExpressionArm(
                        ConstantPattern(type.MemberAccess(symbol.Name)),
                        LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(text)));
            }
        }

        private static SyntaxTrivia Error(string message) =>
            Trivia(
                ErrorDirectiveTrivia(true)
                .WithEndOfDirectiveToken(
                    Token(
                        TriviaList(
                            PreprocessingMessage(message)),
                        SyntaxKind.EndOfDirectiveToken,
                        TriviaList())));

        private NameSyntax InvariantCulture { get; } =
            ParseName("System.Globalization.CultureInfo.InvariantCulture");

        private NameSyntax XmlConvertToStringName { get; }
            = ParseName("System.Xml.XmlConvert.ToString");

        private ExpressionSyntax XmlConvertToString(ExpressionSyntax arg) =>
            XmlConvertToStringName.InvokeWithArguments(arg);
    }
}
