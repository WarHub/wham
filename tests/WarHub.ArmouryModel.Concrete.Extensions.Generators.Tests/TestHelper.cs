using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using WarHub.ArmouryModel.Concrete.Generators;

namespace WarHub.ArmouryModel.Concrete.Extensions.Generators.Tests;

internal static class TestHelper
{
    /// <summary>
    /// Minimal stubs for SymbolKind, SymbolVisitor, ISymbol, and Symbol so that
    /// generators can resolve types without the full project dependency.
    /// </summary>
    public const string Stubs = """
        namespace WarHub.ArmouryModel
        {
            public enum SymbolKind
            {
                Error, GamesystemNamespace, Catalogue, Roster,
                ResourceDefinition, ResourceEntry, Resource,
                ContainerEntry, Container,
                Constraint, Effect, Condition, Query,
                CatalogueReference, PublicationReference, EntryReferencePath,
                RosterCost,
            }

            public interface ISymbol
            {
                SymbolKind Kind { get; }
                void Accept(SymbolVisitor visitor);
                TResult Accept<TResult>(SymbolVisitor<TResult> visitor);
                TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument argument);
            }

            public abstract class SymbolVisitor
            {
                public virtual void DefaultVisit(ISymbol symbol) { }
                public virtual void VisitCatalogue(ICatalogueSymbol s) => DefaultVisit(s);
                public virtual void VisitResourceEntry(IResourceEntrySymbol s) => DefaultVisit(s);
                public virtual void VisitConstraint(IConstraintSymbol s) => DefaultVisit(s);
                public virtual void VisitContainer(IContainerSymbol s) => DefaultVisit(s);
                public virtual void VisitQuery(IQuerySymbol s) => DefaultVisit(s);
            }

            public abstract class SymbolVisitor<TResult>
            {
                protected abstract TResult DefaultResult { get; }
                public virtual TResult DefaultVisit(ISymbol symbol) => DefaultResult;
                public virtual TResult VisitCatalogue(ICatalogueSymbol s) => DefaultVisit(s);
                public virtual TResult VisitResourceEntry(IResourceEntrySymbol s) => DefaultVisit(s);
                public virtual TResult VisitConstraint(IConstraintSymbol s) => DefaultVisit(s);
                public virtual TResult VisitContainer(IContainerSymbol s) => DefaultVisit(s);
                public virtual TResult VisitQuery(IQuerySymbol s) => DefaultVisit(s);
            }

            public abstract class SymbolVisitor<TArgument, TResult>
            {
                protected abstract TResult DefaultResult { get; }
                public virtual TResult DefaultVisit(ISymbol symbol, TArgument argument) => DefaultResult;
                public virtual TResult VisitCatalogue(ICatalogueSymbol s, TArgument a) => DefaultVisit(s, a);
                public virtual TResult VisitResourceEntry(IResourceEntrySymbol s, TArgument a) => DefaultVisit(s, a);
                public virtual TResult VisitConstraint(IConstraintSymbol s, TArgument a) => DefaultVisit(s, a);
                public virtual TResult VisitContainer(IContainerSymbol s, TArgument a) => DefaultVisit(s, a);
                public virtual TResult VisitQuery(IQuerySymbol s, TArgument a) => DefaultVisit(s, a);
            }

            public interface ICatalogueSymbol : ISymbol { }
            public interface IResourceEntrySymbol : ISymbol { }
            public interface IConstraintSymbol : ISymbol { }
            public interface IContainerSymbol : ISymbol { }
            public interface IQuerySymbol : ISymbol { }
        }

        namespace WarHub.ArmouryModel.Concrete
        {
            internal abstract class Symbol : WarHub.ArmouryModel.ISymbol
            {
                public abstract WarHub.ArmouryModel.SymbolKind Kind { get; }
                public abstract void Accept(WarHub.ArmouryModel.SymbolVisitor visitor);
                public abstract TResult Accept<TResult>(WarHub.ArmouryModel.SymbolVisitor<TResult> visitor);
                public abstract TResult Accept<TArgument, TResult>(WarHub.ArmouryModel.SymbolVisitor<TArgument, TResult> visitor, TArgument argument);
            }

            internal abstract class SourceDeclaredSymbol : Symbol
            {
                protected T GetBoundField<T, TState>(ref T? field, TState state, System.Func<object, object, TState, T> bind) where T : class
                    => field!;
                protected System.Collections.Immutable.ImmutableArray<T> GetBoundField<T, TState>(ref System.Collections.Immutable.ImmutableArray<T> field, TState state, System.Func<object, object, TState, System.Collections.Immutable.ImmutableArray<T>> bind)
                    => field;
                protected virtual void CheckReferencesCore() { }
            }
        }
        """;

    public static GeneratorDriver RunGenerator<T>(string source) where T : IIncrementalGenerator, new()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(Stubs + "\n" + source);
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ImmutableArray<>).Assembly.Location),
        };

        // Add System.Runtime reference
        var runtimeDir = System.IO.Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var runtimeRef = MetadataReference.CreateFromFile(System.IO.Path.Combine(runtimeDir, "System.Runtime.dll"));

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            [..references, runtimeRef],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new T();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
        return driver;
    }

    public static (Compilation OutputCompilation, ImmutableArray<Diagnostic> Diagnostics) RunGeneratorAndGetOutput<T>(string source) where T : IIncrementalGenerator, new()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(Stubs + "\n" + source);
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ImmutableArray<>).Assembly.Location),
        };

        var runtimeDir = System.IO.Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var runtimeRef = MetadataReference.CreateFromFile(System.IO.Path.Combine(runtimeDir, "System.Runtime.dll"));

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            [..references, runtimeRef],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new T();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var outputDiagnostics);

        var errors = outputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToImmutableArray();

        return (outputCompilation, errors);
    }

    public static ImmutableArray<Diagnostic> RunAnalyzer(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(Stubs + "\n" + source);
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ImmutableArray<>).Assembly.Location),
        };

        var runtimeDir = System.IO.Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var runtimeRef = MetadataReference.CreateFromFile(System.IO.Path.Combine(runtimeDir, "System.Runtime.dll"));

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            [..references, runtimeRef],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Run the BoundGenerator first to inject the [Bound] attribute source
        var boundGen = new BoundGenerator();
        var genSymGen = new GenerateSymbolGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(boundGen, genSymGen);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var compilationWithAttrs, out _);

        var analyzerDiags = compilationWithAttrs
            .WithAnalyzers([new BoundAnalyzer()])
            .GetAnalyzerDiagnosticsAsync().Result;

        return analyzerDiags;
    }
}
