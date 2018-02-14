using System;
using System.IO;
using System.IO.Compression;
using WarHub.ArmouryModel.Source;
using WarHub.ArmouryModel.Source.BattleScribe;

namespace WarHub.ArmouryModel.Workspaces.BattleScribe
{
    public class XmlDocument
    {
        private readonly FileInfo file;

        public XmlDocument(XmlDocumentKind key, FileInfo file, XmlWorkspace workspace)
        {
            Kind = key;
            this.file = file;
            Workspace = workspace;
            Path = file.FullName;
            Name = file.Name;
        }

        /// <summary>
        /// Gets the filepath of this document.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the filename without file extension.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the kind of this document.
        /// </summary>
        public XmlDocumentKind Kind { get; }
        public XmlWorkspace Workspace { get; }
        private WeakReference<SourceNode> WeakRoot { get; } = new WeakReference<SourceNode>(null);

        /// <summary>
        /// Gets the root node of the document. May cause deserialization.
        /// </summary>
        /// <returns></returns>
        public SourceNode GetRoot()
        {
            return GetRootCore();
        }

        private SourceNode GetRootCore()
        {
            if (WeakRoot.TryGetTarget(out var root))
            {
                return root;
            }
            root = LoadRoot();
            WeakRoot.SetTarget(root);
            return root;
        }

        private SourceNode LoadRoot()
        {
            switch (Kind)
            {
                case XmlDocumentKind.Gamesystem:
                    return FromUnzippedStream(BattleScribeXml.LoadGamesystem);
                case XmlDocumentKind.Catalogue:
                    return FromUnzippedStream(BattleScribeXml.LoadCatalogue);
                case XmlDocumentKind.Roster:
                    return FromUnzippedStream(BattleScribeXml.LoadRoster);
                case XmlDocumentKind.DataIndex:
                    return FromUnzippedStream(BattleScribeXml.LoadDataIndex);
                case XmlDocumentKind.Unknown:
                default:
                    throw new InvalidOperationException(
                        $"Cannot load root for {nameof(XmlDocument)} of {nameof(Kind)} {Kind}");
            }

            SourceNode FromUnzippedStream(Func<Stream, SourceNode> deserialize)
            {
                using (var fileStream = File.OpenRead(Path))
                {
                    if (!XmlFileExtensions.ZippedExtensions.Contains(file.Extension))
                    {
                        return deserialize(fileStream);
                    }
                    using (var archive = new ZipArchive(fileStream))
                    {
                        if (archive.Entries.Count != 1)
                        {
                            throw new InvalidOperationException(
                                $"File is not a correct BattleScribe ZIP archive," +
                                $" contains {archive.Entries.Count} entries, expected 1: '{Path}'");
                        }
                        using (var entryStream = archive.Entries[0].Open())
                        {
                            return deserialize(entryStream);
                        }
                    }
                }
            }
        }
    }
}
