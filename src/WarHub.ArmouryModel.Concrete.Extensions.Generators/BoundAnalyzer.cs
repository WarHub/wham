using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

    public static readonly DiagnosticDescriptor GetBoundFieldNonStaticLambda = new(
        id: "WHAM002",
        title: "GetBoundField called with non-static lambda",
        messageFormat: "GetBoundField lambda should be static to avoid delegate allocation on every access. Add the 'static' modifier.",
        category: "WarHub.ArmouryModel",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(GetBoundFieldWithoutBoundAttribute, GetBoundFieldNonStaticLambda);

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

        CheckBoundAttribute(context, invocation);
        CheckStaticLambda(context, invocation);
    }

    private static void CheckBoundAttribute(OperationAnalysisContext context, IInvocationOperation invocation)
    {
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

    private static void CheckStaticLambda(OperationAnalysisContext context, IInvocationOperation invocation)
    {
        // The last argument to GetBoundField is the binding lambda
        var args = invocation.Arguments;
        if (args.Length < 3)
            return;

        var lambdaArg = args[args.Length - 1];
        if (lambdaArg.Value is not IDelegateCreationOperation delegateCreation)
            return;

        if (delegateCreation.Target is not IAnonymousFunctionOperation anonymousFunc)
            return;

        // Check the syntax node for the 'static' modifier
        var syntax = anonymousFunc.Syntax;
        bool isStatic = false;
        if (syntax is LambdaExpressionSyntax lambda)
        {
            isStatic = lambda.Modifiers.Any(SyntaxKind.StaticKeyword);
        }
        else if (syntax is AnonymousMethodExpressionSyntax anonymousMethod)
        {
            isStatic = anonymousMethod.Modifiers.Any(SyntaxKind.StaticKeyword);
        }

        if (!isStatic)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(GetBoundFieldNonStaticLambda, syntax.GetLocation()));
        }
    }
}
