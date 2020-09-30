using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial record CommentableCore
    {
        [XmlElement("comment")]
        public string? Comment { get; init; }
    }
}
