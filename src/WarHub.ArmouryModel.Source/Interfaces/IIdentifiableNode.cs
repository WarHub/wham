namespace WarHub.ArmouryModel.Source
{
    public interface IIdentifiableNode
    {
        string? Id { get; }
    }

    public partial class CatalogueBaseNode : IIdentifiableNode { }
    public partial class CatalogueLinkNode : IIdentifiableNode { }
    public partial class CharacteristicTypeNode : IIdentifiableNode { }
    public partial class ConstraintNode : IIdentifiableNode { }
    public partial class CostTypeNode : IIdentifiableNode { }
    public partial class EntryBaseNode : IIdentifiableNode { }
    public partial class ProfileTypeNode : IIdentifiableNode { }
    public partial class PublicationNode : IIdentifiableNode { }
    public partial class RosterElementBaseNode : IIdentifiableNode { }
    public partial class RosterNode : IIdentifiableNode { }
}
