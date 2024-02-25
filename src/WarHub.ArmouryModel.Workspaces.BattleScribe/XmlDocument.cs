using System.Threading;
using System.Threading.Tasks;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.BattleScribe
{
    public record XmlDocument(
#pragma warning disable CS1587 // XML comment is not placed on a valid language element
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
#pragma warning restore CS1587
    {
        /// <summary>
        /// Gets the filepath of this document.
        /// </summary>
        public string Filepath => DatafileInfo.Filepath;

        /// <summary>
        /// Gets the filename without file extension.
        /// </summary>
        public string Name => DatafileInfo.GetStorageName();

        /// <summary>
        /// Gets the root node of the document. May cause deserialization.
        /// </summary>
        /// <returns></returns>
        public Task<SourceNode> GetRootAsync(CancellationToken cancellationToken = default) =>
            DatafileInfo.GetDataAsync(cancellationToken);

        public static XmlDocument Create(IDatafileInfo datafileInfo, XmlWorkspace? workspace = null)
        {
            return new XmlDocument(datafileInfo, datafileInfo.DataKind.GetXmlDocumentKindOrUnknown(), workspace);
        }
    }
}
