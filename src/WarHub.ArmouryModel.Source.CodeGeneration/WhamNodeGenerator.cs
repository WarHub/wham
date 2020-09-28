using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    public class WhamNodeGenerator : IRichCodeGenerator
    {
        private static readonly DiagnosticDescriptor exceptionDiagDescriptor
            = new DiagnosticDescriptor(
                id: "WHAMGEN",
                title: "Error when generating code.",
                messageFormat: "Exception details: {0}",
                category: "CodeGeneration",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

#pragma warning disable CA1801 // Unused parameter.
        public WhamNodeGenerator(AttributeData attributeData)
#pragma warning restore CA1801 // Unused parameter.
        {
        }

        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<RichGenerationResult> GenerateRichAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            return Task.FromResult(new RichGenerationResult
            {
                Usings = List(new[]
                {
                    UsingDirective(ParseName(Names.NamespaceSystem)),
                    UsingDirective(ParseName(Names.NamespaceSystemCollectionsGeneric)),
                    UsingDirective(ParseName(Names.NamespaceSystemCollectionsImmutable)),
                    UsingDirective(ParseName(Names.NamespaceSystemDiagnostics)),
                    UsingDirective(ParseName(Names.NamespaceSystemDiagnosticsCodeAnalysis)),
                    UsingDirective(ParseName(Names.NamespaceSystemXmlSerialization))
                }.Where(x => !context.CompilationUnitUsings.Any(other => x.Name.ToString() == other.Name.ToString()))),
                Members =
                    SingletonList<MemberDeclarationSyntax>(
                        context.ProcessingNode.FirstAncestorOrSelf<NamespaceDeclarationSyntax>()
                        .WithMembers(GenerateMembers()))
            });

            SyntaxList<MemberDeclarationSyntax> GenerateMembers()
            {
                var generatedMembers = SyntaxFactory.List<MemberDeclarationSyntax>();
                try
                {
                    if (context.ProcessingNode is ClassDeclarationSyntax classDeclaration)
                    {
                        var descriptorBuilder = new CoreDescriptorBuilder(context, cancellationToken);
                        var descriptor = descriptorBuilder.CreateDescriptor();
                        generatedMembers = generatedMembers.AddRange(GenerateCorePartials(descriptor));
                        generatedMembers = generatedMembers.AddRange(GenerateNodePartials(descriptor));
                    }
                }
                catch (Exception e)
                {
                    if (progress is null)
                    {
                        Console.Error.WriteLine("WHAMGEN: error:" + e.ToString());
                        throw;
                    }
                    progress.Report(
                        Diagnostic.Create(
                            exceptionDiagDescriptor,
                            context.ProcessingNode.GetLocation(),
                            messageArgs: e.ToString()));
                }
                return generatedMembers;
            }
            IEnumerable<TypeDeclarationSyntax> GenerateCorePartials(CoreDescriptor descriptor)
            {
                yield return RecordCorePartialGenerator.Generate(descriptor, cancellationToken);
                if (descriptor.TypeSymbol.IsAbstract)
                {
                    yield break;
                }
                yield return BuilderCorePartialGenerator.Generate(descriptor, cancellationToken);
                yield return FspCorePartialGenerator.Generate(descriptor, cancellationToken);
                yield return FseCorePartialGenerator.Generate(descriptor, cancellationToken);
            }
            IEnumerable<TypeDeclarationSyntax> GenerateNodePartials(CoreDescriptor descriptor)
            {
                yield return CoreToNodeMethodsCorePartialGenerator.Generate(descriptor, cancellationToken);
                yield return BasicDeclarationNodeGenerator.Generate(descriptor, cancellationToken);
                yield return NodeGenerator.Generate(descriptor, cancellationToken);
                yield return NodeExtensionsGenerator.Generate(descriptor, cancellationToken);
                yield return CollectionConversionExtensionsPartialGenerator.Generate(descriptor, cancellationToken);
                yield return NodeConvenienceMethodsGenerator.Generate(descriptor, cancellationToken);
                yield return NodeAcceptSourceVisitorPartialGenerator.Generate(descriptor, cancellationToken);
                if (descriptor.TypeSymbol.IsAbstract)
                {
                    yield break;
                }
                yield return ListNodePartialGenerator.Generate(descriptor, cancellationToken);
                yield return SourceVisitorVisitPartialGenerator.Generate(descriptor, cancellationToken);
                yield return SourceVisitorGenericVisitPartialGenerator.Generate(descriptor, cancellationToken);
                yield return SourceRewriterVisitPartialGenerator.Generate(descriptor, cancellationToken);
                yield return NodeFactoryPartialGenerator.Generate(descriptor, cancellationToken);
            }
        }
    }
}
