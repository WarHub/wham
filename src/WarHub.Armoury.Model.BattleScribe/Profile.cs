// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using ModelBases;
    using Nodes;

    public class Profile : IdentifiedNamedIndexedModelBase<BattleScribeXml.Profile>, IProfile
    {
        private readonly CharacteristicNode _characteristics;
        private readonly ProfileModifierNode _modifiers;
        private readonly IdLink<IProfileType> _typeLink;
        private ICatalogueContext _context;

        public Profile(BattleScribeXml.Profile xml)
            : base(xml)
        {
            _characteristics = new CharacteristicNode(() => XmlBackend.Characteristics)
            {
                Controller = XmlBackend.Controller
            };
            _modifiers = new ProfileModifierNode(() => XmlBackend.Modifiers, this) {Controller = XmlBackend.Controller};
            _typeLink = new IdLink<IProfileType>(
                XmlBackend.ProfileTypeGuid,
                x => XmlBackend.ProfileTypeGuid = x,
                () => XmlBackend.ProfileTypeId);
        }

        public INodeSimple<ICharacteristic> Characteristics
        {
            get { return _characteristics; }
        }

        public ICatalogueContext Context
        {
            get { return _context; }
            set
            {
                var old = _context;
                if (Set(ref _context, value))
                {
                    if (old != null)
                    {
                        old.Profiles.Deregister(this);
                    }
                    TypeLink.Target = null;
                    if (value != null)
                    {
                        value.Profiles.Register(this);
                        value.Catalogue.SystemContext.ProfileTypes.SetTargetOf(TypeLink);
                    }
                    Modifiers.ChangeContext(value);
                }
            }
        }

        public bool IsHidden
        {
            get { return XmlBackend.Hidden; }
            set { Set(XmlBackend.Hidden, value, () => { XmlBackend.Hidden = value; }); }
        }

        public INodeSimple<IProfileModifier> Modifiers
        {
            get { return _modifiers; }
        }

        public IIdLink<IProfileType> TypeLink
        {
            get { return _typeLink; }
        }

        public IProfile Clone()
        {
            return new Profile(new BattleScribeXml.Profile(XmlBackend));
        }
    }
}
