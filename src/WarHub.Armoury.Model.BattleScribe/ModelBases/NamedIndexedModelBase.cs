namespace WarHub.Armoury.Model.BattleScribe.ModelBases
{
    using System.Diagnostics;
    using BattleScribeXml;

    [DebuggerDisplay("{Name}")]
    public class NamedIndexedModelBase<TXml> : XmlBackedModelBase<TXml>, INameable, IBookIndexable
        where TXml : INamed, IBookIndexed
    {
        private readonly BookIndex _book;

        public NamedIndexedModelBase(TXml xml)
            : base(xml)
        {
            _book = new BookIndex(xml);
        }

        public IBookIndex Book
        {
            get { return _book; }
        }

        public string Name
        {
            get { return XmlBackend.Name; }
            set { Set(XmlBackend.Name, value, () => XmlBackend.Name = value); }
        }
    }
}
