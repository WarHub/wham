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
