// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using ModelBases;
    using Nodes;

    public class ProfileType : IdentifiedNamedModelBase<BattleScribeXml.ProfileType>, IProfileType
    {
        private readonly CharacteristicTypeNode _characteristicTypes;
        private IGameSystemContext _context;

        public ProfileType(BattleScribeXml.ProfileType xml)
            : base(xml)
        {
            _characteristicTypes = new CharacteristicTypeNode(() => XmlBackend.Characteristics)
            {
                Controller = XmlBackend.Controller
            };
        }

        public INodeSimple<ICharacteristicType> CharacteristicTypes
        {
            get { return _characteristicTypes; }
        }

        public IGameSystemContext Context
        {
            get { return _context; }
            set
            {
                var old = _context;
                if (Set(ref _context, value))
                {
                    old?.ProfileTypes.Deregister(this);
                    value?.ProfileTypes.Register(this);
                }
            }
        }
    }
}
