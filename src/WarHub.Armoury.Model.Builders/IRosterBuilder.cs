namespace WarHub.Armoury.Model.Builders
{
    public interface IRosterBuilder : IBuilderCore, IForceBuilderNode
    {
        IRoster Roster { get; }
    }
}