// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Source
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    public abstract class SourceTree
    {
        public abstract SourceNode GetRootNode();
    }

    public class CharacteristicxNode : BattleScribeSourceNode
    {
        public CharacteristicxNode(SourceTree tree, BattleScribeSourceNode parent, ImmutableSourceNode immutable) : base(tree)
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

    public static class SourceFactory
    {
        public static DatablobNode GameSystem()
        {
            return new DatablobNode();
        }
    }

    public abstract class ImmutableSourceNode
    {
        public abstract ImmutableArray<ImmutableSourceNode> Children { get; }

        public abstract SourceNodeKind NodeKind { get; }
    }

    ///////////

    abstract class Node
    {

    }

    interface IContainer<TItem> where TItem : Node
    {
        int SlotCount { get; }
        TItem GetNodeSlot(int index);
    }

    struct NodeList<TItem> : IReadOnlyList<TItem> where TItem : Node 
    {
        private IContainer<TItem> Container { get; }

        public TItem this[int index] => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public IEnumerator<TItem> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    class CharacteristicNode : Node
    {
    }


    class CoreNode { }

    class CoreCharacteristic : CoreNode
    {

    }

    class CoreProfile : CoreNode
    {
        public ImmutableArray<CoreCharacteristic> Characteristics { get; }
    }

    class ProfileNode : Node, IContainer<CharacteristicNode>
    {
        private ArrayElement<CharacteristicNode>[] _characteristicNodes;

        private CoreProfile Core { get; }

        int IContainer<CharacteristicNode>.SlotCount => Core.Characteristics.Length;

        CharacteristicNode IContainer<CharacteristicNode>.GetNodeSlot(int index)
        {
            return GetRed(ref _characteristicNodes[index].Value, index);
        }

        T GetRed<T>(ref T field, int slot)
        {
            return default(T);
        }

        NodeList<CharacteristicNode> Characteristics { get; }
    }

    struct Delta<T>
    {
        public Delta(T value)
        {
            Value = value;
            Changed = true;
        }

        public T Value { get; }

        public bool Changed { get; }

        public static implicit operator Delta<T>(T value)
        {
            return new Delta<T>(value);
        }

        public Delta<T> MakeDelta(T originalValue, T newValue)
        {
            if (EqualityComparer<T>.Default.Equals(originalValue, newValue))
            {
                return new Delta<T>();
            }
            return new Delta<T>(newValue);
        }
    }

    class XCharacteristic
    {
        public XCharacteristic(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }

        public string Value { get; }

        class XCharacteristicBuilder
        {
            private readonly XCharacteristic _original;

            private Delta<string> _name;
            private Delta<string> _value;

            public XCharacteristicBuilder(XCharacteristic original)
            {
                _original = original;
            }

            public string Name
            {
                get => _name.Changed ? _name.Value : _original.Name;
                set => _name = _name.MakeDelta(_original.Name, value);
            }

            public string Value
            {
                get => _value.Changed ? _value.Value : _original.Value;
                set => _value = _value.MakeDelta(_original.Value, value);
            }

            public XCharacteristic ToImmutable()
            {
                return new XCharacteristic(Name, Value);
            }
        }
    }

    class XProfile
    {
        public ImmutableArray<XCharacteristic> Characteristics { get; }
    }
}
