namespace WarHub.ArmouryModel.Source
{
    public interface IPublicationReferencingNode
    {
        string PublicationId { get; }
        string Page { get; }
    }

    partial class EntryBaseNode : IPublicationReferencingNode { }
    partial class RosterElementBaseNode : IPublicationReferencingNode { }
}
