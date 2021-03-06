﻿namespace WarHub.ArmouryModel.Source
{
    public interface IPublicationReferencingNode
    {
        string? PublicationId { get; }
        string? Page { get; }
    }

    public partial class EntryBaseNode : IPublicationReferencingNode { }
    public partial class RosterElementBaseNode : IPublicationReferencingNode { }
}
