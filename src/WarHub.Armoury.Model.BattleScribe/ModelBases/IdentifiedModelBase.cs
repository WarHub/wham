namespace WarHub.Armoury.Model.BattleScribe.ModelBases
{
    using BattleScribeXml;

    /// <summary>
    ///     Takes care of initializing Guid property and assigns the XML object ref to protected field.
    /// </summary>
    /// <typeparam name="TXml">Type of XML object held in field.</typeparam>
    public class IdentifiedModelBase<TXml> : XmlBackedModelBase<TXml>, IIdentifiable
        where TXml : IIdentified
    {
        protected IdentifiedModelBase(TXml xml)
            : base(xml)
        {
            Id = new Identifier(xml);
        }

        public IIdentifier Id { get; }
    }
}
