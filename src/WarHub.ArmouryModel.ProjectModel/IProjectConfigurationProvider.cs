namespace WarHub.ArmouryModel.ProjectModel
{
    public interface IProjectConfigurationProvider
    {
        ProjectConfiguration Create(string path);
    }
}