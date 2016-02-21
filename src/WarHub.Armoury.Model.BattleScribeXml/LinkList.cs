// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;

    public class LinkList : ICollection<Link>, IList<Link>
    {
        public LinkList()
        {
        }

        /// <summary>
        ///     Creates deep copy of <paramref name="other" /> .
        /// </summary>
        /// <param name="other">List to be copied.</param>
        public LinkList(LinkList other)
        {
            EntryLinks = other.EntryLinks.Select(x => new Link(x)).ToList();
            EntryGroupLinks = other.EntryGroupLinks.Select(x => new Link(x)).ToList();
            ProfileLinks = other.ProfileLinks.Select(x => new Link(x)).ToList();
            RuleLinks = other.RuleLinks.Select(x => new Link(x)).ToList();
        }

        public int Count
        {
            get
            {
                return EntryLinks.Count
                       + EntryGroupLinks.Count
                       + ProfileLinks.Count
                       + RuleLinks.Count;
            }
        }

        [XmlIgnore]
        public List<Link> EntryGroupLinks { get; } = new List<Link>();

        [XmlIgnore]
        public List<Link> EntryLinks { get; } = new List<Link>();

        /// <summary>
        ///     This implementation returns false, it's never read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        [XmlIgnore]
        public List<Link> ProfileLinks { get; } = new List<Link>();

        [XmlIgnore]
        public List<Link> RuleLinks { get; } = new List<Link>();

        private IEnumerable<Link> AllLinks
        {
            get
            {
                return EntryLinks
                    .Concat(EntryGroupLinks)
                    .Concat(ProfileLinks)
                    .Concat(RuleLinks)
                    .ToList();
            }
        }

        public void Add(Link item)
        {
            ListFor(item).Add(item);
        }

        public void Clear()
        {
            EntryGroupLinks.Clear();
            EntryLinks.Clear();
            ProfileLinks.Clear();
            RuleLinks.Clear();
        }

        public bool Contains(Link item)
        {
            return ListFor(item).Contains(item);
        }

        public void CopyTo(Link[] array, int arrayIndex)
        {
            AllLinks.ToList().CopyTo(array, arrayIndex);
        }

        public IEnumerator<Link> GetEnumerator()
        {
            return AllLinks.GetEnumerator();
        }

        public bool Remove(Link item)
        {
            return ListFor(item).Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return AllLinks.GetEnumerator();
        }

        private List<Link> ListFor(Link item)
        {
            switch (item.LinkType)
            {
                case LinkType.Entry:
                    return EntryLinks;

                case LinkType.EntryGroup:
                    return EntryGroupLinks;

                case LinkType.Profile:
                    return ProfileLinks;

                case LinkType.Rule:
                    return RuleLinks;

                default:
                    throw new ArgumentException("Value of LinkType enum not in correct range!");
            }
        }

        public int IndexOf(Link item)
        {
            return GetIndex(item);
        }

        public void Insert(int index, Link item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public Link this[int index]
        {
            get
            {
                var counter = 0;
                foreach (var item in AllLinks)
                {
                    if (counter++ == index)
                    {
                        return item;
                    }
                }
                throw new ArgumentOutOfRangeException("Index out of range!");
            }
            set { throw new NotImplementedException(); }
        }

        private int GetIndex(Link item)
        {
            var index = 0;
            foreach (var link in AllLinks)
            {
                if (link.Equals(item))
                {
                    return index;
                }
                ++index;
            }
            throw new ArgumentOutOfRangeException("Item not in collection!");
        }
    }
}
