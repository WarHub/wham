using System;
using System.Threading;
using System.Threading.Tasks;
using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    public class WhamNodeGenerator : ICodeGenerator
    {
        private static readonly DiagnosticDescriptor ExceptionDiagDescriptor
            = new DiagnosticDescriptor(
                id: "WHAMGEN",
                title: "Error when generating code.",
                messageFormat: "Exception details: {0}",
                category: "CodeGeneration",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        public WhamNodeGenerator(AttributeData attributeData)
        {
        }

        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
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
                        ExceptionDiagDescriptor,
                        context.ProcessingNode.GetLocation(),
                        messageArgs: e.ToString()));
            }
            return Task.FromResult(generatedMembers);
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
                yield return NodeCollectionConvenienceMethodsGenerator.Generate(descriptor, cancellationToken);
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
                yield return NodeFactoryPartialGenerator.Generate(descriptor, cancellationToken);
            }
        }
    }
}
