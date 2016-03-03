namespace WarHub.Armoury.Model.BattleScribe
{
    public class ProfileMultiLink : IdLink<IProfileLink>, IProfileMultiLink
    {
        public ProfileMultiLink(IProfileLink link)
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
