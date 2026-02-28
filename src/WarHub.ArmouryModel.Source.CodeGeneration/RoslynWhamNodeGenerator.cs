using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MoreLinq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    [Generator]
    public class RoslynWhamNodeGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Collect all record declarations annotated with [WhamNodeCore]
            var recordSymbols = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    CoreDescriptorBuilder.WhamNodeCoreAttributeMetadataName,
                    predicate: static (node, _) => node is RecordDeclarationSyntax,
                    transform: static (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol)
                .Collect();

            var compilationAndSymbols = context.CompilationProvider.Combine(recordSymbols);

            context.RegisterSourceOutput(compilationAndSymbols, static (spc, source) =>
            {
                var (compilation, symbols) = source;
                Execute(spc, compilation, symbols);
            });
        }

        private static void Execute(
            SourceProductionContext context,
            Compilation compilation,
            ImmutableArray<INamedTypeSymbol> symbols)
        {
            if (symbols.IsDefaultOrEmpty)
                return;

            var descriptorBuilder = CoreDescriptorBuilder.Create(compilation);

            var descriptors = symbols
                .Where(x => x.GetAttributes().Any(a =>
                    SymbolEqualityComparer.Default.Equals(descriptorBuilder.WhamNodeCoreAttributeSymbol, a.AttributeClass)))
                .Select(x => descriptorBuilder.CreateDescriptor(x))
                .OrderBy(x => x.RawModelName)
                .ToImmutableArray();

            foreach (var descriptor in descriptors)
            {
                var compilationUnit = CreateSource(descriptor);
                context.AddSource(descriptor.RawModelName, compilationUnit.GetText(Encoding.UTF8));
            }

            var serializerRoot = WhamSerializerGenerator.Generate(compilation, descriptors);
            context.AddSource("WhamCoreXmlSerializer", serializerRoot.GetText(Encoding.UTF8));
        }

        private static CompilationUnitSyntax CreateSource(CoreDescriptor descriptor)
        {
            var declaredUsings = descriptor.TypeSymbol.DeclaringSyntaxReferences[0].GetSyntax().SyntaxTree.GetCompilationUnitRoot().Usings;
            var usings = RequiredUsings.AddRange(
                declaredUsings.Where(x => x.Name is { } name && !RequiredNamespaces.Contains(name.ToString())));
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

    }
}
