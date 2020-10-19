﻿using System;
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
    internal partial class WhamSerializerGenerator
    {
        private const string WhamCoreXmlSerializationReaderName = "WhamCoreXmlSerializationReader";
        private const string WhamCoreXmlSerializationWriterName = "WhamCoreXmlSerializationWriter";
        private const string XmlEnumAttributeMetadataName = "System.Xml.Serialization.XmlEnumAttribute";

        public WhamSerializerGenerator(Compilation compilation, ImmutableArray<CoreDescriptor> descriptors)
        {
            Compilation = compilation;
            Descriptors = descriptors;
            SealedCores = descriptors.Where(x => x.TypeSymbol.IsSealed).ToImmutableArray();
            RootCores = SealedCores.Where(x => x.Xml.IsRoot).ToImmutableArray();
            XmlEnumSymbol = Compilation.GetTypeByMetadataNameOrThrow(XmlEnumAttributeMetadataName);
        }

        public Compilation Compilation { get; }
        public ImmutableArray<CoreDescriptor> Descriptors { get; }
        public ImmutableArray<CoreDescriptor> SealedCores { get; }
        public ImmutableArray<CoreDescriptor> RootCores { get; }
        private INamedTypeSymbol XmlEnumSymbol { get; }

        private static ImmutableArray<string> DisabledErrorCodes { get; } =
            new[]
            {
                "CA1062", // Validate arguments of public methods
                "CA1707", // Identifiers should not contain underscores
                "CA1801", // Review unused parameters
                "CA1822", // Mark members as static
            }.ToImmutableArray();

        private HashSet<INamedTypeSymbol> UsedEnumSymbols { get; } = new HashSet<INamedTypeSymbol>();

        private NameSyntax InvariantCulture { get; } =
            ParseName("System.Globalization.CultureInfo.InvariantCulture");

        private NameSyntax XmlConvertToStringName { get; } =
            ParseName("System.Xml.XmlConvert.ToString");

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

        private static SyntaxTrivia Error(string message) =>
            Trivia(
                ErrorDirectiveTrivia(true)
                .WithEndOfDirectiveToken(
                    Token(
                        TriviaList(
                            PreprocessingMessage(message)),
                        SyntaxKind.EndOfDirectiveToken,
                        TriviaList())));

        private ExpressionSyntax XmlConvertToString(ExpressionSyntax arg) =>
            XmlConvertToStringName.Invoke(arg);

        public static CompilationUnitSyntax Generate(Compilation compilation, ImmutableArray<CoreDescriptor> descriptors)
        {
            var generator = new WhamSerializerGenerator(compilation, descriptors);
            return generator.GenerateCompilationUnit();
        }

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

        public CompilationUnitSyntax GenerateCompilationUnit()
        {
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
                        GenerateTypes()))
               .NormalizeWhitespace();
        }

        private IEnumerable<ClassDeclarationSyntax> GenerateTypes()
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
                        .Dot(nameof(XmlReader.IsStartElement))
                        .Invoke(
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
                        .WrapInParens()
                        .Dot(
                            ReadRootName(root))
                        .Invoke()));

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
                    .WrapInParens()
                    .Dot(
                        WriteRootName(root))
                    .Invoke(
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
    }
}
