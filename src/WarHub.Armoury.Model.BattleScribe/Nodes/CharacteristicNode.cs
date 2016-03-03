using XmlCharacteristicList = WarHub.Armoury.Model.BattleScribe.Nodes.CastingList
    <WarHub.Armoury.Model.BattleScribeXml.ICharacteristic,
        WarHub.Armoury.Model.BattleScribeXml.Characteristic>;
using XmlRosterCharacteristicList = WarHub.Armoury.Model.BattleScribe.Nodes.CastingList
    <WarHub.Armoury.Model.BattleScribeXml.ICharacteristic,
        WarHub.Armoury.Model.BattleScribeXml.RosterCharacteristic>;

namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using BattleScribeXml;
    using Characteristic = BattleScribe.Characteristic;
    using ICharacteristic = Model.ICharacteristic;

    internal class CharacteristicNode
        : XmlBackedNodeSimple<ICharacteristic, Characteristic, BattleScribeXml.ICharacteristic>
    {
        public CharacteristicNode(Func<List<BattleScribeXml.Characteristic>> listGet)
            : base(new XmlCharacteristicList(listGet), Transformation,
                () => Factory(XmlCharacteristicFactory))
        {
        }

        public CharacteristicNode(Func<List<RosterCharacteristic>> listGet)
            : base(new XmlRosterCharacteristicList(listGet), Transformation,
                () => Factory(XmlRosterCharacteristicFactory))
        {
        }

        private static Characteristic Factory(Func<BattleScribeXml.ICharacteristic> xmlFactory)
        {
            var characteristic = xmlFactory();
            return Transformation(characteristic);
        }

        private static Characteristic Transformation(BattleScribeXml.ICharacteristic arg)
        {
            return new Characteristic(arg);
        }

        private static BattleScribeXml.Characteristic XmlCharacteristicFactory()
        {
            return new BattleScribeXml.Characteristic();
        }

        private static RosterCharacteristic XmlRosterCharacteristicFactory()
        {
            return new RosterCharacteristic();
        }
    }
}
