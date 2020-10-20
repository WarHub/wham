using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static MoreLinq.Extensions.AggregateRightExtension;
using static MoreLinq.Extensions.IndexExtension;
using static MoreLinq.Extensions.TagFirstLastExtension;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal partial class WhamSerializerGenerator
    {
        private XmlReaderMembers Reader { get; } = new XmlReaderMembers();
        private XmlNodeTypeMembers XmlNodeType { get; } = new XmlNodeTypeMembers();
        private XmlConvertMembers XmlConvert { get; } = new XmlConvertMembers();
        private ExpressionSyntax EmptyString { get; } = "".ToLiteralExpression();
        private ExpressionSyntax ReadNullCache { get; } = "ReadNull".Invoke();

        private Dictionary<string, IdentifierNameSyntax> ReaderAtomizedStrings { get; } =
            new Dictionary<string, IdentifierNameSyntax>();

        private ExpressionSyntax ReadNull() => ReadNullCache;

        private static string ReadRootName(CoreDescriptor core) => "ReadRoot_" + core.Xml.ElementName;

        private static string ReadCoreName(CoreDescriptor core) => "Read_" + core.CoreTypeIdentifier;

        private static string ReadEnumName(INamedTypeSymbol symbol) => "ReadEnum_" + symbol.Name;

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
                var n = ReaderAtomized(core.Xml.ElementName);
                var ns = ReaderAtomized(core.Xml.Namespace ?? "", core.Xml.ElementName + "Namespace");
                yield return Reader.MoveToContent();
                yield return
                    IfStatement(
                        Reader.NodeType.OpEquals(XmlNodeType.Element),
                        Block(new StatementSyntax[]
                        {
                            IfStatement(
                                CorrectNames(n, ns),
                                ReturnStatement(
                                    ReadCoreName(core).Invoke(ns, False, True))),
                            ThrowStatement("CreateUnknownNodeException".Invoke())
                        }));
                yield return UnknownNode(Null, ns, core.Xml.ElementName);
                yield return ReturnStatement(Null);
            }
        }

        private MethodDeclarationSyntax CreateReadCoreMethod(CoreDescriptor core)
        {
            var n = ReaderAtomized(core.Xml.ElementName);
            // parameters
            var ns = IdentifierName("ns");
            var isNullable = IdentifierName("isNullable");
            var checkType = IdentifierName("checkType");
            // locals
            var isNull = IdentifierName("isNull");
            var xsiType = IdentifierName("xsiType");
            var empty = IdentifierName("empty");
            var paramsRead = IdentifierName("paramsRead");
            var values = core.Entries.ToImmutableDictionary(x => x, x => IdentifierName("o" + x.Identifier));
            // others
            var possibleAttributeNames = core.Entries
                .Where(x => x.Xml.Kind == XmlNodeKind.Attribute)
                .Select(x => x.Xml.ElementName)
                .ToArray();
            var textEntries = core.Entries
                .Where(x => x.Xml.Kind is XmlNodeKind.TextContent)
                .ToImmutableArray();
            var elementEntries = core.Entries
                .Where(x => x.Xml.Kind is XmlNodeKind.Array or XmlNodeKind.Element)
                .ToImmutableArray();
            var possibleChildElementNames = elementEntries.Select(x => x.Xml.ElementName).ToArray();
            return
                MethodDeclaration(
                    core.CoreType.Nullable(),
                    Identifier(
                        ReadCoreName(core)))
                .AddParameterListParameters(
                    Parameter(ns.Identifier).WithType(String),
                    Parameter(isNullable.Identifier).WithType(Bool),
                    Parameter(checkType.Identifier).WithType(Bool))
                .AddBodyStatements(
                    CreateBody());

            IEnumerable<StatementSyntax> CreateBody()
            {
                // var isNull = isNullable && ReadNull();
                yield return isNull.InitVarStatement(isNullable.And("ReadNull".Invoke()));
                yield return IfGetXsiInvalidThenThrow(n, ns, checkType, xsiType);
                yield return IfStatement(isNull, ReturnStatement(Null));
                yield return empty.InitVarStatement(core.CoreType.Dot("Empty"));
                // var paramsRead = new bool[paramCount];
                yield return paramsRead.InitVarStatement(NewBoolArray(core));
                foreach (var property in core.Entries)
                {
                    // var oProperty = empty.Property;
                    yield return values[property].InitVarStatement(empty.Dot(property.IdentifierName));
                }
                var ifXmlnsAttribute =
                    IfStatement(
                        Not("IsXmlnsAttribute".Invoke(Reader.Name)),
                        UnknownNode(Null, null, possibleAttributeNames));
                yield return
                    WhileStatement(
                        Reader.MoveToNextAttribute(),
                        core.Entries.Index()
                        .Where(x => x.Value.Xml.Kind == XmlNodeKind.Attribute)
                        .Select(x => ReadElementAttribute(x.Key, x.Value))
                        .Append(ifXmlnsAttribute)
                        .AggregateRight((x, y) => x.WithElse(ElseClause(y))));
                yield return Reader.MoveToElement();
                yield return
                    IfStatement(
                        Reader.IsEmptyElement,
                        Reader.Skip(),
                        ElseClause(
                            Block(ReadElementChildContent())));
                yield return
                    ReturnStatement(
                        core.CoreType.ObjectCreationWithInitializer(
                            // Prop = oProp
                            core.Entries.Select(x => x.IdentifierName.Assign(values[x])).ToArray()));
            }
            IEnumerable<StatementSyntax> ReadElementChildContent()
            {
                if (textEntries.Length > 0 && (textEntries.Length > 1 || elementEntries.Length > 0))
                {
                    yield return EmptyStatement().WithLeadingTrivia(Error("Can only contain single text node and no elements."));
                    yield break;
                }
                yield return Reader.ReadStartElement();
                yield return Reader.MoveToContent();
                // locals
                var whileIterations = IdentifierName("whileIterations");
                var readerCount = IdentifierName("readerCount");
                yield return whileIterations.InitVarStatement(0.ToLiteralExpression());
                yield return readerCount.InitVarStatement(IdentifierName("ReaderCount"));
                var nodeTypeHandling = (elements: elementEntries.Length, texts: textEntries.Length) switch
                {
                    (0, 0) => UnknownNodeLocal(),
                    (0, 1) =>
                    IfStatement(
                        Reader.NodeType.Is(
                            ConstantPattern(XmlNodeType.Text)
                            .Or(ConstantPattern(XmlNodeType.CDATA))
                            .Or(ConstantPattern(XmlNodeType.Whitespace))
                            .Or(ConstantPattern(XmlNodeType.SignificantWhitespace))),
                        values[textEntries[0]]
                        .Assign("ReadString".Invoke(Null, False))
                        .AsStatement(),
                        ElseClause(UnknownNodeLocal())),
                    _ =>
                    IfStatement(
                        Reader.NodeType.OpEquals(XmlNodeType.Element),
                        Block(
                            core.Entries.Index()
                            .Select(x => x.Value.Xml.Kind switch
                            {
                                XmlNodeKind.Element => ReadSingleElement(x.Key, x.Value),
                                XmlNodeKind.Array => ReadArrayElement(x.Value),
                                _ => null
                            })
                            .Where(x => x is not null)
                            .AggregateRight(
                                seed: UnknownNodeLocal(),
                                // nullability suppression ok because of Where(not null) above
                                (x, y) => x!.WithElse(ElseClause(y)))),
                        ElseClause(UnknownNodeLocal()))
                };
                yield return
                    WhileStatement(
                        Reader.NodeType.OpNotEquals(XmlNodeType.EndElement)
                        .And(Reader.NodeType.OpNotEquals(XmlNodeType.None)),
                        Block(
                            nodeTypeHandling,
                            Reader.MoveToContent(),
                            CheckReaderCount(whileIterations, readerCount)));
                yield return "ReadEndElement".Invoke().AsStatement();

                StatementSyntax UnknownNodeLocal() =>
                    UnknownNode(Null, ns, possibleChildElementNames);
            }
            IfStatementSyntax ReadElementAttribute(int i, CoreChildBase entry)
            {
                var n = ReaderAtomized(entry.Xml.ElementName);
                var ns = ReaderAtomized(entry.Xml.Namespace);
                var paramsReadOfI = paramsRead.ElementAccess(i.ToLiteralExpression());
                var value = entry.Symbol.Type switch
                {
                    { SpecialType: SpecialType.System_String } => Reader.Value,
                    { SpecialType: SpecialType.System_Boolean } => XmlConvert.ToBoolean(Reader.Value),
                    { SpecialType: SpecialType.System_Int32 } => XmlConvert.ToInt32(Reader.Value),
                    { SpecialType: SpecialType.System_Decimal } => XmlConvert.ToDecimal(Reader.Value),
                    { TypeKind: TypeKind.Enum } typeSymbol => CreateReadEnumCall(typeSymbol, Reader.Value),
                    _ => Null.WithLeadingTrivia(Error("Unsupported child attribute type"))
                };
                return
                    IfStatement(
                        Not(paramsReadOfI).And(CorrectNames(n, ns)),
                        Tuple(paramsReadOfI, values[entry])
                        .Assign(
                            Tuple(True, value))
                        .AsStatement());
            }
            IfStatementSyntax ReadSingleElement(int i, CoreChildBase entry)
            {
                var n = ReaderAtomized(entry.Xml.ElementName);
                var paramsReadOfI = paramsRead.ElementAccess(i.ToLiteralExpression());
                var value = entry.Symbol.Type switch
                {
                    { SpecialType: SpecialType.System_String } => Reader.ReadElementString(),
                    { SpecialType: SpecialType.System_Int32 } => XmlConvert.ToInt32(Reader.ReadElementString()),
                    { } when GetCore(entry) is { } core => ReadCoreName(core).Invoke(ns, True, True),
                    _ => Null.WithLeadingTrivia(Error("Unsupported child element type"))
                };
                return
                    IfStatement(
                        Not(paramsReadOfI).And(CorrectNames(n, ns)),
                        Tuple(paramsReadOfI, values[entry])
                        .Assign(
                            Tuple(True, value))
                        .AsStatement());
            }
            IfStatementSyntax ReadArrayElement(CoreChildBase entry)
            {
                var n = ReaderAtomized(entry.Xml.ElementName);
                var variable = values[entry];
                var core = GetCore(entry);
                var childName = ReaderAtomized(core.Xml.ElementName);
                var witness = ReadCoreWitnessTypeName(core);
                // oProp = oProp.AddRange(ReadNotnullListWithWitness<PropCore, PropCore_Witness>(n, ns));
                var assignment =
                    variable.Assign(
                        variable.Dot("AddRange").Invoke(
                            GenericName("ReadNotnullListWithWitness")
                            .AddTypeArgumentListArguments(core.CoreType, witness)
                            .Invoke(childName, ns)));
                return
                    IfStatement(
                        CorrectNames(n, ns),
                        Block(
                            IfStatement(Not(ReadNull()), assignment.AsStatement())));
            }
        }

        private ExpressionSyntax CreateReadEnumCall(ITypeSymbol enumSymbol, ExpressionSyntax value)
        {
            if (enumSymbol is not INamedTypeSymbol { TypeKind: TypeKind.Enum } symbol)
                throw new InvalidOperationException("Not an enum");
            UsedEnumSymbols.Add(symbol);
            return ReadEnumName(symbol).Invoke(value);
        }

        private MethodDeclarationSyntax CreateReadEnumMethod(INamedTypeSymbol symbol)
        {
            var s = IdentifierName("s");
            var type = IdentifierName(symbol.Name);
            return
                MethodDeclaration(type, ReadEnumName(symbol))
                .AddParameterListParameters(
                    Parameter(s.Identifier).WithType(String))
                .WithExpressionBodyFull(
                    SwitchExpression(s)
                    .AddArms(
                        symbol.GetMembers()
                        .Where(x => x.Kind == SymbolKind.Field)
                        .Select(ToArm)
                        .ToArray())
                    .AddArms(
                        SwitchExpressionArm(
                            DiscardPattern(),
                            ThrowExpression(
                                "CreateUnknownConstantException".Invoke(s, TypeOfExpression(type))))));
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
                        ConstantPattern(text.ToLiteralExpression()),
                        type.Dot(symbol.Name));
            }
        }

        private static StatementSyntax CheckReaderCount(IdentifierNameSyntax whileIterations, IdentifierNameSyntax readerCount)
        {
            return
                InvocationExpression(
                    IdentifierName("CheckReaderCount"),
                    ArgumentList(
                        SeparatedList(new[]
                        {
                            Argument(null, Token(SyntaxKind.RefKeyword), whileIterations),
                            Argument(null, Token(SyntaxKind.RefKeyword), readerCount)
                        })))
                .AsStatement();
        }

        private static TupleExpressionSyntax Tuple(params ExpressionSyntax[] args) =>
            TupleExpression(SeparatedList(args.Select(Argument)));

        private ArrayCreationExpressionSyntax NewBoolArray(CoreDescriptor core)
        {
            return
                ArrayCreationExpression(
                    ArrayType(
                        Bool,
                        SingletonList(
                            ArrayRankSpecifier(
                                SingletonSeparatedList<ExpressionSyntax>(
                                    core.Entries.Length.ToLiteralExpression())))));
        }

        private IfStatementSyntax IfGetXsiInvalidThenThrow(ExpressionSyntax n, IdentifierNameSyntax ns, IdentifierNameSyntax checkType, IdentifierNameSyntax xsiType)
        {
            return
                IfStatement(
                    checkType
                    .And(
                        // GetXsiType() is { } xsiType
                        "GetXsiType".Invoke()
                        .Is(
                            RecursivePattern(
                                type: null,
                                positionalPatternClause: null,
                                propertyPatternClause: PropertyPatternClause(),
                                designation: SingleVariableDesignation(xsiType.Identifier))))
                    .And(
                        // (xsiType.Name != (object)n || xsiType.Namespace != (object)ns)
                        xsiType.Dot("Name").OpNotEquals(n.Cast(Object))
                        .Or(
                            xsiType.Dot("Namespace").OpNotEquals(ns.Cast(Object)))
                        .WrapInParens()),
                    ThrowStatement(
                        "CreateUnknownTypeException".Invoke(xsiType)));
        }

        private StatementSyntax UnknownNode(ExpressionSyntax o, ExpressionSyntax? ns, params string[] names)
        {
            var qnames = (ns, names) switch
            {
                (_, names: { Length: 0 }) => EmptyString,
                (ns: { } nns, names: { Length: 1 }) => nns.OpAdd($":{names[0]}".ToLiteralExpression()),
                (ns: null, _) => string.Join(", ", names.Select(x => ":" + x)).ToLiteralExpression(),
                _ => InterpolatedStringExpression(
                        Token(SyntaxKind.InterpolatedStringStartToken),
                        List(CreateInterpolation(ns! /* not null because above switch arm catches null */)))
            };
            return "UnknownNode".Invoke(o, qnames).AsStatement();

            IEnumerable<InterpolatedStringContentSyntax> CreateInterpolation(ExpressionSyntax ns)
            {
                foreach (var text in names.TagFirstLast((x, first, last) => ":" + x + (last ? "" : ", ")))
                {
                    yield return Interpolation(ns);
                    yield return
                        InterpolatedStringText(
                            Token(
                                TriviaList(),
                                SyntaxKind.InterpolatedStringTextToken,
                                text,
                                text,
                                TriviaList()));
                }
            }
        }

        private ExpressionSyntax ReaderAtomized(string text, string? nameHint = null)
        {
            if (ReaderAtomizedStrings.TryGetValue(text, out var id))
            {
                return id;
            }
            var idName = "id_" + Regex.Replace(nameHint ?? text, "[^a-zA-Z0-9_]+", "") + $"_{ReaderAtomizedStrings.Count:G}";
            return ReaderAtomizedStrings[text] = IdentifierName(idName);
        }

        private IEnumerable<MemberDeclarationSyntax> CreateReaderAtomizationMembers()
        {
            var initSuppressedNull = EqualsValueClause(Null.SuppressNullableWarning());
            var pairs = ReaderAtomizedStrings.OrderBy(x => x.Value.Identifier.Text).ToList();
            foreach (var (_, id) in pairs)
            {
                // string id_name_123 = null!;
                // we suppress nullable warning because the InitIDs is called by framework (XmlSerializer)
                // before any other operations
                yield return
                    FieldDeclaration(
                        VariableDeclaration(
                            String,
                            SingletonSeparatedList(
                                VariableDeclarator(id.Identifier, null, initSuppressedNull))));
            }
            var readerNameTableAdd = ParseName("Reader.NameTable.Add");
            yield return
                MethodDeclaration(Void, "InitIDs")
                .AddModifiers(SyntaxKind.ProtectedKeyword, SyntaxKind.OverrideKeyword)
                .AddBodyStatements(
                    pairs.Select(x => AtomizationAssignment(x.Value, x.Key)));

            StatementSyntax AtomizationAssignment(IdentifierNameSyntax name, string text)
            {
                // id_name_123 = Reader.NameTable.Add("literal");
                var value = text.ToLiteralExpression();
                return name.Assign(readerNameTableAdd.Invoke(value)).AsStatement();
            }
        }

        private ExpressionSyntax CorrectNames(ExpressionSyntax n, ExpressionSyntax ns) =>
            Reader.LocalName.OpEquals(n.Cast(Object)).And(Reader.NamespaceURI.OpEquals(ns.Cast(Object)));
    }
}
