﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MoreLinq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    [Generator]
    public class RoslynWhamNodeGenerator : ISourceGenerator
    {
        private const string ImmutableArrayMetadataName = "System.Collections.Immutable.ImmutableArray`1";
        private const string WhamNodeCoreAttributeMetadataName = "WarHub.ArmouryModel.Source.WhamNodeCoreAttribute";

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            //Debugger.Launch();
            if (context.SyntaxReceiver is not SyntaxReceiver syntaxReceiver)
                return;

            var parseOptions = context.ParseOptions;
            var attributeSymbol = context.Compilation.GetTypeByMetadataName(WhamNodeCoreAttributeMetadataName)
                ?? throw new InvalidOperationException("Symbol not found: " + WhamNodeCoreAttributeMetadataName);
            var immutableArraySymbol = context.Compilation.GetTypeByMetadataName(ImmutableArrayMetadataName)
                ?? throw new InvalidOperationException("Symbol not found: " + ImmutableArrayMetadataName);
            var coreSymbols = GetNodeCoreSymbols().ToHashSet();

            foreach (var coreSymbol in coreSymbols)
            {
                var descriptor = CreateDescriptor(coreSymbol);
                var compilationUnit = CreateSource(descriptor);
                var sourceText = SyntaxTree(compilationUnit, parseOptions, encoding: Encoding.UTF8).GetText();
                context.AddSource(descriptor.RawModelName, sourceText);
            }

            IEnumerable<INamedTypeSymbol> GetNodeCoreSymbols()
            {
                foreach (var candidate in syntaxReceiver.Candidates)
                {
                    var model = context.Compilation.GetSemanticModel(candidate.SyntaxTree);
                    var classSymbol = model.GetDeclaredSymbol(candidate);
                    if (classSymbol?.GetAttributes().Any(data => data.AttributeClass?.Equals(attributeSymbol, SymbolEqualityComparer.Default) == true) == true)
                    {
                        yield return classSymbol;
                    }
                }
            }

            CoreDescriptor CreateDescriptor(INamedTypeSymbol coreSymbol)
            {
                var declarationSyntax = (RecordDeclarationSyntax)coreSymbol.DeclaringSyntaxReferences[0].GetSyntax();
                var attributes = declarationSyntax.AttributeLists
                    .SelectMany(x => x.Attributes)
                    .Where(x => x.IsNamed(Names.XmlRoot) || x.IsNamed(Names.XmlType))
                    .Select(x => AttributeList().AddAttributes(x))
                    .ToImmutableArray();

                var entries = GetCustomBaseTypesAndSelf(coreSymbol)
                    .Reverse()
                    .SelectMany(x => x.GetMembers().OfType<IPropertySymbol>())
                    .Where(p => p is { IsStatic: false, IsIndexer: false, DeclaredAccessibility: Accessibility.Public })
                    .Select(p =>
                    {
                        var x = p.DeclaringSyntaxReferences.FirstOrDefault();
                        var syntax = (PropertyDeclarationSyntax?)x?.GetSyntax();
                        var auto = syntax?.AccessorList?.Accessors.All(ac => ac is { Body: null, ExpressionBody: null, Keyword: { ValueText: "get" or "init" } }) ?? false;
                        return auto ? new { syntax = syntax!, symbol = p } : null;
                    })
                    .Where(x => x != null)
                    .Select(x => CoreDescriptorBuilder.CreateRecordEntry(x!.symbol, x.syntax, coreSymbol, immutableArraySymbol, attributeSymbol))
                    .ToImmutableArray();
                var descriptor = new CoreDescriptor(
                    coreSymbol,
                    IdentifierName(declarationSyntax.Identifier),
                    declarationSyntax.Identifier.WithoutTrivia(),
                    entries,
                    attributes);
                return descriptor;

                static IEnumerable<INamedTypeSymbol> GetCustomBaseTypesAndSelf(INamedTypeSymbol self)
                {
                    yield return self;
                    while (self.BaseType?.SpecialType == SpecialType.None)
                    {
                        self = self.BaseType;
                        yield return self;
                    }
                }
            }
        }

        private static CompilationUnitSyntax CreateSource(CoreDescriptor descriptor)
        {
            var declaredUsings = descriptor.TypeSymbol.DeclaringSyntaxReferences[0].GetSyntax().SyntaxTree.GetCompilationUnitRoot().Usings;
            var usings = RequiredUsings.AddRange(
                declaredUsings.Where(x => !RequiredNamespaces.Contains(x.Name.ToString())));
            var generatedMembers = List<MemberDeclarationSyntax>();
            generatedMembers = generatedMembers.AddRange(GenerateCorePartials(descriptor));
            generatedMembers = generatedMembers.AddRange(GenerateNodePartials(descriptor));
            return
               CompilationUnit()
               .AddUsings(usings.ToArray())
               .AddMembers(
                   NamespaceDeclaration(
                       ParseName(descriptor.TypeSymbol.ContainingNamespace.ToDisplayString()))
                   .WithMembers(generatedMembers))
               .WithLeadingTrivia(
                   TriviaList(PragmaWarningDisable))
               .NormalizeWhitespace();
        }

        private static SyntaxTrivia? pragmaWarningDisable;
        private static SyntaxTrivia PragmaWarningDisable =>
            pragmaWarningDisable ??= CreatePragmaWarningDisable();

        private static SyntaxTrivia CreatePragmaWarningDisable()
        {
            var codes = new[]
            {
                "CA1034", // Nested types should not be visible
                "CA1054", // URI parameters should not be strings
                "CA1056", // URI properties should not be strings
                "CA1062", // Validate arguments of public methods
                "CA1710", // Identifiers should have correct suffix
                "CA1716", // Identifiers should not match keywords
                "CA1815", // Override equals and operator equals on value types
                "CA2225", // Operator overloads have named alternates
                "CA2227", // Collection properties should be read only
            };
            return
                Trivia(
                    PragmaWarningDirectiveTrivia(
                        Token(SyntaxKind.DisableKeyword),
                        SeparatedList<ExpressionSyntax>(
                            codes.Select(x => IdentifierName(x))),
                        isActive: true));
        }

        private static ImmutableHashSet<string> RequiredNamespaces { get; } =
            ImmutableHashSet.Create(
                Names.NamespaceSystem,
                Names.NamespaceSystemCollections,
                Names.NamespaceSystemCollectionsGeneric,
                Names.NamespaceSystemCollectionsImmutable,
                Names.NamespaceSystemDiagnostics,
                Names.NamespaceSystemDiagnosticsCodeAnalysis,
                Names.NamespaceSystemXmlSerialization);

        private static ImmutableArray<UsingDirectiveSyntax>? requiredUsings;

        private static ImmutableArray<UsingDirectiveSyntax> RequiredUsings => requiredUsings ??= CreateRequiredUsings();

        private static ImmutableArray<UsingDirectiveSyntax> CreateRequiredUsings() =>
            ImmutableArray.Create(
                UsingDirective(ParseName(Names.NamespaceSystem)),
                UsingDirective(ParseName(Names.NamespaceSystemCollections)),
                UsingDirective(ParseName(Names.NamespaceSystemCollectionsGeneric)),
                UsingDirective(ParseName(Names.NamespaceSystemCollectionsImmutable)),
                UsingDirective(ParseName(Names.NamespaceSystemDiagnostics)),
                UsingDirective(ParseName(Names.NamespaceSystemDiagnosticsCodeAnalysis)),
                UsingDirective(ParseName(Names.NamespaceSystemXmlSerialization)));

        private static IEnumerable<TypeDeclarationSyntax> GenerateCorePartials(CoreDescriptor descriptor)
        {
            if (descriptor.TypeSymbol.IsAbstract)
            {
                yield break;
            }
            yield return CoreEmptyPropertyPartialGenerator.Generate(descriptor, default);
            yield return BuilderCorePartialGenerator.Generate(descriptor, default);
            yield return FspCorePartialGenerator.Generate(descriptor, default);
            yield return FseCorePartialGenerator.Generate(descriptor, default);
        }

        private static IEnumerable<TypeDeclarationSyntax> GenerateNodePartials(CoreDescriptor descriptor)
        {
            yield return CoreToNodeMethodsCorePartialGenerator.Generate(descriptor, default);
            yield return BasicDeclarationNodeGenerator.Generate(descriptor, default);
            yield return NodeGenerator.Generate(descriptor, default);
            yield return NodeExtensionsGenerator.Generate(descriptor, default);
            yield return CollectionConversionExtensionsPartialGenerator.Generate(descriptor, default);
            yield return NodeConvenienceMethodsGenerator.Generate(descriptor, default);
            yield return NodeAcceptSourceVisitorPartialGenerator.Generate(descriptor, default);
            if (descriptor.TypeSymbol.IsAbstract)
            {
                yield break;
            }
            yield return ListNodePartialGenerator.Generate(descriptor, default);
            yield return SourceVisitorVisitPartialGenerator.Generate(descriptor, default);
            yield return SourceVisitorGenericVisitPartialGenerator.Generate(descriptor, default);
            yield return SourceRewriterVisitPartialGenerator.Generate(descriptor, default);
            yield return NodeFactoryPartialGenerator.Generate(descriptor, default);
        }

        private class SyntaxReceiver : ISyntaxReceiver
        {
            public List<RecordDeclarationSyntax> Candidates { get; } = new List<RecordDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode.IsKind(SyntaxKind.RecordDeclaration)
                    && syntaxNode is RecordDeclarationSyntax record
                    && record.AttributeLists.Count > 0)
                {
                    Candidates.Add(record);
                }
            }
        }
    }
}
