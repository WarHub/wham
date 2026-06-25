using FluentAssertions;
using Microsoft.CodeAnalysis;
using WarHub.ArmouryModel.Concrete.Generators;
using Xunit;

namespace WarHub.ArmouryModel.Concrete.Extensions.Generators.Tests;

public class BoundAnalyzerTests
{
    [Fact]
    public void WHAM001_GetBoundField_without_Bound_attribute_reports_diagnostic()
    {
        var source = """
            namespace WarHub.ArmouryModel.Concrete
            {
                internal sealed partial class TestSymbol : SourceDeclaredSymbol, WarHub.ArmouryModel.ISymbol
                {
                    private WarHub.ArmouryModel.ISymbol? lazyRef;

                    public WarHub.ArmouryModel.ISymbol MyRef =>
                        GetBoundField(ref lazyRef, this,
                            static (b, d, s) => null!);
                }
            }
            """;

        var diagnostics = TestHelper.RunAnalyzer(source);

        diagnostics.Should().ContainSingle(d => d.Id == "WHAM001");
    }

    [Fact]
    public void WHAM001_GetBoundField_with_Bound_attribute_no_diagnostic()
    {
        var source = """
            namespace WarHub.ArmouryModel.Concrete
            {
                internal sealed partial class TestSymbol : SourceDeclaredSymbol, WarHub.ArmouryModel.ISymbol
                {
                    private WarHub.ArmouryModel.ISymbol? lazyRef;

                    [Bound]
                    public WarHub.ArmouryModel.ISymbol MyRef =>
                        GetBoundField(ref lazyRef, this,
                            static (b, d, s) => null!);
                }
            }
            """;

        var diagnostics = TestHelper.RunAnalyzer(source);

        diagnostics.Where(d => d.Id == "WHAM001").Should().BeEmpty();
    }

    [Fact]
    public void WHAM002_non_static_lambda_reports_diagnostic()
    {
        var source = """
            namespace WarHub.ArmouryModel.Concrete
            {
                internal sealed partial class TestSymbol : SourceDeclaredSymbol, WarHub.ArmouryModel.ISymbol
                {
                    private WarHub.ArmouryModel.ISymbol? lazyRef;

                    [Bound]
                    public WarHub.ArmouryModel.ISymbol MyRef =>
                        GetBoundField(ref lazyRef, this,
                            (b, d, s) => null!);
                }
            }
            """;

        var diagnostics = TestHelper.RunAnalyzer(source);

        diagnostics.Should().ContainSingle(d => d.Id == "WHAM002");
    }

    [Fact]
    public void WHAM002_static_lambda_no_diagnostic()
    {
        var source = """
            namespace WarHub.ArmouryModel.Concrete
            {
                internal sealed partial class TestSymbol : SourceDeclaredSymbol, WarHub.ArmouryModel.ISymbol
                {
                    private WarHub.ArmouryModel.ISymbol? lazyRef;

                    [Bound]
                    public WarHub.ArmouryModel.ISymbol MyRef =>
                        GetBoundField(ref lazyRef, this,
                            static (b, d, s) => null!);
                }
            }
            """;

        var diagnostics = TestHelper.RunAnalyzer(source);

        diagnostics.Where(d => d.Id == "WHAM002").Should().BeEmpty();
    }

    [Fact]
    public void WHAM001_GetBoundField_outside_property_no_diagnostic()
    {
        var source = """
            namespace WarHub.ArmouryModel.Concrete
            {
                internal sealed partial class TestSymbol : SourceDeclaredSymbol, WarHub.ArmouryModel.ISymbol
                {
                    private WarHub.ArmouryModel.ISymbol? lazyRef;

                    public void SomeMethod()
                    {
                        _ = GetBoundField(ref lazyRef, this,
                            static (b, d, s) => null!);
                    }
                }
            }
            """;

        var diagnostics = TestHelper.RunAnalyzer(source);

        diagnostics.Where(d => d.Id == "WHAM001").Should().BeEmpty();
    }
}
