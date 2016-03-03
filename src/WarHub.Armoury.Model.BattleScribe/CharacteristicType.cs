namespace WarHub.Armoury.Model.BattleScribe
{
    using ModelBases;

    public class CharacteristicType : IdentifiedNamedModelBase<BattleScribeXml.CharacteristicType>,
        ICharacteristicType
    {
        public CharacteristicType(BattleScribeXml.CharacteristicType xml)
            : base(xml)
        {
        }
    }
}
