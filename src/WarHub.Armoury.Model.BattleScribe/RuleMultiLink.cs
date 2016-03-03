namespace WarHub.Armoury.Model.BattleScribe
{
    public class RuleMultiLink : IdLink<IRuleLink>, IRuleMultiLink
    {
        public RuleMultiLink(IRuleLink link)
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
