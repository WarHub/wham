using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class WhamSerializerGenerator
    {
        private const string WhamCoreXmlSerializationReaderName = "WhamCoreXmlSerializationReader";
        private const string WhamCoreXmlSerializationWriterName = "WhamCoreXmlSerializationWriter";

        public WhamSerializerGenerator(Compilation compilation, ImmutableArray<CoreDescriptor> descriptors)
        {
            Compilation = compilation;
            Descriptors = descriptors;
            SealedCores = descriptors.Where(x => x.TypeSymbol.IsSealed).ToImmutableArray();
            RootCores = SealedCores
                .Where(x => x.Xml.IsRoot)
                .ToImmutableArray();
        }

        public Compilation Compilation { get; }

        public ImmutableArray<CoreDescriptor> Descriptors { get; }

        public ImmutableArray<CoreDescriptor> SealedCores { get; }

        public ImmutableArray<CoreDescriptor> RootCores { get; }

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
            foreach (var root in RootCores)
            {
                yield return CreateXmlSerializer(root);
            }
        }

        private ClassDeclarationSyntax CreateXmlSerializer(CoreDescriptor root)
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
                            IdentifierName(WhamCoreXmlSerializationReaderName))
                        .AddArgumentListArguments()));

            MethodDeclarationSyntax CreateWriter() =>
                MethodDeclaration(
                    IdentifierName("XmlSerializationWriter"),
                    "CreateWriter")
                .AddModifiers(SyntaxKind.ProtectedKeyword, SyntaxKind.OverrideKeyword)
                .AddBodyStatements(
                    ReturnStatement(
                        ObjectCreationExpression(
                            IdentifierName(WhamCoreXmlSerializationWriterName))
                        .AddArgumentListArguments()));
        }

        private ClassDeclarationSyntax CreateXmlSerializationReader()
        {
            return ClassDeclaration(WhamCoreXmlSerializationReaderName)
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
            return
                MethodDeclaration(
                    type.CoreType.Nullable(),
                    Identifier("Read_" + type.Xml.ElementName))
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
                    Identifier("Read_" + type.CoreTypeIdentifier))
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
