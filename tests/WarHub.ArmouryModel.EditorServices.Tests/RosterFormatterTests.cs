using FluentAssertions;
using WarHub.ArmouryModel.EditorServices.Formatting;
using WarHub.ArmouryModel.Source;
using Xunit;
using static WarHub.ArmouryModel.Source.NodeFactory;

namespace WarHub.ArmouryModel.EditorServices;

public class RosterFormatterTests
{
    private static RosterNode CreateRoster() =>
        Roster(Gamesystem("TestGst"), "Test Roster");

    [Fact]
    public void Format_Json_ReturnsValidJsonContainingRosterName()
    {
        var roster = CreateRoster();
        var format = new RosterFormat { Method = FormatMethod.Json };

        var result = RosterFormatter.Format(roster, format);

        result.Should().Contain("\"name\"").And.Contain("Test Roster");
    }

    [Fact]
    public void Format_Handlebars_WithSimpleTemplate_ReturnsRosterName()
    {
        var roster = CreateRoster();
        var format = new RosterFormat { Method = FormatMethod.Handlebars, Template = "{{roster.name}}" };

        var result = RosterFormatter.Format(roster, format);

        result.Should().Be("Test Roster");
    }

    [Fact]
    public void Format_Handlebars_WithNullTemplate_ReturnsEmptyString()
    {
        var roster = CreateRoster();
        var format = new RosterFormat { Method = FormatMethod.Handlebars, Template = null };

        var result = RosterFormatter.Format(roster, format);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Format_Handlebars_WithInvalidTemplate_ReturnsErrorMessage()
    {
        var roster = CreateRoster();
        var format = new RosterFormat { Name = "BadTemplate", Method = FormatMethod.Handlebars, Template = "{{#each}}{{/if}}" };

        var result = RosterFormatter.Format(roster, format);

        result.Should().Contain("Error:");
    }

    [Fact]
    public void Format_UnknownFormatMethod_ThrowsArgumentException()
    {
        var roster = CreateRoster();
        var format = new RosterFormat { Method = (FormatMethod)999 };

        var act = () => RosterFormatter.Format(roster, format);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void BuiltinFormatters_ReturnsNonEmptyCollection()
    {
        RosterFormatter.BuiltinFormatters.Should().NotBeEmpty();
    }
}
