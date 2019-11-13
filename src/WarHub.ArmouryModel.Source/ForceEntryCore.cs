using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("forceEntry")]
    public sealed partial class ForceEntryCore : ContainerEntryBaseCore
    {
        [XmlArray("forceEntries")]
        public ImmutableArray<ForceEntryCore> ForceEntries { get; }

        [XmlArray("categoryLinks")]
        public ImmutableArray<CategoryLinkCore> CategoryLinks { get; }
    }
}
