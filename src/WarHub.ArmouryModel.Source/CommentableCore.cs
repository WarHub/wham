using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial class CommentableCore
    {
        [XmlElement("comment")]
        public string Comment { get; }
    }
}
