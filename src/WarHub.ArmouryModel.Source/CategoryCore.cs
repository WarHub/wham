using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("category")]
    public partial class CategoryCore : RosterElementBaseCore
    {
        [XmlArray("selections", Order = 0)]
        public ImmutableArray<SelectionCore> Selections { get; }
    }
}
