// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.Services
{
    using System;
    using System.Linq;
    using Repo;

    public class SampleObjectFactory
    {
        private RemoteDataSourceIndex _bsi;
        private ICatalogue _cat;
        private IGameSystem _gst;
        private IRoster _ros;

        public SampleDataInfos Infos { get; } = new SampleDataInfos();

        public ICatalogue SampleCatalogue => _cat ?? (_cat = GetNewCatalogue());

        public IGameSystem SampleGameSystem => _gst ?? (_gst = GetNewGameSystem());

        public IRoster SampleRoster => _ros ?? (_ros = GetNewRoster());

        public RemoteDataSourceIndex SampleSourceIndex => _bsi ?? (_bsi = GetSourceIndex());

        private static void FillCatalogue(ICatalogue catalogue, IGameSystem system)
        {
            var captain = catalogue.Entries.AddNew();
            captain.CategoryLink.Target = system.ForceTypes[0].Categories[0];
            SetupCaptain(captain);
            var troopUnit = catalogue.Entries.AddNew();
            troopUnit.Name = "Shooters";
            troopUnit.CategoryLink.Target = system.ForceTypes[0].Categories[1];
            var trooper = troopUnit.Entries.AddNew();
            trooper.Name = "Shooter";
            trooper.PointCost = 14m;
            trooper.Limits.SelectionsLimit.Min = 4;
            trooper.Limits.SelectionsLimit.Max = 12;
            var leader = troopUnit.Entries.AddNew();
            leader.Name = "Leader";
            leader.PointCost = 20m;
            leader.Limits.SelectionsLimit.Max = 1;
            var fighter = catalogue.Entries.AddNew();
            fighter.Name = "Fighter jet";
            fighter.PointCost = 155m;
            fighter.CategoryLink.Target = system.ForceTypes[1].Categories[0];
        }

        private static void SetupCaptain(IEntry captain)
        {
            captain.Name = "Captain";
            captain.PointCost = 100m;
            captain.Limits.InForceLimit.Min = 1;
            var armour = captain.Entries.AddNew();
            // static armour
            armour.Name = "Armour";
            armour.PointCost = 0m;
            armour.Limits.SelectionsLimit.SetValues(1, 1);
            // binary relic
            var relic = captain.Entries.AddNew();
            relic.Name = "Relic";
            relic.PointCost = 25m;
            relic.Limits.SelectionsLimit.SetValues(0, 1);
            // multiple grenades
            var grenade = captain.Entries.AddNew();
            grenade.Name = "Grenade";
            grenade.PointCost = 2;
            grenade.Limits.SelectionsLimit.SetValues(0, 5);
            // complex honour guard
            var honourGuard = captain.Entries.AddNew();
            honourGuard.Name = "Honour Guard";
            honourGuard.Limits.SelectionsLimit.SetValues(0, 4);
            honourGuard.PointCost = 40;
            var honourGuardWeaponGroup = honourGuard.Groups.AddNew();
            honourGuardWeaponGroup.Name = "Weapon:";
            honourGuardWeaponGroup.Limits.SelectionsLimit.SetValues(1, 1);
            {
                var hgSword = honourGuardWeaponGroup.Entries.AddNew();
                hgSword.Name = "Sword";
                hgSword.PointCost = 0;
                var hgHammer = honourGuardWeaponGroup.Entries.AddNew();
                hgHammer.Name = "War Hammer";
                hgHammer.PointCost = 15;
                honourGuardWeaponGroup.DefaultChoice = hgSword;
            }
            // weapons - radio group
            var weapons = captain.Groups.AddNew();
            weapons.Name = "Melee Weapon:";
            weapons.Limits.SelectionsLimit.SetValues(1, 1);
            var sword = weapons.Entries.AddNew();
            sword.Name = "Sword";
            sword.PointCost = 0;
            var hammer = weapons.Entries.AddNew();
            hammer.Name = "War Hammer";
            hammer.PointCost = 15;
            weapons.DefaultChoice = sword;
            // wargear - multiple group
            var wargear = captain.Groups.AddNew();
            wargear.Name = "Wargear:";
            var comms = wargear.Entries.AddNew();
            comms.Name = "Comms array";
            comms.PointCost = 5;
            comms.Limits.SelectionsLimit.SetValues(0, 1);
            var shield = wargear.Entries.AddNew();
            shield.Name = "Power Shield";
            shield.PointCost = 15;
            shield.Limits.SelectionsLimit.SetValues(0, 1);
        }

        private static void FillRoster(IRoster roster, ICatalogue catalogue, IGameSystem system)
        {
            var force = roster.Forces.AddNew(
                new ForceNodeArgument(catalogue, system.ForceTypes[0]));
            var captainEntry = catalogue.Entries.First(x => x.Name.Equals("Captain"));
            var path = new CataloguePath(catalogue);
            var captain = force.CategoryMocks.First(x => x.Name == "HQ").Selections.AddNew(path.Select(captainEntry));
            var honourGuardEntry = captainEntry.GetSubEntries().First(group => group.Name.Contains("Honour Guard"));
            var hgPath = path.Select(captainEntry).Select(honourGuardEntry);
            var honourGuard1 = captain.Selections.AddNew(hgPath);
            if (honourGuard1.NumberTaken != 1)
                throw new InvalidOperationException(
                    $"Error adding single Honour Guard in {nameof(SampleObjectFactory)}.");
            var honourGuard2 = captain.Selections.AddNew(hgPath);
            var hgWeaponGroup = honourGuardEntry.GetSubGroups().Single();
            var hgWarhammer = hgWeaponGroup.GetSubEntries().First(entry => entry.Name.Contains("War Hammer"));
            honourGuard2.Selections.Remove(honourGuard2.Selections.Single());
            honourGuard2.Selections.AddNew(hgPath.Select(hgWeaponGroup).Select(hgWarhammer));
            var troopEntry = catalogue.Entries.First(x => x.Name.Equals("Shooters"));
            var troopsCategory = force.CategoryMocks.First(x => x.Name == "Troops");
            troopsCategory.Selections.AddNew(path.Select(troopEntry));
            troopsCategory.Selections.AddNew(path.Select(troopEntry));
            var specialForce = roster.Forces.AddNew(
                new ForceNodeArgument(catalogue, system.ForceTypes[1]));
            var jetEntry = catalogue.Entries.First(x => x.Name.Equals("Fighter jet"));
            specialForce.CategoryMocks.First(x => x.Name == "Elite").Selections.AddNew(path.Select(jetEntry));
        }

        private static void FillSystem(IGameSystem system)
        {
            var regularType = system.ForceTypes.AddNew();
            regularType.Name = "Regular detachment";
            var hqCat = regularType.Categories.AddNew();
            hqCat.Name = "HQ";
            var troopsCat = regularType.Categories.AddNew();
            troopsCat.Name = "Troops";
            var supportCat = regularType.Categories.AddNew();
            supportCat.Name = "Support";
            var specialType = system.ForceTypes.AddNew();
            specialType.Name = "Special detachment";
            var eliteCat = specialType.Categories.AddNew();
            eliteCat.Name = "Elite";
        }

        private ICatalogue GetNewCatalogue()
        {
            var catalogue = RepoObjectFactory.CreateCatalogue(Infos.SampleCatInfo, SampleGameSystem.Context);
            FillCatalogue(catalogue, SampleGameSystem);
            return catalogue;
        }

        private IGameSystem GetNewGameSystem()
        {
            var system = RepoObjectFactory.CreateGameSystem(Infos.SampleGstInfo);
            FillSystem(system);
            return system;
        }

        private IRoster GetNewRoster()
        {
            var roster = RepoObjectFactory.CreateRoster(Infos.SampleRosInfo, SampleGameSystem.Context);
            FillRoster(roster, SampleCatalogue, SampleGameSystem);
            return roster;
        }

        private RemoteDataSourceIndex GetSourceIndex()
        {
            var originVersion = Infos.SampleGstInfo.OriginProgramVersion;
            var index = new RemoteDataSourceIndex
            {
                Name = "Age of Modern Warfare",
                IndexUri = new Uri("http://example.com/index.bsi"),
                OriginProgramVersion = originVersion
            };
            index.RemoteDataInfos.AddRange(new[]
            {
                new RemoteDataInfo("/usmarinecorps.cat", Infos.SampleCatInfo.Name, originVersion,
                    Infos.SampleCatInfo.RawId, Infos.SampleCatInfo.Revision + 3, RemoteDataType.Catalogue),
                new RemoteDataInfo("/AoMW (2015).gst", Infos.SampleGstInfo.Name, originVersion,
                    Infos.SampleGstInfo.RawId, Infos.SampleGstInfo.Revision, RemoteDataType.GameSystem),
                new RemoteDataInfo("/usnavy.cat", "US Navy (2015)", originVersion, "vbn-snyh-sfbj-weol", 1,
                    RemoteDataType.Catalogue)
            });
            return index;
        }
    }
}
