using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MoreLinq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class WhamSerializerGenerator
    {
        private const string XmlRootAttributeMetadataName = "System.Xml.Serialization.XmlRootAttribute";

        public WhamSerializerGenerator(Compilation compilation, ImmutableArray<CoreDescriptor> descriptors)
        {
            Compilation = compilation;
            XmlRootSymbol = compilation.GetTypeByMetadataNameOrThrow(XmlRootAttributeMetadataName);
            Descriptors = descriptors;
            SealedCores = descriptors.Where(x => x.TypeSymbol.IsSealed).ToImmutableArray();
            RootCores = SealedCores
                .Where(x => x.XmlAttributeLists.SelectMany(x => x.Attributes).Any(x => x.IsNamed("XmlRoot")))
                .ToImmutableArray();
        }

        public Compilation Compilation { get; }

        public ImmutableArray<CoreDescriptor> Descriptors { get; }

        public ImmutableArray<CoreDescriptor> SealedCores { get; }

        public ImmutableArray<CoreDescriptor> RootCores { get; }

        public INamedTypeSymbol XmlRootSymbol { get; }

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
                    .AddMembers(
                        generator.GenerateTypes()))
               .NormalizeWhitespace();
        }

        IEnumerable<ClassDeclarationSyntax> GenerateTypes()
        {
            yield return CreateXmlSerializationWriter();
            yield return CreateXmlSerializationReader();
            yield return CreateXmlSerializer();
        }

        private ClassDeclarationSyntax CreateXmlSerializer()
        {
            return ClassDeclaration("WhamCoreXmlSerializer")
                .AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.SealedKeyword)
                .AddBaseListTypes(
                    SimpleBaseType(
                        IdentifierName("XmlSerializer")))
                .AddMembers(
                    CreateReader(),
                    CreateWriter());

            MethodDeclarationSyntax CreateReader() =>
                MethodDeclaration(
                    IdentifierName("XmlSerializationReader"),
                    "CreateReader")
                .AddModifiers(SyntaxKind.ProtectedKeyword, SyntaxKind.OverrideKeyword)
                .AddBodyStatements(
                    ReturnStatement(
                        ObjectCreationExpression(
                            IdentifierName("WhamCoreXmlSerializationReader"))
                        .AddArgumentListArguments()));

            MethodDeclarationSyntax CreateWriter() =>
                MethodDeclaration(
                    IdentifierName("XmlSerializationWriter"),
                    "CreateWriter")
                .AddModifiers(SyntaxKind.ProtectedKeyword, SyntaxKind.OverrideKeyword)
                .AddBodyStatements(
                    ReturnStatement(
                        ObjectCreationExpression(
                            IdentifierName("WhamCoreXmlSerializationWriter"))
                        .AddArgumentListArguments()));
        }

        private ClassDeclarationSyntax CreateXmlSerializationReader()
        {
            return ClassDeclaration("WhamCoreXmlSerializationReader")
                .AddModifiers(SyntaxKind.PublicKeyword)
                .AddBaseListTypes(
                    SimpleBaseType(
                        IdentifierName("XmlSerializationReader")))
                .AddMembers(
                    MethodDeclaration(
                        PredefinedType(Token(SyntaxKind.VoidKeyword)),
                        "InitCallbacks")
                    .AddModifiers(SyntaxKind.ProtectedKeyword, SyntaxKind.OverrideKeyword)
                    .AddBodyStatements(),
                    MethodDeclaration(
                        PredefinedType(Token(SyntaxKind.VoidKeyword)),
                        "InitIDs")
                    .AddModifiers(SyntaxKind.ProtectedKeyword, SyntaxKind.OverrideKeyword)
                    .AddBodyStatements())
                .AddMembers(
                    SealedCores.Select(CreateReadCoreMethod));
        }

        private ClassDeclarationSyntax CreateXmlSerializationWriter()
        {
            return ClassDeclaration("WhamCoreXmlSerializationWriter")
                .AddModifiers(SyntaxKind.PublicKeyword)
                .AddBaseListTypes(
                    SimpleBaseType(
                        IdentifierName("XmlSerializationWriter")))
                .AddMembers(
                    MethodDeclaration(
                        PredefinedType(Token(SyntaxKind.VoidKeyword)),
                        "InitCallbacks")
                    .AddModifiers(SyntaxKind.ProtectedKeyword, SyntaxKind.OverrideKeyword)
                    .AddBodyStatements())
                .AddMembers(
                    SealedCores.Select(CreateWriteCoreMethod));
        }

        private MethodDeclarationSyntax CreateReadRootMethod(CoreDescriptor type)
        {
            var rootAttributes = type.TypeSymbol.GetAttributes();
            var xmlRoot = rootAttributes.Single(data => SymbolEqualityComparer.Default.Equals(XmlRootSymbol, data.AttributeClass));
            var xmlRootArgs = xmlRoot.NamedArguments.ToDictionary();
            var elementName = xmlRoot.ConstructorArguments.Length > 0
                ? xmlRoot.ConstructorArguments[0].ToCSharpString()
                : xmlRootArgs.TryGetValue(nameof(XmlRootAttribute.ElementName), out var elName)
                ? elName.ToCSharpString()
                : type.RawModelName;
            return
                MethodDeclaration(
                    type.CoreType.Nullable(),
                    Identifier("Read" + type.CoreTypeIdentifier))
                .AddParameterListParameters()
                .AddBodyStatements(
                    CreateBody());

            IEnumerable<StatementSyntax> CreateBody()
            {
                yield return
                    ReturnStatement(
                        LiteralExpression(SyntaxKind.NullLiteralExpression));
            }
        }

        private MethodDeclarationSyntax CreateReadCoreMethod(CoreDescriptor type)
        {
            return
                MethodDeclaration(
                    type.CoreType.Nullable(),
                    Identifier("Read" + type.CoreTypeIdentifier))
                .AddParameterListParameters(
                    Parameter(
                        Identifier("isNullable"))
                    .WithType(
                        PredefinedType(
                            Token(SyntaxKind.BoolKeyword))),
                    Parameter(
                        Identifier("checkType"))
                    .WithType(
                        PredefinedType(
                            Token(SyntaxKind.BoolKeyword))))
                .AddBodyStatements(
                    CreateBody());

            IEnumerable<StatementSyntax> CreateBody()
            {
                yield return 
                    ReturnStatement(
                        LiteralExpression(SyntaxKind.NullLiteralExpression));
            }
        }

        private MethodDeclarationSyntax CreateWriteCoreMethod(CoreDescriptor type)
        {
            return
                MethodDeclaration(
                    PredefinedType(
                        Token(SyntaxKind.VoidKeyword)),
                    Identifier("Write" + type.CoreTypeIdentifier))
                .AddParameterListParameters(
                    Parameter(
                        Identifier("n"))
                    .WithType(
                        PredefinedType(
                            Token(SyntaxKind.BoolKeyword))),
                    Parameter(
                        Identifier("ns"))
                    .WithType(
                        PredefinedType(
                            Token(SyntaxKind.BoolKeyword))),
                    Parameter(
                        Identifier("o"))
                    .WithType(
                        type.CoreType.Nullable()),
                    Parameter(
                        Identifier("isNullable"))
                    .WithType(
                        PredefinedType(
                            Token(SyntaxKind.BoolKeyword))),
                    Parameter(
                        Identifier("needType"))
                    .WithType(
                        PredefinedType(
                            Token(SyntaxKind.BoolKeyword))))
                .AddBodyStatements(
                    CreateBody());

            IEnumerable<StatementSyntax> CreateBody()
            {
                yield break;
            }
        }
    }
}
