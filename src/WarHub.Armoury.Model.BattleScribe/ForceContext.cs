namespace WarHub.Armoury.Model.BattleScribe
{
    public class ForceContext : IForceContext
    {
        private readonly SelectionRegistry _selections = new SelectionRegistry();

        public ForceContext(IForce force)
        {
            Force = force;
        }

        public IForce Force { get; }

        public IRoster Roster => Force.Context.Roster;

        public ISelectionRegistry Selections => _selections;

        public ICatalogue SourceCatalogue => Force.CatalogueLink.Target;
    }
}
