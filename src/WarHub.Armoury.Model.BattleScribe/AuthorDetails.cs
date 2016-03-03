namespace WarHub.Armoury.Model.BattleScribe
{
    using BattleScribeXml;
    using ModelBases;

    public class AuthorDetails : XmlBackedModelBase<IAuthorable>, IAuthorDetails
    {
        public AuthorDetails(IAuthorable source)
            : base(source)
        {
        }

        public string Contact
        {
            get { return XmlBackend.AuthorContact; }
            set { Set(XmlBackend.AuthorContact, value, () => XmlBackend.AuthorContact = value); }
        }

        public string Name
        {
            get { return XmlBackend.AuthorName; }
            set { Set(XmlBackend.AuthorName, value, () => XmlBackend.AuthorName = value); }
        }

        public string Website
        {
            get { return XmlBackend.AuthorUrl; }
            set { Set(XmlBackend.AuthorUrl, value, () => XmlBackend.AuthorUrl = value); }
        }
    }
}
