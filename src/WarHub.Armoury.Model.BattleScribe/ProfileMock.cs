// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using System.Linq;
    using BattleScribeXml;
    using ModelBases;
    using Nodes;
    using ICharacteristic = Model.ICharacteristic;

    public class ProfileMock : NamedIndexedModelBase<BattleScribeXml.ProfileMock>, IProfileMock
    {
        private readonly CharacteristicNode _characteristics;
        private readonly LinkPath<IProfile> _originPath;
        private IForceContext _forceContext;

        public ProfileMock(BattleScribeXml.ProfileMock xml)
            : base(xml)
        {
            _originPath = new LinkPath<IProfile>(
                XmlBackend.Guids,
                newList => XmlBackend.Guids = newList,
                () => XmlBackend.Id);
            _characteristics = new CharacteristicNode(() => XmlBackend.Characteristics)
            {
                Controller = XmlBackend.Controller
            };
        }

        public INodeSimple<ICharacteristic> Characteristics => _characteristics;

        public IForceContext ForceContext
        {
            get { return _forceContext; }
            set
            {
                if (!Set(ref _forceContext, value))
                {
                    return;
                }
                ICatalogueContext catalogueContext = null;
                if (value != null)
                {
                    catalogueContext = value.SourceCatalogue.Context;
                    catalogueContext.Profiles.SetTargetOf(OriginProfilePath);
                }
                _originPath.SetCatalogueContext(catalogueContext);
            }
        }

        public bool IsHidden
        {
            get { return XmlBackend.Hidden; }
            set { Set(XmlBackend.Hidden, value, () => { XmlBackend.Hidden = value; }); }
        }

        public ILinkPath<IProfile> OriginProfilePath => _originPath;

        public static ProfileMock CreateFrom(CataloguePath path)
        {
            var profile = (IProfile) path.Last();
            var xml = new BattleScribeXml.ProfileMock
            {
                Book = profile.Book.Title,
                Guids = path.GetProfileMockGuids(),
                Hidden = profile.IsHidden,
                Id = path.GetProfileMockId(),
                Name = profile.Name,
                Page = profile.Book.Page,
                ProfileTypeName = profile.TypeLink.Target.Name
            };
            foreach (var xmlCharacteristic in profile.Characteristics.Select(characteristic => new RosterCharacteristic
            {
                Guid = characteristic.TypeId.Value,
                Id = characteristic.TypeId.RawValue,
                Name = characteristic.Name,
                Value = characteristic.Value
            }))
            {
                xml.Characteristics.Add(xmlCharacteristic);
            }
            return new ProfileMock(xml);
        }
    }
}
