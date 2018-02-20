using System;
using System.Collections.Generic;
using System.Text;

namespace WarHub.ArmouryModel.Source
{
    public interface IIdentifiableNode
    {
        string Id { get; }
    }

    partial class CatalogueBaseNode : IIdentifiableNode { }
    partial class RosterElementBaseNode : IIdentifiableNode { }
    partial class EntryBaseNode : IIdentifiableNode { }
    partial class CharacteristicTypeNode : IIdentifiableNode { }
    partial class CostTypeNode : IIdentifiableNode { }
    partial class ConstraintNode : IIdentifiableNode { }
    partial class ProfileTypeNode : IIdentifiableNode { }
    partial class RosterNode : IIdentifiableNode { }
}
