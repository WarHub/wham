using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace WarHub.ArmouryModel.Concrete.Generators;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BoundAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor GetBoundFieldWithoutBoundAttribute = new(
        id: "WHAM001",
        title: "Property calls GetBoundField without [Bound] attribute",
        messageFormat: "Property '{0}' calls GetBoundField but is not annotated with [Bound]. Add [Bound] to ensure it is included in CheckReferencesCore.",
        category: "WarHub.ArmouryModel",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(GetBoundFieldWithoutBoundAttribute);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        if (invocation.TargetMethod.Name != "GetBoundField")
            return;

        // Walk up to find the containing property
        var containingSymbol = context.ContainingSymbol;
        if (containingSymbol is not IMethodSymbol { AssociatedSymbol: IPropertySymbol property })
            return;

        // Check if the property has [Bound] attribute
        var hasBoundAttribute = false;
        foreach (var attr in property.GetAttributes())
        {
            if (attr.AttributeClass?.Name == "BoundAttribute"
                && attr.AttributeClass.ContainingNamespace.ToDisplayString() == "WarHub.ArmouryModel.Concrete")
            {
                hasBoundAttribute = true;
                break;
            }
        }

        if (!hasBoundAttribute)
        {
            var diagnostic = Diagnostic.Create(
                GetBoundFieldWithoutBoundAttribute,
                invocation.Syntax.GetLocation(),
                property.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
