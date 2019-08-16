namespace WarHub.ArmouryModel.Source
{
    public interface IIdentifiableNode
    {
        string Id { get; }
    }

    partial class CatalogueBaseNode : IIdentifiableNode { }
    partial class CatalogueLinkNode : IIdentifiableNode { }
    partial class CharacteristicTypeNode : IIdentifiableNode { }
    partial class ConstraintNode : IIdentifiableNode { }
    partial class CostTypeNode : IIdentifiableNode { }
    partial class EntryBaseNode : IIdentifiableNode { }
    partial class ProfileTypeNode : IIdentifiableNode { }
    partial class PublicationNode : IIdentifiableNode { }
    partial class RosterElementBaseNode : IIdentifiableNode { }
    partial class RosterNode : IIdentifiableNode { }
}
