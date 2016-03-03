// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using System;

    public class UnlinkedMultiLink : IdLink<IIdentifiable>, IUnlinkedMultiLink
    {
        public UnlinkedMultiLink(Guid guid, string raw)
            : base(guid, _ => { }, () => raw)
        {
        }

        public void Visit(IMultiLinkVisitor visitor)
        {
            visitor.Accept(this);
        }
    }
}
