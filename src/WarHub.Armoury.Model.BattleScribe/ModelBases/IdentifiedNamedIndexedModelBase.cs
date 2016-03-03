namespace WarHub.Armoury.Model.BattleScribe.ModelBases
{
    using BattleScribeXml;

    public class IdentifiedNamedIndexedModelBase<TXml>
        : NamedIndexedModelBase<TXml>, IIdentifiable
        where TXml : IIdentified, INamed, IBookIndexed
    {
        public IdentifiedNamedIndexedModelBase(TXml xml)
            : base(xml)
        {
            Id = new Identifier(xml);
        }

        public IIdentifier Id { get; }
    }
}
