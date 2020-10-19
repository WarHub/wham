using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MoreLinq.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal partial class WhamSerializerGenerator
    {
        private static string WriteRootName(CoreDescriptor core) => "WriteRoot_" + core.Xml.ElementName;

        private static string WriteCoreName(CoreDescriptor core) => "Write_" + core.CoreTypeIdentifier;

        private static string WriteEnumName(INamedTypeSymbol symbol) => "WriteEnum_" + symbol.Name;

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
                            o.Dot(nameof(object.GetType)).Invoke()
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
                            Not(a.Dot(nameof(ImmutableArray<int>.IsDefaultOrEmpty))),
                            Block(
                                IfNotEmptyStatements())));
                IEnumerable<StatementSyntax> IfNotEmptyStatements()
                {
                    yield return "WriteStartElement".Invoke(n, ns, Null, False).AsStatement();
                    yield return
                        ForStatement(
                            declaration: i.Identifier.InitVar(Zero),
                            initializers: default,
                            condition: i.OpLessThan(a.Dot("Length")),
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
                .Dot("ToString")
                .Invoke(InvariantCulture);
            var typeFullName = TypeOfExpression(type).Dot(nameof(Type.FullName));
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
                        ConstantPattern(type.Dot(symbol.Name)),
                        LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(text)));
            }
        }
    }
}
