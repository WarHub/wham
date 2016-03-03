namespace WarHub.Armoury.Model.BattleScribe
{
    public class GameSystemContext : IGameSystemContext
    {
        public GameSystemContext(IGameSystem system)
        {
            GameSystem = system;
            Categories.Register(new NoCategory());
        }

        public IRegistry<ICatalogue> Catalogues { get; } = new Registry<ICatalogue>();

        public IRegistry<ICategory> Categories { get; } = new Registry<ICategory>();

        public IRegistry<IForceType> ForceTypes { get; } = new Registry<IForceType>();

        public IGameSystem GameSystem { get; }

        public IRegistry<IProfileType> ProfileTypes { get; } = new Registry<IProfileType>();
    }
}
