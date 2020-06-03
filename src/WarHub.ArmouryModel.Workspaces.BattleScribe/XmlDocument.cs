using System.IO;
using System.Threading.Tasks;
using Amadevus.RecordGenerator;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.BattleScribe
{
    [Record]
    public partial class XmlDocument
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
        /// Gets the kind of this document.
        /// </summary>
        public XmlDocumentKind Kind { get; }

        /// <summary>
        /// Gets the underlying datafile info.
        /// </summary>
        public IDatafileInfo DatafileInfo { get; }

        /// <summary>
        /// Gets the parent workspace of this document.
        /// </summary>
        public XmlWorkspace? Workspace { get; }

        /// <summary>
        /// Gets the root node of the document. May cause deserialization.
        /// </summary>
        /// <returns></returns>
        public Task<SourceNode?> GetRootAsync() => DatafileInfo.GetDataAsync();

        public static XmlDocument Create(IDatafileInfo datafileInfo, XmlWorkspace? workspace = null)
        {
            return new XmlDocument(datafileInfo.Filepath.GetXmlDocumentKind(), datafileInfo, workspace);
        }
    }
}
