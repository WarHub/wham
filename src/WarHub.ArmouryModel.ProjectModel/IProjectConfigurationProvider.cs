namespace WarHub.ArmouryModel.ProjectModel
{
    public interface IProjectConfigurationProvider
    {
        ProjectConfigurationInfo Create(string path);

        ProjectConfigurationInfo Empty { get; }
    }
}
