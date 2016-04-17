namespace WarHub.Armoury.Model.Builders
{
    public interface ICharacteristicBuilder
    {
        ICharacteristicType CharacteristicType { get; }
        ICharacteristic Characteristic { get; }
        string Value { get; set; }
        void Reset();
    }
}
