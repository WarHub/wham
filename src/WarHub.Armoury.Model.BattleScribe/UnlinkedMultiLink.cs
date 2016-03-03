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
