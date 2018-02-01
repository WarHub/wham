using System;

namespace WarHub.ArmouryModel.Source
{
    public interface ICore<TNode> where TNode : SourceNode
    {
        TNode ToNode(SourceNode parent = null);

        // TODO uncomment after generation implemented

        //Type GetSerializationProxyType();

        //Type GetBuilderType();

        //Type GetSerializationEnumerableType();
    }
}
