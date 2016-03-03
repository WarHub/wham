namespace WarHub.Armoury.Model.BattleScribe.ModelBases
{
    public class XmlBackedModelBase<TXml> : ModelBase, IXmlBackedObject<TXml>
    {
        public XmlBackedModelBase(TXml xmlBackend)
        {
            XmlBackend = xmlBackend;
        }

        public TXml XmlBackend { get; }
    }
}
