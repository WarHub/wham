// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    public class GroupMultiLink : IdLink<IGroupLink>, IGroupMultiLink
    {
        public GroupMultiLink(IGroupLink link)
            : base(link)
        {
            Target = link;
        }

        public void Visit(IMultiLinkVisitor visitor)
        {
            visitor.Accept(this);
        }
    }
}
