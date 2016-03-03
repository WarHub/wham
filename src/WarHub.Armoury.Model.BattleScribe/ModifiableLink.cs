namespace WarHub.Armoury.Model.BattleScribe
{
    using System.ComponentModel;

    public abstract class ModifiableLink<TTarget, TModifier>
        : IdLink<TTarget>, IModifiableLink<TTarget, TModifier>,
            IXmlBackedObject<BattleScribeXml.Link>, ICatalogueItem
        where TTarget : class, IIdentifiable
        where TModifier : INotifyPropertyChanged
    {
        protected ModifiableLink(BattleScribeXml.Link xml)
            : base(xml.TargetGuid,
                newGuid => xml.TargetGuid = newGuid,
                () => xml.TargetId)
        {
            XmlBackend = xml;
            Id = new Identifier(XmlBackend);
        }

        public abstract ICatalogueContext Context { get; set; }

        public IIdentifier Id { get; }

        public abstract INodeSimple<TModifier> Modifiers { get; }

        public BattleScribeXml.Link XmlBackend { get; }
    }
}
