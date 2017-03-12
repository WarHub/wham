// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Source
{
    using System;
    using System.Collections.Immutable;

    public abstract class SourceTree
    {
        public abstract SourceNode GetRootNode();
    }

    public class CharacteristicNode : BattleScribeSourceNode
    {
        public CharacteristicNode(SourceTree tree, BattleScribeSourceNode parent, ImmutableSourceNode immutable) : base(tree)
        {
            Immutable = immutable;
        }
        
        public IdToken CharacteristicId { get; }

        public TextToken Value { get; }

        public TextToken Name { get; }

        public override SourceNode Parent => throw new NotImplementedException();

        public override SourceNodeKind NodeKind => throw new NotImplementedException();

        public override SourceNodeList<SourceNode> Children => throw new NotImplementedException();

        private ImmutableSourceNode Immutable { get; }

        internal override int GetSlotCount()
        {
            throw new NotImplementedException();
        }

        internal override SourceNode GetSlotNode(int index)
        {
            throw new NotImplementedException();
        }
    }

    public class ImmutableCharacteristicNode : ImmutableSourceNode
    {
        public override ImmutableArray<ImmutableSourceNode> Children => ImmutableArray<ImmutableSourceNode>.Empty;

        public override SourceNodeKind NodeKind => SourceNodeKind.Unspecified;

        public IdToken CharacteristicId { get; }

        public TextToken Value { get; }

        public TextToken Name { get; }
    }

    public class ImmutableCharacteristicTypeNode : ImmutableSourceNode
    {
        public override ImmutableArray<ImmutableSourceNode> Children => ImmutableArray<ImmutableSourceNode>.Empty;

        public override SourceNodeKind NodeKind => SourceNodeKind.Unspecified;

        public IdToken CharacteristicId { get; }

        public TextToken Name { get; }
    }

    public class IdToken
    {

    }

    public class IntegerToken
    {

    }

    public class TextToken
    {

    }
    
    public abstract class BattleScribeSourceNode : SourceNode
    {
        public BattleScribeSourceNode(SourceTree tree) : base(tree)
        {
        }

        public override string ModelLanguage => "BattleScribe";
    }

    public abstract class ImmutableSourceNode
    {
        public abstract ImmutableArray<ImmutableSourceNode> Children { get; }

        public abstract SourceNodeKind NodeKind { get; }
    }
}
