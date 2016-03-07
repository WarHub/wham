// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.Files
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using BattleScribeXml;
    using Repo;

    /// <summary>
    ///     Provides methods to read BattleScribe index-formatted data from streams and deserialize
    ///     them into <see cref="RemoteDataSourceIndex" />.
    /// </summary>
    public class DataIndexFile
    {
        /// <summary>
        ///     Creates new source index based on BattleScribe index.xml or .bsi-formatted stream,
        ///     automatically choosing format depending on given filepath's file extension.
        /// </summary>
        /// <param name="filepath">Filepath or filename to find out a format of the file.</param>
        /// <param name="stream">Contains index.xml content.</param>
        /// <returns>Created source index object.</returns>
        /// <exception cref="NotSupportedException">
        ///     If the <paramref name="filepath" /> has unsupported extension.
        /// </exception>
        /// <exception cref="InvalidDataException">When index could not be read.</exception>
        public static RemoteDataSourceIndex ReadBattleScribeIndexAuto(string filepath, Stream stream)
        {
            try
            {
                if (filepath.EndsWith(".bsi"))
                {
                    return ReadBattleScribeIndexZipped(stream);
                }
                if (filepath.EndsWith(".xml"))
                {
                    return ReadBattleScribeIndex(stream);
                }
            }
            catch (Exception e)
            {
                throw new InvalidDataException($"Failed to read index file {filepath}", e);
            }
            throw new NotSupportedException($"Not .bsi nor .xml file received ({filepath}).");
        }

        /// <summary>
        ///     Creates new source index based on BattleScribe index.xml-formatted stream.
        /// </summary>
        /// <param name="stream">Contains index.xml content.</param>
        /// <returns>Created source index object.</returns>
        /// <exception cref="InvalidDataException">On error reading index content.</exception>
        public static RemoteDataSourceIndex ReadBattleScribeIndex(Stream stream)
        {
            var index = XmlSerializer.Deserialize<DataIndex>(stream);
            try
            {
                return index.CreateSourceIndex();
            }
            catch (NotSupportedException e)
            {
                string indexString = null;
                try
                {
                    using (var memoryStream = new MemoryStream())
                    using (var reader = new StreamReader(memoryStream))
                    {
                        XmlSerializer.Serialize(index, memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        indexString = reader.ReadToEnd();
                    }
                }
                catch (Exception)
                {
                    // ignore failure on reading
                }
                if (indexString != null)
                {
                    throw new InvalidDataException($"Source index had no source URL. index content: {indexString}", e);
                }
                throw;
            }
        }

        /// <summary>
        ///     Creates new source index based on BattleScribe zipped index.bsi-formatted stream.
        /// </summary>
        /// <param name="stream">Contains index.bsi zip archive content.</param>
        /// <returns>Created source index object.</returns>
        /// <exception cref="NotSupportedException">
        ///     If the zipped <paramref name="stream" /> has invalid number of entries.
        /// </exception>
        public static RemoteDataSourceIndex ReadBattleScribeIndexZipped(Stream stream)
        {
            using (var archive = new ZipArchive(stream))
            {
                if (archive.Entries.Count != 1 || !archive.Entries.Single().Name.EndsWith(".xml"))
                {
                    throw new NotSupportedException("Wrong number of *.bsi zip entries.");
                }
                using (var indexStream = archive.Entries[0].Open())
                {
                    return ReadBattleScribeIndex(indexStream);
                }
            }
        }
    }
}
