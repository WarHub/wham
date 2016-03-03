namespace WarHub.Armoury.Model.BattleScribe
{
    public interface IXmlBackedObject<out TXml>
    {
        TXml XmlBackend { get; }
    }
}
