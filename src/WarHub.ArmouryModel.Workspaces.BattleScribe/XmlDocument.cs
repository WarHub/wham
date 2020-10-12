using System.IO;
using System.Threading.Tasks;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.BattleScribe
{
    public record XmlDocument(
        /// <summary>
        /// Gets the kind of this document.
        /// </summary>
        IDatafileInfo DatafileInfo,
        /// <summary>
        /// Gets the underlying datafile info.
        /// </summary>
        XmlDocumentKind Kind,
        /// <summary>
        /// Gets the parent workspace of this document.
        /// </summary>
        XmlWorkspace? Workspace = null)
    {
        /// <summary>
        /// Gets the filepath of this document.
        /// </summary>
        public string Filepath => DatafileInfo.Filepath;

        /// <summary>
        /// Gets the filename without file extension.
        /// </summary>
        public string Name => Path.GetFileNameWithoutExtension(Filepath);

        /// <summary>
        /// Gets the root node of the document. May cause deserialization.
        /// </summary>
        /// <returns></returns>
        public Task<SourceNode?> GetRootAsync() => DatafileInfo.GetDataAsync();

        public static XmlDocument Create(IDatafileInfo datafileInfo, XmlWorkspace? workspace = null)
        {
            return new XmlDocument(datafileInfo, datafileInfo.Filepath.GetXmlDocumentKind(), workspace);
        }
    }
}
