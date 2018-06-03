using System.Collections.Immutable;

namespace WarHub.ArmouryModel.Source
{
    internal interface INodeListWithCoreArray<TNode, TCore>
        where TNode : SourceNode, INodeWithCore<TCore>
        where TCore : ICore<TNode>
    {
        ImmutableArray<TCore> Cores { get; }
    }
}
