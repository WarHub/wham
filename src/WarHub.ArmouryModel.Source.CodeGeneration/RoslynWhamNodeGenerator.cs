using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    [Generator]
    public class RoslynWhamNodeGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver syntaxReceiver)
                return;

            var coreSymbols = GetNodeCoreSymbols().ToHashSet();

            foreach (var coreSymbol in coreSymbols)
            {
                var descriptor = CreateDescriptor(coreSymbol);
                var text = CreateSourceText(descriptor);
                context.AddSource(descriptor.RawModelName, text);
            }

            IEnumerable<INamedTypeSymbol> GetNodeCoreSymbols()
            {
                var attributeSymbol = context.Compilation.GetTypeByMetadataName("WarHub.ArmouryModel.Source.WhamNodeCoreAttribute");
                foreach (var candidate in syntaxReceiver.CandidateClasses)
                {
                    var model = context.Compilation.GetSemanticModel(candidate.SyntaxTree);
                    var classSymbol = model.GetDeclaredSymbol(candidate);
                    if (classSymbol.GetAttributes().Any(data => data.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default)))
                    {
                        yield return classSymbol;
                    }
                }
            }
        }

        private static CoreDescriptor CreateDescriptor(INamedTypeSymbol coreSymbol)
        {
            var declarationSyntax = (ClassDeclarationSyntax)coreSymbol.DeclaringSyntaxReferences[0].GetSyntax();
            var attributes = declarationSyntax.AttributeLists
                .SelectMany(x => x.Attributes)
                .Where(x => x.IsNamed(Names.XmlRoot) || x.IsNamed(Names.XmlType))
                .Select(x => AttributeList().AddAttributes(x))
                .ToImmutableArray();

            var entries = GetCustomBaseTypesAndSelf(coreSymbol)
                .Reverse()
                .SelectMany(x => x.GetMembers().OfType<IPropertySymbol>())
                .Where(p => p is { IsReadOnly: true, IsStatic: false, IsIndexer: false, DeclaredAccessibility: Accessibility.Public })
                .Select(p =>
                {
                    var x = p.DeclaringSyntaxReferences.FirstOrDefault();
                    var syntax = (PropertyDeclarationSyntax?)x?.GetSyntax();
                    var auto = syntax?.AccessorList?.Accessors.All(accessor => accessor.Body == null && accessor.ExpressionBody == null) ?? false;
                    return auto ? new { syntax = syntax!, symbol = p } : null;
                })
                .Where(x => x != null)
                .Select(x => CoreDescriptorBuilder.CreateRecordEntry(x!.symbol, x.syntax))
                .ToImmutableArray();
            var descriptor = new CoreDescriptor(
                coreSymbol,
                ParseName(coreSymbol.ToDisplayString()),
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

        private static SourceText CreateSourceText(CoreDescriptor descriptor)
        {
            var generatedMembers = List<MemberDeclarationSyntax>();
            generatedMembers = generatedMembers.AddRange(GenerateCorePartials(descriptor));
            generatedMembers = generatedMembers.AddRange(GenerateNodePartials(descriptor));
            return
               CompilationUnit()
               .AddUsings(
                   UsingDirective(ParseName(Names.NamespaceSystem)),
                   UsingDirective(ParseName(Names.NamespaceSystemCollectionsGeneric)),
                   UsingDirective(ParseName(Names.NamespaceSystemCollectionsImmutable)),
                   UsingDirective(ParseName(Names.NamespaceSystemDiagnostics)),
                   UsingDirective(ParseName(Names.NamespaceSystemDiagnosticsCodeAnalysis)),
                   UsingDirective(ParseName(Names.NamespaceSystemXmlSerialization)))
               .AddMembers(
                   NamespaceDeclaration(
                       ParseName(descriptor.TypeSymbol.ContainingNamespace.ToDisplayString()))
                   .WithMembers(generatedMembers))
               .SyntaxTree
               .GetText();
        }

        private static IEnumerable<TypeDeclarationSyntax> GenerateCorePartials(CoreDescriptor descriptor)
        {
            yield return RecordCorePartialGenerator.Generate(descriptor, default);
            if (descriptor.TypeSymbol.IsAbstract)
            {
                yield break;
            }
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
            public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();
            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode.IsKind(SyntaxKind.ClassDeclaration)
                    && syntaxNode is ClassDeclarationSyntax classDeclaration
                    && classDeclaration.AttributeLists.Count > 0)
                {
                    CandidateClasses.Add(classDeclaration);
                }
            }
        }
    }
}
