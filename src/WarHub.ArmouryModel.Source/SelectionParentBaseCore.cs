using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial record SelectionParentBaseCore : RosterElementBaseCore
    {
        [XmlArray("selections")]
        public ImmutableArray<SelectionCore> Selections { get; init; } = ImmutableArray<SelectionCore>.Empty;
    }
}
