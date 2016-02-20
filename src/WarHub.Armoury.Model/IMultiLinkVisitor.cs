// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System;

    public interface IMultiLinkVisitor
    {
        void Accept(IEntryMultiLink link);
        void Accept(IGroupMultiLink link);
        void Accept(IProfileMultiLink link);
        void Accept(IRuleMultiLink link);
        void Accept(IUnlinkedMultiLink link);
    }

    public class MultiLinkRelayVisitor : IMultiLinkVisitor
    {
        private readonly Action<IEntryMultiLink> _entryLinkConsumer;
        private readonly Action<IGroupMultiLink> _groupLinkConsumer;
        private readonly Action<IProfileMultiLink> _profileLinkConsumer;
        private readonly Action<IRuleMultiLink> _ruleLinkConsumer;
        private readonly Action<IUnlinkedMultiLink> _unlinkedLinkConsumer;

        public MultiLinkRelayVisitor(
            Action<IEntryMultiLink> entryLinkConsumer,
            Action<IGroupMultiLink> groupLinkConsumer,
            Action<IProfileMultiLink> profileLinkConsumer,
            Action<IRuleMultiLink> ruleLinkConsumer,
            Action<IUnlinkedMultiLink> unlinkedLinkConsumer)
        {
            _entryLinkConsumer = entryLinkConsumer;
            _groupLinkConsumer = groupLinkConsumer;
            _profileLinkConsumer = profileLinkConsumer;
            _ruleLinkConsumer = ruleLinkConsumer;
            _unlinkedLinkConsumer = unlinkedLinkConsumer;
        }

        public void Accept(IEntryMultiLink link)
        {
            _entryLinkConsumer(link);
        }

        public void Accept(IGroupMultiLink link)
        {
            _groupLinkConsumer(link);
        }

        public void Accept(IProfileMultiLink link)
        {
            _profileLinkConsumer(link);
        }

        public void Accept(IRuleMultiLink link)
        {
            _ruleLinkConsumer(link);
        }

        public void Accept(IUnlinkedMultiLink link)
        {
            _unlinkedLinkConsumer(link);
        }
    }
}
