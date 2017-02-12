// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Serialization;
    using Repo;

    /// <summary>
    ///     Provides convenient and (precisely) formatted (de)serialization of xml data objects.
    /// </summary>
    public class XmlSerializer
    {
        /// <summary>
        ///     Convenience method. Serializes simultaneously formatting output using
        ///     FormatAsBatleScribe method.
        /// </summary>
        /// <param name="root">Object to be serialized.</param>
        /// <param name="streamOutput">
        ///     Stream into which
        ///     <param>root</param>
        ///     will be serialized. <b>Important:</b> Stream is
        ///     NOT closed after serialization!
        /// </param>
        /// <typeparam name="T">Type of serialized object.</typeparam>
        public static void SerializeFormatted<T>(T root, Stream streamOutput) where T : IXmlProperties
        {
            using (var stream = new MemoryStream())
            {
                Serialize(root, stream);
                stream.Position = 0;
                FormatAsBattleScribe(stream, streamOutput);
            }
        }

        /// <summary>
        ///     Formats content of input and writes it to output. Formatting inludes:
        ///     * header replacement
        ///     * removal of spaces at the end of element, such as ..." /&gt;... becomes ..."/&gt;...
        ///     * encoding quotes (") in text nodes as "
        ///     * encoding apostrophes (') anywhere as '
        /// </summary>
        /// <param name="input">The stream to read. The stream after being read is disposed.</param>
        /// <param name="output">The stream to write formatted output to.</param>
        public static void FormatAsBattleScribe(Stream input, Stream output)
        {
            string xmlFormatted;
            using (var reader = new StreamReader(input))
            {
                xmlFormatted = reader.ReadToEnd();
            }
            // format header
            xmlFormatted = xmlFormatted.Replace(
                @"<?xml version=""1.0"" encoding=""utf-8""?>",
                @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>");
            // format ElementEnds
            xmlFormatted = xmlFormatted.Replace(" />", "/>");
            // encode quotes
            xmlFormatted = Regex.Replace(xmlFormatted,
                @">(([^<""]*)""([^<""]*))*<",
                m => m.Value.Replace(@"""", @"&quot;"));
            // encode apostrophes
            xmlFormatted = xmlFormatted.Replace("'", @"&apos;");
            var writer = new StreamWriter(output, Encoding.UTF8) {NewLine = "\n"};
            writer.Write(xmlFormatted);
            writer.Flush();
        }

        /// <summary>
        ///     Encloses XmlSerializer.Serialize using XmlWriter with these settings: CloseOutput =
        ///     false, Encoding = UTF8, Indent = true, OmitXmlDeclaration = false
        /// </summary>
        /// <typeparam name="T">Type of serialized object.</typeparam>
        /// <param name="root">Object to be serialized.</param>
        /// <param name="streamOutput">
        ///     Stream into which
        ///     <param>root</param>
        ///     will be serialized. <b>Important:</b> Stream is
        ///     NOT closed after serialization!
        /// </param>
        public static void Serialize<T>(T root, Stream streamOutput) where T : IXmlProperties
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add("", root.DefaultXmlNamespace);
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            var settings = new XmlWriterSettings
            {
                CloseOutput = false,
                Encoding = Encoding.UTF8,
                NewLineChars = "\n",
                NewLineHandling = NewLineHandling.Replace,
                Indent = true,
                OmitXmlDeclaration = false
            };
            using (var writer = XmlWriter.Create(streamOutput, settings))
            {
                try
                {
                    serializer.Serialize(writer, root, ns);
                }
                catch (XmlException e)
                {
                    throw new SerializationException("Serialization failed.", e);
                }
            }
        }

        /// <summary>
        ///     Deserializes content of given stream as type T and returns deserialized object. Does not
        ///     close the stream.
        /// </summary>
        /// <typeparam name="T">Type of deserialized object.</typeparam>
        /// <param name="stream">The stream to read the object from. Is not disposed.</param>
        /// <returns>The deserialized data object.</returns>
        public static T Deserialize<T>(Stream stream) where T : IXmlProperties
        {
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(T));
            try
            {
                return (T) xs.Deserialize(stream);
            }
            catch (XmlException e)
            {
                throw new SerializationException("Deserialization failed.", e);
            }
        }
    }
}
