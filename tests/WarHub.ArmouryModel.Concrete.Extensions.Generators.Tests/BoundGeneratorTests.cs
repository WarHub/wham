using FluentAssertions;
using Microsoft.CodeAnalysis;
using WarHub.ArmouryModel.Concrete.Generators;
using Xunit;

namespace WarHub.ArmouryModel.Concrete.Extensions.Generators.Tests;

public class BoundGeneratorTests
{
    [Fact]
    public void Generates_CheckReferencesCore_for_single_Bound_property()
    {
        var source = """
            namespace WarHub.ArmouryModel.Concrete
            {
                [GenerateSymbol(WarHub.ArmouryModel.SymbolKind.Catalogue)]
                internal sealed partial class TestSymbol : SourceDeclaredSymbol, WarHub.ArmouryModel.ICatalogueSymbol
                {
                    private WarHub.ArmouryModel.ICatalogueSymbol? lazyGamesystem;

                    [Bound]
                    public WarHub.ArmouryModel.ICatalogueSymbol Gamesystem =>
                        GetBoundField(ref lazyGamesystem, this,
                            static (b, d, s) => null!);
                }
            }
            """;

        var driver = TestHelper.RunGenerator<BoundGenerator>(source);
        var results = driver.GetRunResult();

        results.Diagnostics.Should().BeEmpty();

        var generatedSource = results.GeneratedTrees
            .Select(t => t.GetText().ToString())
            .First(t => t.Contains("CheckReferencesCore", StringComparison.Ordinal));

        generatedSource.Should().Contain("sealed override void CheckReferencesCore()");
        generatedSource.Should().Contain("_ = Gamesystem;");
    }

    [Fact]
    public void Generates_CheckReferencesCore_with_multiple_Bound_properties_sorted()
    {
        var source = """
            namespace WarHub.ArmouryModel.Concrete
            {
                internal sealed partial class TestMultiSymbol : SourceDeclaredSymbol, WarHub.ArmouryModel.ISymbol
                {
                    private WarHub.ArmouryModel.ISymbol? lazyTarget;
                    private WarHub.ArmouryModel.ISymbol? lazySource;

                    [Bound]
                    public WarHub.ArmouryModel.ISymbol Target =>
                        GetBoundField(ref lazyTarget, this,
                            static (b, d, s) => null!);

                    [Bound]
                    public WarHub.ArmouryModel.ISymbol Alpha =>
                        GetBoundField(ref lazySource, this,
                            static (b, d, s) => null!);
                }
            }
            """;

        var driver = TestHelper.RunGenerator<BoundGenerator>(source);
        var results = driver.GetRunResult();

        var generatedSource = results.GeneratedTrees
            .Select(t => t.GetText().ToString())
            .First(t => t.Contains("CheckReferencesCore", StringComparison.Ordinal));

        // Properties should be accessed in alphabetical order (deterministic output)
        var alphaIndex = generatedSource.IndexOf("_ = Alpha;", StringComparison.Ordinal);
        var targetIndex = generatedSource.IndexOf("_ = Target;", StringComparison.Ordinal);
        alphaIndex.Should().BePositive();
        targetIndex.Should().BePositive();
        alphaIndex.Should().BeLessThan(targetIndex, "properties should be sorted alphabetically");
    }

    [Fact]
    public void No_output_when_no_Bound_properties()
    {
        var source = """
            namespace WarHub.ArmouryModel.Concrete
            {
                internal sealed partial class TestNoBound : SourceDeclaredSymbol, WarHub.ArmouryModel.ISymbol
                {
                    public string Name => "";
                }
            }
            """;

        var driver = TestHelper.RunGenerator<BoundGenerator>(source);
        var results = driver.GetRunResult();

        var generatedTrees = results.GeneratedTrees
            .Where(t => t.GetText().ToString().Contains("CheckReferencesCore"))
            .ToList();

        generatedTrees.Should().BeEmpty("no [Bound] properties means no CheckReferencesCore");
    }

    [Fact]
    public void Emits_BoundAttribute_source()
    {
        var source = """
            namespace Test
            {
                // No usage
            }
            """;

        var driver = TestHelper.RunGenerator<BoundGenerator>(source);
        var results = driver.GetRunResult();

        var attrSource = results.GeneratedTrees
            .Select(t => t.GetText().ToString())
            .First(t => t.Contains("BoundAttribute", StringComparison.Ordinal));

        attrSource.Should().Contain("class BoundAttribute");
    }

    [Fact]
    public void Generated_CheckReferencesCore_compiles_without_errors()
    {
        var source = """
            namespace WarHub.ArmouryModel.Concrete
            {
                internal sealed partial class TestSymbol : SourceDeclaredSymbol, WarHub.ArmouryModel.ICatalogueSymbol
                {
                    public override WarHub.ArmouryModel.SymbolKind Kind => WarHub.ArmouryModel.SymbolKind.Catalogue;
                    public override void Accept(WarHub.ArmouryModel.SymbolVisitor v) => v.VisitCatalogue(this);
                    public override TResult Accept<TResult>(WarHub.ArmouryModel.SymbolVisitor<TResult> v) => v.VisitCatalogue(this);
                    public override TResult Accept<TArgument, TResult>(WarHub.ArmouryModel.SymbolVisitor<TArgument, TResult> v, TArgument a) => v.VisitCatalogue(this, a);

                    private WarHub.ArmouryModel.ICatalogueSymbol? lazyRef;

                    [Bound]
                    public WarHub.ArmouryModel.ICatalogueSymbol MyRef =>
                        GetBoundField(ref lazyRef, this,
                            static (b, d, s) => null!);
                }
            }
            """;

        var (output, errors) = TestHelper.RunGeneratorAndGetOutput<BoundGenerator>(source);

        errors.Should().BeEmpty("generated CheckReferencesCore should compile without errors");
    }
}
