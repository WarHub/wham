using FluentAssertions;
using Microsoft.CodeAnalysis;
using WarHub.ArmouryModel.Concrete.Generators;
using Xunit;

namespace WarHub.ArmouryModel.Concrete.Extensions.Generators.Tests;

public class GenerateSymbolGeneratorTests
{
    [Fact]
    public void Generates_Kind_and_Accept_for_Symbol_subclass()
    {
        var source = """
            namespace WarHub.ArmouryModel.Concrete
            {
                [GenerateSymbol(WarHub.ArmouryModel.SymbolKind.Catalogue)]
                internal partial class TestCatalogueSymbol : Symbol, WarHub.ArmouryModel.ICatalogueSymbol
                {
                }
            }
            """;

        var driver = TestHelper.RunGenerator<GenerateSymbolGenerator>(source);
        var results = driver.GetRunResult();

        results.Diagnostics.Should().BeEmpty();
        results.GeneratedTrees.Should().HaveCount(2, "attribute source + generated members");

        var generatedSource = results.GeneratedTrees
            .Select(t => t.GetText().ToString())
            .First(t => t.Contains("TestCatalogueSymbol", StringComparison.Ordinal));

        generatedSource.Should().Contain("SymbolKind Kind => global::WarHub.ArmouryModel.SymbolKind.Catalogue;");
        generatedSource.Should().Contain("sealed override void Accept(global::WarHub.ArmouryModel.SymbolVisitor visitor)");
        generatedSource.Should().Contain("visitor.VisitCatalogue(this)");
        generatedSource.Should().Contain("sealed override TResult Accept<TResult>(global::WarHub.ArmouryModel.SymbolVisitor<TResult> visitor)");
        generatedSource.Should().Contain("sealed override TResult Accept<TArgument, TResult>(global::WarHub.ArmouryModel.SymbolVisitor<TArgument, TResult> visitor");
    }

    [Fact]
    public void Generates_only_Accept_for_non_Symbol_class()
    {
        var source = """
            namespace WarHub.ArmouryModel.Concrete
            {
                [GenerateSymbol(WarHub.ArmouryModel.SymbolKind.ResourceEntry)]
                internal sealed partial class EffectiveTestSymbol : WarHub.ArmouryModel.IResourceEntrySymbol
                {
                    public WarHub.ArmouryModel.SymbolKind Kind => WarHub.ArmouryModel.SymbolKind.ResourceEntry;
                }
            }
            """;

        var driver = TestHelper.RunGenerator<GenerateSymbolGenerator>(source);
        var results = driver.GetRunResult();

        results.Diagnostics.Should().BeEmpty();

        var generatedSource = results.GeneratedTrees
            .Select(t => t.GetText().ToString())
            .First(t => t.Contains("EffectiveTestSymbol", StringComparison.Ordinal));

        // Should NOT contain Kind property (non-Symbol class manages its own)
        generatedSource.Should().NotContain("SymbolKind Kind =>");

        // Should contain public (not override) Accept methods
        generatedSource.Should().Contain("public void Accept(global::WarHub.ArmouryModel.SymbolVisitor visitor)");
        generatedSource.Should().Contain("visitor.VisitResourceEntry(this)");
        generatedSource.Should().NotContain("override");
    }

    [Fact]
    public void Emits_GenerateSymbolAttribute_source()
    {
        var source = """
            namespace WarHub.ArmouryModel.Concrete
            {
                // No usage, just check attribute is emitted
            }
            """;

        var driver = TestHelper.RunGenerator<GenerateSymbolGenerator>(source);
        var results = driver.GetRunResult();

        var attrSource = results.GeneratedTrees
            .Select(t => t.GetText().ToString())
            .First(t => t.Contains("GenerateSymbolAttribute", StringComparison.Ordinal));

        attrSource.Should().Contain("class GenerateSymbolAttribute");
        attrSource.Should().Contain("SymbolKind Kind");
    }

    [Fact]
    public void Generates_sealed_override_for_abstract_base_class()
    {
        var source = """
            namespace WarHub.ArmouryModel.Concrete
            {
                [GenerateSymbol(WarHub.ArmouryModel.SymbolKind.Constraint)]
                internal abstract partial class TestConstraintBase : SourceDeclaredSymbol, WarHub.ArmouryModel.IConstraintSymbol
                {
                }
            }
            """;

        var driver = TestHelper.RunGenerator<GenerateSymbolGenerator>(source);
        var results = driver.GetRunResult();

        var generatedSource = results.GeneratedTrees
            .Select(t => t.GetText().ToString())
            .First(t => t.Contains("TestConstraintBase", StringComparison.Ordinal));

        // Abstract base still gets sealed override (subtype must not change dispatch)
        generatedSource.Should().Contain("SymbolKind Kind => global::WarHub.ArmouryModel.SymbolKind.Constraint;");
        generatedSource.Should().Contain("sealed override void Accept(global::WarHub.ArmouryModel.SymbolVisitor visitor)");
    }

    [Fact]
    public void Generated_code_compiles_without_errors()
    {
        var source = """
            namespace WarHub.ArmouryModel.Concrete
            {
                [GenerateSymbol(WarHub.ArmouryModel.SymbolKind.Container)]
                internal partial class TestContainerSymbol : Symbol, WarHub.ArmouryModel.IContainerSymbol
                {
                }
            }
            """;

        var (output, errors) = TestHelper.RunGeneratorAndGetOutput<GenerateSymbolGenerator>(source);

        errors.Should().BeEmpty("generated code should compile without errors");
    }
}
