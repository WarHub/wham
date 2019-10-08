using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    public static class GitreeExtensions
    {
        public static GitreeNode ConvertToGitree(this SourceNode node)
        {
            var converter = new SourceNodeToGitreeConverter();
            var gitreeNode = converter.Visit(node);
            return gitreeNode;
        }
    }
}
