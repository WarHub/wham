// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXmlTests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using BattleScribeXml;
    using BattleScribeXml.GuidMapping;
    using Xunit;

    //public class GuidControllerEditModeTests
    //{
    //    [Fact]
    //    public void CatalogueAlreadyLoadedExceptionTest()
    //    {
    //        var gameSystem = GetAllGuidGameSystem();
    //        var catalogue = GetAllGuidCatalogue();
    //        var controller = GetNewEditableController();
    //        controller.Process(gameSystem);
    //        controller.Process(catalogue);
    //        Assert.Throws<ProcessingFailedException>(() => controller.Process(catalogue));
    //    }

    //    [Fact]
    //    public void CatalogueGameSystemNotLoadedExceptionTest()
    //    {
    //        var catalogue = GetAllGuidCatalogue();
    //        var controller = GetNewEditableController();
    //        Assert.Throws<ProcessingFailedException>(() => controller.Process(catalogue));
    //    }

    //    [Fact]
    //    public void CatalogueIncompatibleGameSystemExceptionTest()
    //    {
    //        var gameSystem = GetAllGuidGameSystem();
    //        var catalogue = GetAllGuidCatalogue();
    //        catalogue.GameSystemId = NewGuid();
    //        var controller = GetNewEditableController();
    //        controller.Process(gameSystem);
    //        Assert.Throws<ProcessingFailedException>(() => controller.Process(catalogue));
    //    }

    //    [Fact]
    //    public void ControllerCtorTest()
    //    {
    //        var controller = GetNewEditableController();
    //        Assert.Equal(GuidControllerMode.Edit, controller.Mode);
    //    }

    //    [Fact]
    //    public void GameSystemAlreadyLoadedExceptionTest()
    //    {
    //        var gameSystem = GetAllGuidGameSystem();
    //        var controller = GetNewEditableController();
    //        controller.Process(gameSystem);
    //        Assert.Throws<ProcessingFailedException>(() => controller.Process(gameSystem));
    //    }

    //    [Fact]
    //    public void NewGuidTest()
    //    {
    //        var guid = NewGuid();
    //        Guid.ParseExact(guid, "D");
    //    }

    //    [Fact]
    //    public void NoCategoryCategoryMockProcessingTest()
    //    {
    //        var controller = new GuidController(GuidControllerMode.Edit);
    //        Assert.Equal(
    //            controller.ParseId(ReservedIdentifiers.NoCategoryName),
    //            ReservedIdentifiers.NoCategoryId);
    //    }

    //    [Fact]
    //    public void NoCategoryCategoryMockReprocessingTest()
    //    {
    //        var controller = new GuidController(GuidControllerMode.Edit);
    //        Assert.Equal(
    //            controller.ParseGuid(ReservedIdentifiers.NoCategoryId),
    //            ReservedIdentifiers.NoCategoryName);
    //    }

    //    [Fact]
    //    public void ProcessCatalogueRegenerateAllTest()
    //    {
    //        var controller = GetNewEditableController();
    //        controller.Process(GetNoGuidGameSystem());
    //        Assert.Equal(
    //            NoGuidGameSystemExpectedSubstitutionsCount,
    //            controller.GeneratedGuidCount);
    //        controller.Process(GetNoGuidCatalogue());
    //        Assert.Equal(
    //            NoGuidGameSystemExpectedSubstitutionsCount
    //            + NoGuidCatalogueExpectedSubstitutionsCount,
    //            controller.GeneratedGuidCount);
    //    }

    //    [Fact]
    //    public void ProcessCatalogueRegenerateNoneTest()
    //    {
    //        var controller = GetNewEditableController();
    //        controller.Process(GetAllGuidGameSystem());
    //        controller.Process(GetAllGuidCatalogue());
    //        Assert.Equal(0, controller.GeneratedGuidCount);
    //    }

    //    [Fact]
    //    public void ProcessCatalogueRegenerateOneTest()
    //    {
    //        var controller = GetNewEditableController();
    //        controller.Process(GetAllGuidGameSystem());
    //        var catalogue = GetAllGuidCatalogue();
    //        catalogue.Id = MakeId(catalogue.Name);
    //        controller.Process(catalogue);
    //        Assert.Equal(1, controller.GeneratedGuidCount);
    //    }

    //    [Fact]
    //    public void ProcessGstRegenerateAllTest()
    //    {
    //        var controller = GetNewEditableController();
    //        var gameSystem = GetNoGuidGameSystem();
    //        controller.Process(gameSystem);
    //        Assert.Equal(
    //            NoGuidGameSystemExpectedSubstitutionsCount,
    //            controller.GeneratedGuidCount);
    //    }

    //    [Fact]
    //    public void ProcessGstRegenerateNoneTest()
    //    {
    //        var controller = GetNewEditableController();
    //        controller.Process(GetAllGuidGameSystem());
    //        Assert.Equal(0, controller.GeneratedGuidCount);
    //    }

    //    [Fact]
    //    public void ProcessGstRegenerateOneTest()
    //    {
    //        var controller = GetNewEditableController();
    //        var gameSystem = GetAllGuidGameSystem();
    //        gameSystem.Id = "someRandomId";
    //        controller.Process(gameSystem);
    //        Assert.Equal(1, controller.GeneratedGuidCount);
    //    }

    //    [Fact]
    //    public void ProcessRosterRegenerateAllTest()
    //    {
    //        var controller = GetNewEditableController();
    //        controller.Process(GetNoGuidGameSystem());
    //        Assert.Equal(
    //            NoGuidGameSystemExpectedSubstitutionsCount,
    //            controller.GeneratedGuidCount);
    //        controller.Process(GetNoGuidCatalogue());
    //        Assert.Equal(
    //            NoGuidGameSystemExpectedSubstitutionsCount
    //            + NoGuidCatalogueExpectedSubstitutionsCount,
    //            controller.GeneratedGuidCount);
    //        controller.Process(GetNoGuidRoster());
    //        Assert.Equal(
    //            NoGuidGameSystemExpectedSubstitutionsCount
    //            + NoGuidCatalogueExpectedSubstitutionsCount
    //            + NoGuidRosterExpectedSubstitutionsCount,
    //            controller.GeneratedGuidCount);
    //    }

    //    [Fact]
    //    public void ProcessRosterRegenerateNoneTest()
    //    {
    //        var controller = GetNewEditableController();
    //        controller.Process(GetAllGuidGameSystem());
    //        controller.Process(GetAllGuidCatalogue());
    //        controller.Process(GetAllGuidRoster());
    //        Assert.Equal(0, controller.GeneratedGuidCount);
    //    }

    //    [Fact]
    //    public void ProcessRosterRegenerateOneTest()
    //    {
    //        var controller = GetNewEditableController();
    //        controller.Process(GetAllGuidGameSystem());
    //        controller.Process(GetAllGuidCatalogue());
    //        var roster = GetAllGuidRoster();
    //        roster.Forces[0].Categories[0].Selections[0].Id = MakeId("some selection id");
    //        controller.Process(roster);
    //        Assert.Equal(1, controller.GeneratedGuidCount);
    //    }

    //    [Fact]
    //    public void RosterAlreadyLoadedExceptionTest()
    //    {
    //        var catalogue = GetAllGuidCatalogue();
    //        var controller = GetNewEditableController();
    //        Assert.Throws<ProcessingFailedException>(() => controller.Process(catalogue));
    //    }

    //    [Fact]
    //    public void RosterGameSystemNotLoadedExceptionTest()
    //    {
    //        var roster = GetAllGuidRoster();
    //        var controller = GetNewEditableController();
    //        Assert.Throws<ProcessingFailedException>(() => controller.Process(roster));
    //    }

    //    [Fact]
    //    public void RosterIncompatibleGameSystemExceptionTest()
    //    {
    //        var gameSystem = GetAllGuidGameSystem();
    //        var catalogue = GetAllGuidCatalogue();
    //        var roster = GetAllGuidRoster();
    //        roster.GameSystemId = NewGuid();
    //        var controller = GetNewEditableController();
    //        controller.Process(gameSystem);
    //        controller.Process(catalogue);
    //        Assert.Throws<ProcessingFailedException>(() => controller.Process(roster));
    //    }

    //    [Fact]
    //    public void RosterRequiredCatalogueNotLoadedExceptionTest()
    //    {
    //        var gameSystem = GetAllGuidGameSystem();
    //        var catalogue = GetAllGuidCatalogue();
    //        var roster = GetAllGuidRoster();
    //        roster.Forces[0].CatalogueId = NewGuid();
    //        var controller = GetNewEditableController();
    //        controller.Process(gameSystem);
    //        controller.Process(catalogue);
    //        Assert.Throws<ProcessingFailedException>(() => controller.Process(roster));
    //    }

    //    private static GuidController GetNewEditableController()
    //    {
    //        return new GuidController(GuidControllerMode.Edit);
    //    }

    //    private static string MakeId(string guid)
    //    {
    //        var result = new StringBuilder();
    //        guid.ToCharArray().ForEach(c => result.AppendFormat("{0:x}", (int) c));
    //        return result.ToString();
    //    }

    //    private static string NewGuid()
    //    {
    //        return Guid.NewGuid().ToString("D");
    //    }

    //    #region sample data

    //    private static GameSystem GetAllGuidGameSystem()
    //    {
    //        return new GameSystem
    //        {
    //            Name = "Good game system",
    //            Id = "c812efa0-73b1-47a1-aa27-03e8533b64a0",
    //            ForceTypes = new List<ForceType>
    //            {
    //                new ForceType
    //                {
    //                    Name = "Parent force type",
    //                    Id = "8615a14a-272d-4446-8311-bfe2e560a5ba",
    //                    Categories = new List<Category>
    //                    {
    //                        new Category
    //                        {
    //                            Name = "Some category",
    //                            Id = "47347e82-e5ec-4ad6-8edf-59b42654c072"
    //                        }
    //                    },
    //                    ForceTypes = new List<ForceType>
    //                    {
    //                        new ForceType
    //                        {
    //                            Name = "Nested force type 1",
    //                            Id = NewGuid()
    //                        },
    //                        new ForceType
    //                        {
    //                            Name = "Nested force type 2",
    //                            Id = NewGuid()
    //                        }
    //                    }
    //                },
    //                new ForceType
    //                {
    //                    Name = "Second force type",
    //                    Id = NewGuid()
    //                }
    //            },
    //            ProfileTypes = new List<ProfileType>
    //            {
    //                new ProfileType
    //                {
    //                    Name = "Filled profile type",
    //                    Id = NewGuid(),
    //                    Characteristics = new List<CharacteristicType>
    //                    {
    //                        new CharacteristicType
    //                        {
    //                            Name = "char type 1",
    //                            Id = NewGuid()
    //                        },
    //                        new CharacteristicType
    //                        {
    //                            Name = "char type 2",
    //                            Id = NewGuid()
    //                        }
    //                    }
    //                },
    //                new ProfileType
    //                {
    //                    Name = "Half-filled profile type",
    //                    Id = NewGuid(),
    //                    Characteristics = new List<CharacteristicType>
    //                    {
    //                        new CharacteristicType
    //                        {
    //                            Name = "char type 3",
    //                            Id = NewGuid()
    //                        }
    //                    }
    //                }
    //            }
    //        };
    //    }

    //    private static GameSystem GetNoGuidGameSystem()
    //    {
    //        return new GameSystem
    //        {
    //            Name = "Good game system",
    //            Id = MakeId("Good game system"),
    //            ForceTypes = new List<ForceType>
    //            {
    //                new ForceType
    //                {
    //                    Name = "Parent force type",
    //                    Id = MakeId("Parent force type"),
    //                    Categories = new List<Category>
    //                    {
    //                        new Category
    //                        {
    //                            Name = "Single category",
    //                            Id = MakeId("Single category")
    //                        }
    //                    },
    //                    ForceTypes = new List<ForceType>
    //                    {
    //                        new ForceType
    //                        {
    //                            Name = "Nested force type 1",
    //                            Id = MakeId("Nested force type 1")
    //                        },
    //                        new ForceType
    //                        {
    //                            Name = "Nested force type 2",
    //                            Id = MakeId("Nested force type 2")
    //                        }
    //                    }
    //                },
    //                new ForceType
    //                {
    //                    Name = "Second force type",
    //                    Id = MakeId("Second force type")
    //                }
    //            },
    //            ProfileTypes = new List<ProfileType>
    //            {
    //                new ProfileType
    //                {
    //                    Name = "Filled profile type",
    //                    Id = MakeId("Filled profile type"),
    //                    Characteristics = new List<CharacteristicType>
    //                    {
    //                        new CharacteristicType
    //                        {
    //                            Name = "char type 1",
    //                            Id = MakeId("char type 1")
    //                        },
    //                        new CharacteristicType
    //                        {
    //                            Name = "char type 2",
    //                            Id = MakeId("char type 2")
    //                        }
    //                    }
    //                },
    //                new ProfileType
    //                {
    //                    Name = "Half-filled profile type",
    //                    Id = MakeId("Half-filled profile type"),
    //                    Characteristics = new List<CharacteristicType>
    //                    {
    //                        new CharacteristicType
    //                        {
    //                            Name = "char type 3",
    //                            Id = MakeId("char type 3")
    //                        }
    //                    }
    //                }
    //            }
    //        };
    //    }

    //    private static Catalogue GetAllGuidCatalogue()
    //    {
    //        return new Catalogue
    //        {
    //            Name = "Some catalogue",
    //            Id = "95e49441-b80e-41d9-bc99-75b19e2ebfe8",
    //            GameSystemId = AllGuidGameSystem.Id,
    //            Entries = new List<Entry>
    //            {
    //                new Entry
    //                {
    //                    Name = "First Entry",
    //                    Id = "6810b90d-cd3f-4c4d-9f6f-f9db72f0536a",
    //                    CategoryId = AllGuidGameSystem.ForceTypes[0].Categories[0].Id
    //                }
    //            },
    //            Rules = new List<Rule>
    //            {
    //                new Rule
    //                {
    //                    Name = "Some rule",
    //                    Id = NewGuid()
    //                }
    //            },
    //            SharedEntryGroups = new List<EntryGroup>
    //            {
    //                new EntryGroup
    //                {
    //                    Name = "Some entry group",
    //                    Id = NewGuid(),
    //                    DefaultEntryId = NewGuid()
    //                }
    //            },
    //            SharedProfiles = new List<Profile>
    //            {
    //                new Profile
    //                {
    //                    Name = "Some profile",
    //                    Id = NewGuid(),
    //                    ProfileTypeId = NewGuid()
    //                }
    //            }
    //        };
    //    }

    //    private static Catalogue GetNoGuidCatalogue()
    //    {
    //        return new Catalogue
    //        {
    //            Name = "Some catalogue",
    //            Id = MakeId("Some catalogue"),
    //            GameSystemId = NoGuidGameSystem.Id,
    //            Entries = new List<Entry>
    //            {
    //                new Entry
    //                {
    //                    Name = "First Entry",
    //                    Id = MakeId("First Entry"),
    //                    CategoryId = NoGuidGameSystem.ForceTypes[0].Categories[0].Id
    //                }
    //            },
    //            Rules = new List<Rule>
    //            {
    //                new Rule
    //                {
    //                    Name = "Some rule",
    //                    Id = MakeId("Some rule")
    //                }
    //            },
    //            SharedEntryGroups = new List<EntryGroup>
    //            {
    //                new EntryGroup
    //                {
    //                    Name = "Some entry group",
    //                    Id = MakeId("Some entry group"),
    //                    DefaultEntryId = "12345"
    //                }
    //            },
    //            SharedProfiles = new List<Profile>
    //            {
    //                new Profile
    //                {
    //                    Name = "Some profile",
    //                    Id = MakeId("Some profile"),
    //                    ProfileTypeId = NoGuidGameSystem.ProfileTypes[0].Characteristics[0].Id
    //                }
    //            }
    //        };
    //    }

    //    private static Roster GetAllGuidRoster()
    //    {
    //        return new Roster
    //        {
    //            Name = "Some roster",
    //            Id = NewGuid(),
    //            GameSystemId = AllGuidGameSystem.Id,
    //            Forces = new List<Force>
    //            {
    //                new Force
    //                {
    //                    Id = "4eac65c6-e609-41f3-9fd3-1fd11ff62199",
    //                    CatalogueId = AllGuidCatalogue.Id,
    //                    ForceTypeId = AllGuidGameSystem.ForceTypes[0].Id,
    //                    Categories = new List<CategoryMock>
    //                    {
    //                        new CategoryMock
    //                        {
    //                            Id = NewGuid(),
    //                            CategoryId = AllGuidGameSystem.ForceTypes[0].Categories[0].Id,
    //                            Selections = new List<Selection>
    //                            {
    //                                new Selection
    //                                {
    //                                    EntryId = AllGuidCatalogue.Entries[0].Id,
    //                                    Id = NewGuid()
    //                                }
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        };
    //    }

    //    private static Roster GetNoGuidRoster()
    //    {
    //        return new Roster
    //        {
    //            Name = "Some roster 2",
    //            Id = MakeId("Some roster 2"),
    //            GameSystemId = NoGuidGameSystem.Id,
    //            Forces = new List<Force>
    //            {
    //                new Force
    //                {
    //                    Id = MakeId("Some catalogue link id"),
    //                    CatalogueId = NoGuidCatalogue.Id,
    //                    ForceTypeId = NoGuidGameSystem.ForceTypes[0].Id,
    //                    Categories = new List<CategoryMock>
    //                    {
    //                        new CategoryMock
    //                        {
    //                            Name = "Some category mock",
    //                            Id = MakeId("Some category mock"),
    //                            CategoryId = NoGuidGameSystem.ForceTypes[0].Categories[0].Id,
    //                            Selections = new List<Selection>
    //                            {
    //                                new Selection
    //                                {
    //                                    EntryId = NoGuidCatalogue.Entries[0].Id,
    //                                    Name = "Some selection",
    //                                    Id = MakeId("Some selection")
    //                                }
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        };
    //    }

    //    #endregion sample data

    //    #region game system

    //    private const int NoGuidGameSystemExpectedSubstitutionsCount = 11;

    //    private static GameSystem AllGuidGameSystem { get; } = GetAllGuidGameSystem();

    //    private static GameSystem NoGuidGameSystem { get; } = GetNoGuidGameSystem();

    //    #endregion game system

    //    #region catalogue

    //    private const int NoGuidCatalogueExpectedSubstitutionsCount = 6;

    //    private static Catalogue AllGuidCatalogue { get; } = GetAllGuidCatalogue();

    //    private static Catalogue NoGuidCatalogue { get; } = GetNoGuidCatalogue();

    //    #endregion catalogue

    //    #region roster

    //    private const int NoGuidRosterExpectedSubstitutionsCount = 4;

    //    private static Roster AllGuidRoster { get; } = GetAllGuidRoster();

    //    private static Roster NoGuidRoster { get; } = GetNoGuidRoster();

    //    #endregion roster
    //}
}
