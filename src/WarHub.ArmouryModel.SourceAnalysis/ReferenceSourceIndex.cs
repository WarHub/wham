using System.Collections.Immutable;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.SourceAnalysis
{
    public class ReferenceSourceIndex : IReferenceSourceIndex
    {
        public ReferenceSourceIndex(
            ImmutableArray<QueryBaseNode> inQueryScope,
            ImmutableArray<QueryBaseNode> inQueryField,
            ImmutableArray<QueryFilteredBaseNode> inQueryChildId,
            ImmutableArray<SourceNode> inLinkTargetId,
            ImmutableArray<SourceNode> inValueTypeId,
            ImmutableArray<SourceNode> inPublicationId)
        {
            InQueryScope = inQueryScope;
            InQueryField = inQueryField;
            InQueryChildId = inQueryChildId;
            InLinkTargetId = inLinkTargetId;
            InValueTypeId = inValueTypeId;
            InPublicationId = inPublicationId;
        }

        public ImmutableArray<QueryBaseNode> InQueryScope { get; }
        public ImmutableArray<QueryBaseNode> InQueryField { get; }
        public ImmutableArray<QueryFilteredBaseNode> InQueryChildId { get; }
        public ImmutableArray<SourceNode> InLinkTargetId { get; }
        public ImmutableArray<SourceNode> InValueTypeId { get; }
        public ImmutableArray<SourceNode> InPublicationId { get; }
    }
}
