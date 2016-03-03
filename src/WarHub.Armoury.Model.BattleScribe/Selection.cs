// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ModelBases;
    using Nodes;
    using Repo;

    public class Selection : IdentifiedNamedIndexedModelBase<BattleScribeXml.Selection>, ISelection
    {
        private readonly LinkPath<IEntry> _originEntryPath;
        private readonly LinkPath<IGroup> _originGroupPath;
        private readonly ProfileMockNode _profileMocksNode;
        private readonly RuleMockNode _ruleMocksNode;
        private readonly SelectionNode _selectionsNode;
        private readonly EntryType _type;
        private IForceContext _forceContext;

        public Selection(BattleScribeXml.Selection xml)
            : base(xml)
        {
            _originEntryPath = new LinkPath<IEntry>(
                XmlBackend.EntryGuids,
                newList => XmlBackend.EntryGuids = newList,
                () => XmlBackend.EntryId);
            _originGroupPath = new LinkPath<IGroup>(
                XmlBackend.EntryGroupGuids,
                newList => XmlBackend.EntryGroupGuids = newList,
                () => XmlBackend.EntryGroupId);
            _profileMocksNode = new ProfileMockNode(() => XmlBackend.Profiles, this)
            {
                Controller = XmlBackend.Controller
            };
            _ruleMocksNode = new RuleMockNode(() => XmlBackend.Rules, this) {Controller = XmlBackend.Controller};
            _selectionsNode = new SelectionNode(() => XmlBackend.Selections, this) {Controller = XmlBackend.Controller};
            xml.Type.ParseXml(out _type);
        }

        public event PointCostChangedEventHandler PointCostChanged;

        public IForceContext ForceContext
        {
            get { return _forceContext; }
            set
            {
                var oldValue = _forceContext;
                if (!Set(ref _forceContext, value))
                {
                    return;
                }
                oldValue?.Selections.Deregister(this);
                ICatalogueContext catalogueContext = null;
                if (value != null)
                {
                    catalogueContext = value.SourceCatalogue.Context;
                    catalogueContext.Entries.SetTargetOf(OriginEntryPath);
                    if (!OriginGroupPath.TargetId.Value.Equals(Guid.Empty))
                    {
                        catalogueContext.Groups.SetTargetOf(OriginGroupPath);
                    }
                    value.Selections.Register(this);
                }
                _originEntryPath.SetCatalogueContext(catalogueContext);
                _originGroupPath.SetCatalogueContext(catalogueContext);
                ProfileMocks.ChangeContext(value);
                RuleMocks.ChangeContext(value);
                Selections.ChangeContext(value);
            }
        }

        public uint NumberTaken
        {
            get { return XmlBackend.Number; }
            set
            {
                Set(XmlBackend.Number, value, () =>
                {
                    XmlBackend.Number = value;
                    PointCost = value*OriginEntryPath.Target.PointCost;
                });
            }
        }

        public ILinkPath<IEntry> OriginEntryPath => _originEntryPath;

        public ILinkPath<IGroup> OriginGroupPath => _originGroupPath;

        public decimal PointCost
        {
            get { return XmlBackend.Points; }
            protected set { SetPointCost(value); }
        }

        public IEnumerable<IProfileMock> ProfileMocks => _profileMocksNode;

        public IEnumerable<IRuleMock> RuleMocks => _ruleMocksNode;

        public INode<ISelection, CataloguePath> Selections => _selectionsNode;

        public EntryType Type => _type;

        public static Selection CreateFrom(CataloguePath path)
        {
            var entry = (IEntry) path.Last();
            var guid = Guid.NewGuid();
            var xml = new BattleScribeXml.Selection
            {
                Book = entry.Book.Title,
                EntryGroupGuids = path.GetGroupGuids(),
                EntryGroupId = path.GetGroupId(),
                EntryGuids = path.GetEntryGuids(),
                EntryId = path.GetEntryId(),
                Guid = guid,
                Id = guid.ToString(SampleDataInfos.GuidFormat),
                Name = entry.Name,
                Page = entry.Book.Page,
                Type = entry.Type.XmlName()
            };
            var selection = new Selection(xml);
            selection.OriginEntryPath.Target = entry;
            selection.ResetDefault(path);
            return selection;
        }

        private void RaisePointCostChanged(decimal oldValue, decimal newValue)
        {
            PointCostChanged?.Invoke(this, new PointCostChangedEventArgs());
        }

        /// <summary>
        ///     Cleans up the selection to it's default state. That means a state in which all
        ///     direct-parent-entry's limits visible from this selection (so only deeper) are taken into
        ///     account and best effort is done to apply them all. NOTE: Don't call before LinkUp.
        /// </summary>
        private void ResetDefault(CataloguePath path)
        {
            ResetProfiles(path);
            ResetRules(path);
            ResetNumberTaken();
            ResetSubSelections(path);
        }

        private void ResetNumberTaken()
        {
            NumberTaken = 1;
        }

        private void ResetProfiles(CataloguePath path)
        {
            var node = _profileMocksNode;
            var entry = OriginEntryPath.Target;
            var mocks = (from profile in entry.Profiles
                         select ProfileMock.CreateFrom(path.Select(profile)))
                .Concat(
                    from link in entry.ProfileLinks
                    select ProfileMock.CreateFrom(path.Select(link)))
                .OrderBy(x => x.Name);
            node.Clear();
            node.AddRange(mocks);
        }

        private void ResetRules(CataloguePath path)
        {
            var node = _ruleMocksNode;
            var entry = OriginEntryPath.Target;
            var mocks = (from rule in entry.Rules
                         select RuleMock.CreateFrom(path.Select(rule)))
                .Concat(
                    from link in entry.RuleLinks
                    select RuleMock.CreateFrom(path.Select(link)))
                .OrderBy(x => x.Name);
            node.Clear();
            node.AddRange(mocks);
        }

        private void ResetSubSelections(CataloguePath path)
        {
            var node = _selectionsNode;
            var originEntry = OriginEntryPath.Target;
            node.Clear();
            var selections =
                SelectionCreationInfo.CreateChildInfosFrom(originEntry, path)
                    .SelectMany(creationInfo => creationInfo.CreateSelections());
            node.AddRange(selections);
        }

        private void SetPointCost(decimal newValue)
        {
            var oldValue = PointCost;
            if (oldValue == newValue)
            {
                return;
            }
            XmlBackend.Points = newValue;
            RaisePropertyChanged(nameof(PointCost));
            RaisePointCostChanged(oldValue, newValue);
        }

        private class SelectionCreationInfo
        {
            private SelectionCreationInfo(CataloguePath cataloguePath, IEntry entry, int minLimit)
            {
                CataloguePath = cataloguePath;
                Entry = entry;
                MinLimit = minLimit;
            }

            private CataloguePath CataloguePath { get; }

            private IEntry Entry { get; }

            private int MinLimit { get; }

            public IEnumerable<ISelection> CreateSelections()
            {
                if (!Entry.IsAllContentCollective())
                {
                    return Enumerable.Repeat(this, MinLimit).Select(x => CreateFrom(x.CataloguePath));
                }
                var selection = CreateFrom(CataloguePath);
                selection.NumberTaken = (uint) MinLimit;
                return new[] {selection};
            }

            public static IEnumerable<SelectionCreationInfo> CreateChildInfosFrom(IEntryBase parentEntry,
                CataloguePath parentPath)
            {
                return (from entryPair in parentEntry.GetEntryLinkPairs()
                        let entry = entryPair.Entry
                        let min = entry.Limits.SelectionsLimit.Min
                        where min > 0
                        select new SelectionCreationInfo(parentPath.SelectAuto(entryPair), entry, min))
                    .Concat(
                        from groupPair in parentEntry.GetGroupLinkPairs()
                        let @group = groupPair.Group
                        let groupDefault = @group.DefaultChoice
                        where groupDefault != null
                        let groupMin = @group.Limits.SelectionsLimit.Min
                        where groupMin > 0
                        let selectionMin = groupDefault.Limits.SelectionsLimit.Min
                        let min = selectionMin > groupMin ? selectionMin : groupMin
                        let path =
                            parentPath.SelectAuto(groupPair)
                                .SelectAuto(groupDefault,
                                    @group.EntryLinks.FirstOrDefault(link => link.Target == groupDefault))
                        select new SelectionCreationInfo(path, groupDefault, min));
            }
        }
    }
}
