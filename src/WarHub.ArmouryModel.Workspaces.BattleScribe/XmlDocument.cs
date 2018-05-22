using System.IO;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.BattleScribe
{
    public class XmlDocument
    {
        private readonly IDatafileInfo _datafileInfo;

        public XmlDocument(XmlDocumentKind key, IDatafileInfo datafileInfo, XmlWorkspace workspace)
        {
            Kind = key;
            _datafileInfo = datafileInfo;
            Workspace = workspace;
            Filepath = datafileInfo.Filepath;
            Name = Path.GetExtension(Filepath);
        }

        /// <summary>
        /// Gets the filepath of this document.
        /// </summary>
        public string Filepath { get; }

        /// <summary>
        /// Gets the filename without file extension.
        /// </summary>
        public string Name { get; }

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
            return _datafileInfo.Data;
        }
    }
}
