namespace WarHub.ArmouryModel.ProjectSystem
{
    public interface IProjectConfigurationProvider
    {
        ProjectConfiguration Create(string path);
    }
}