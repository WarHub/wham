using System.IO;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.BattleScribe
{
    public class XmlDocument
    {
        private readonly IDatafileInfo _datafileInfo;

        public XmlDocument(XmlDocumentKind kind, IDatafileInfo datafileInfo, XmlWorkspace workspace)
        {
            Kind = kind;
            _datafileInfo = datafileInfo;
            Workspace = workspace;
        }

        /// <summary>
        /// Gets the filepath of this document.
        /// </summary>
        public string Filepath => _datafileInfo.Filepath;

        /// <summary>
        /// Gets the filename without file extension.
        /// </summary>
        public string Name => Path.GetFileNameWithoutExtension(Filepath);

        /// <summary>
        /// Gets the kind of this document.
        /// </summary>
        public XmlDocumentKind Kind { get; }

        /// <summary>
        /// Gets the parent workspace of this document.
        /// </summary>
        public XmlWorkspace Workspace { get; }

        /// <summary>
        /// Gets the root node of the document. May cause deserialization.
        /// </summary>
        /// <returns></returns>
        public SourceNode GetRoot()
        {
            return _datafileInfo.GetData();
        }
    }
}
