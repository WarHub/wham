using System;
using System.Collections;
using System.Collections.Generic;

namespace WarHub.ArmouryModel.Source
{
    public struct NamedNodeOrList : IReadOnlyCollection<SourceNode>
    {
        public NamedNodeOrList(string name, SourceNode singleChild)
        {
            Name = name;
            SingleChild = singleChild;
            List = default;
        }
        public NamedNodeOrList(string name, NodeList<SourceNode> list)
        {
            Name = name;
            SingleChild = default;
            List = list;
        }

        public bool IsSingle => SingleChild != null;
        public bool IsList => SingleChild == null;
        public int Count => IsSingle ? 1 : List.Count;
        public SourceNode this[int index]
        {
            get
            {
                return
                    IsList ? List[index] :
                    index == 0 ? SingleChild :
                    throw new IndexOutOfRangeException(
                        "This is a single element union, tried to access index " + index);
            }
        }

        public string Name { get; }
        public SourceNode SingleChild { get; }
        public NodeList<SourceNode> List { get; }

        public IEnumerator<SourceNode> GetEnumerator()
        {
            var self = this;
            return self.IsSingle ? SingleEnumerator() : self.List.GetEnumerator();
            IEnumerator<SourceNode> SingleEnumerator()
            {
                yield return self.SingleChild;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
