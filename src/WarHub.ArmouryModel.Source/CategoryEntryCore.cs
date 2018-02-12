using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("categoryEntry")]
    public partial class CategoryEntryCore : EntryBaseCore
    {
        [XmlArray("constraints")]
        public ImmutableArray<ConstraintCore> Constraints { get; }
    }
}
