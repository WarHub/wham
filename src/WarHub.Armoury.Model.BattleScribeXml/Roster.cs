// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using GuidMapping;

    [XmlRoot("roster", Namespace = BsRosterXmlNamespace)]
    public sealed class Roster : IdentifiedGuidControllableBase,
        IXmlProperties, INamed
    {
        public const string BsRosterXmlNamespace =
            "http://www.battlescribe.net/schema/rosterSchema";

        private Guid _gameSystemGuid;

        public Roster()
        {
            Forces = new List<Force>(0);
            Rules = new List<RuleMock>(0);
        }

        public string DefaultXmlNamespace => BsRosterXmlNamespace;

        [XmlAttribute("id")]
        public override string Id { get; set; }

        [XmlAttribute("battleScribeVersion")]
        public string BattleScribeVersion { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("points")]
        public decimal Points { get; set; }

        [XmlAttribute("pointsLimit")]
        public decimal PointsLimit { get; set; }

        [XmlAttribute("gameSystemId")]
        public string GameSystemId { get; set; }

        [XmlIgnore]
        public Guid GameSystemGuid
        {
            get { return _gameSystemGuid; }
            set { TrySetAndRaise(ref _gameSystemGuid, value, newId => GameSystemId = newId); }
        }

        [XmlAttribute("gameSystemName")]
        public string GameSystemName { get; set; }

        [XmlAttribute("gameSystemRevision")]
        public uint GameSystemRevision { get; set; }

        [XmlArray("forces")]
        public List<Force> Forces { get; set; }

        [XmlArray("rules")]
        public List<RuleMock> Rules { get; set; }

        public override void Process(GuidController controller)
        {
            base.Process(controller);
            GameSystemGuid = controller.ParseId(GameSystemId);
            controller.Process(Forces);
        }
    }
}
