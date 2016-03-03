namespace WarHub.Armoury.Model.BattleScribe
{
    using ModelBases;
    using XmlICharacteristic = BattleScribeXml.ICharacteristic;

    public class Characteristic : XmlBackedModelBase<XmlICharacteristic>, ICharacteristic
    {
        private readonly Identifier _typeId;

        public Characteristic(XmlICharacteristic xml)
            : base(xml)
        {
            _typeId = new Identifier(XmlBackend);
        }

        public string Name
        {
            get { return XmlBackend.Name; }
            set { Set(XmlBackend.Name, value, () => XmlBackend.Name = value); }
        }

        public IIdentifier TypeId => _typeId;

        public string Value
        {
            get { return XmlBackend.Value; }
            set { Set(XmlBackend.Value, value, () => XmlBackend.Value = value); }
        }

        public ICharacteristic Clone()
        {
            return new Characteristic(new BattleScribeXml.Characteristic(XmlBackend));
        }
    }
}
