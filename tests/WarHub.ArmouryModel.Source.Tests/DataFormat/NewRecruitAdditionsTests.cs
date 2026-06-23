using System.IO;
using FluentAssertions;
using WarHub.ArmouryModel.Source.BattleScribe;
using Xunit;
using static WarHub.ArmouryModel.Source.NodeFactory;

namespace WarHub.ArmouryModel.Source.Tests.DataFormat
{
    /// <summary>
    /// NewRecruit extended the BattleScribe data format with new nodes, enum values, and attributes.
    /// These are modelled in wham as part of the format; the tests assert they (a) validate against the
    /// schema and (b) survive a serialize → deserialize round-trip.
    /// </summary>
    public class NewRecruitAdditionsTests
    {
        private static GamesystemNode BuildGamesystemWithAdditions() =>
            Gamesystem()
            .AddProfileTypes(
                ProfileType("ptype")
                .WithKindValue("Cost")
                .AddCharacteristicTypes(
                    CharacteristicType("ctype")
                    .WithKindValue("Annotation")
                    .WithDefaultValue("0"))
                .AddAttributeTypes(
                    AttributeType(comment: null, id: "atype-1", name: "Attr")))
            .AddSelectionEntries(
                SelectionEntry("Squad", "se-1")
                .WithType(SelectionEntryKind.UnitGroup)
                .AddConstraints(
                    Constraint(field: "selections", scope: "parent", value: 1m, id: "con-1", type: ConstraintKind.Exactly)
                    .WithNegative(true)
                    .WithAutomatic(true)
                    .WithMessage("custom message"))
                .AddAssociations(
                    Association(
                        comment: null,
                        field: "selections",
                        scope: "parent",
                        value: 0m,
                        isValuePercentage: false,
                        shared: false,
                        includeChildSelections: false,
                        includeChildForces: false,
                        id: "assoc-1",
                        name: "Bodyguard",
                        min: 1,
                        max: 2,
                        childId: null))
                .AddModifiers(
                    Modifier(type: ModifierKind.Multiply, field: "points")
                    .AddConditionGroups(ConditionGroup(type: ConditionGroupKind.GreaterOrEqual))
                    .AddLocalConditionGroups(
                        LocalConditionGroup(
                            comment: null,
                            field: "selections",
                            scope: "parent",
                            value: 1m,
                            isValuePercentage: false,
                            shared: false,
                            includeChildSelections: false,
                            includeChildForces: false,
                            childId: null,
                            type: ConditionKind.Before,
                            repeatCount: 1))));

        [Fact]
        public void NewRecruit_additions_are_schema_validated()
        {
            var catalogue =
                Catalogue(BuildGamesystemWithAdditions())
                .AddSharedForceEntries(ForceEntry().WithName("Detachment"))
                .AddSharedAssociations(
                    Association(
                        comment: null,
                        field: "selections",
                        scope: "force",
                        value: 0m,
                        isValuePercentage: false,
                        shared: false,
                        includeChildSelections: false,
                        includeChildForces: false,
                        id: "assoc-shared",
                        name: "Warlord",
                        min: 0,
                        max: 1,
                        childId: null));

            var messages = SchemaUtils.Validate(catalogue);

            messages.Should().BeEmpty();
        }

        [Fact]
        public void NewRecruit_additions_round_trip()
        {
            var original = BuildGamesystemWithAdditions();
            using var stream = new MemoryStream();
            original.Serialize(stream);
            stream.Position = 0;

            var gst = stream.DeserializeGamesystem()!;

            var entry = gst.SelectionEntries[0];
            entry.Type.Should().Be(SelectionEntryKind.UnitGroup);

            var constraint = entry.Constraints[0];
            constraint.Type.Should().Be(ConstraintKind.Exactly);
            constraint.Negative.Should().BeTrue();
            constraint.Automatic.Should().BeTrue();
            constraint.Message.Should().Be("custom message");

            entry.Associations.Should().ContainSingle()
                .Which.Min.Should().Be(1);

            var modifier = entry.Modifiers[0];
            modifier.Type.Should().Be(ModifierKind.Multiply);
            modifier.ConditionGroups[0].Type.Should().Be(ConditionGroupKind.GreaterOrEqual);
            modifier.LocalConditionGroups.Should().ContainSingle()
                .Which.Type.Should().Be(ConditionKind.Before);

            var profileType = gst.ProfileTypes[0];
            profileType.KindValue.Should().Be("Cost");
            profileType.AttributeTypes.Should().ContainSingle().Which.Name.Should().Be("Attr");
            profileType.CharacteristicTypes[0].KindValue.Should().Be("Annotation");
            profileType.CharacteristicTypes[0].DefaultValue.Should().Be("0");
        }

        [Fact]
        public void Default_constraint_flags_are_omitted_from_output()
        {
            // negative/automatic carry [DefaultValue(false)], so a constraint that does not set them
            // serialises without the attributes — original BattleScribe data round-trips unchanged.
            var gst =
                Gamesystem()
                .AddSelectionEntries(
                    SelectionEntry()
                    .AddConstraints(Constraint()));

            using var writer = new StringWriter();
            gst.Serialize(writer);
            var xml = writer.ToString();

            xml.Should().NotContain("negative=");
            xml.Should().NotContain("automatic=");
        }
    }
}
