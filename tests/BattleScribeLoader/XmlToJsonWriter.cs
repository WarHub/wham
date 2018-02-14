using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using MoreLinq;
using Newtonsoft.Json;
using WarHub.ArmouryModel.Source;

namespace BattleScribeLoader
{
    /// <summary>
    /// Splits every entity into JSON with properties and folders for each collection,
    /// which contain folder for each item.
    /// </summary>
    public class XmlToJsonWriter
    {
        public XmlToJsonWriter()
        {
            Serializer = new JsonSerializer
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting = Formatting.Indented,
                ContractResolver = new NoChildrenContractResolver(),
                Converters =
                    {
                        new MultiLineStringConverter()
                    }
            };
            InvalidChars = Path.GetInvalidPathChars().Union(Path.GetInvalidFileNameChars()).ToArray();
            var escaped = Regex.Escape(new string(InvalidChars));
            EscapingRegex = new Regex($@"[\s{escaped}]+");
        }

        private Regex EscapingRegex { get; }
        private char[] InvalidChars { get; }
        private JsonSerializer Serializer { get; }

        // testing reading of MultiLineStringConverter
        public RuleCore ReadRule(DirectoryInfo directory)
        {
            using (var stream = directory.EnumerateFiles().First().OpenText())
            {
                var rule = (RuleCore.Builder)Serializer.Deserialize(stream, typeof(RuleCore.Builder));
                return rule.ToImmutable();
            }
        }

        public void WriteSplit(NodeCore core, DirectoryInfo directory)
        {
            var type = core.GetType();
            var filename = GetFilename(type, core);
            WriteNode(core, type, filename, directory);
        }

        void WriteNode(NodeCore core, Type type, string filename, DirectoryInfo directory)
        {
            using (var fileStream = File.CreateText(Path.Combine(directory.FullName, filename)))
            {
                Serializer.Serialize(fileStream, core);
            }

            var listProperties = type.GetProperties()
                .Where(p => p.PropertyType.IsConstructedGenericType && typeof(NodeCore).IsAssignableFrom(p.PropertyType.GenericTypeArguments[0]));
            foreach (var list in listProperties)
            {
                var cores = (IEnumerable<NodeCore>)list.GetValue(core);
                if (!cores.Any())
                {
                    continue;
                }
                var directoryName = GetXmlName(list.CustomAttributes);
                var listDir = directory.CreateSubdirectory(directoryName);
                foreach (var (index, child) in cores.Index())
                {
                    WriteChild(index, child, listDir);
                }
            }
        }

        void WriteChild(int index, NodeCore core, DirectoryInfo childrenDir)
        {
            var type = core.GetType();
            var filename = GetFilename(type, core);
            var childDir = childrenDir.CreateSubdirectory($"{index} {filename}");
            WriteNode(core, type, filename, childDir);
        }

        string GetFilename(Type type, NodeCore core)
        {
            if (type.GetProperty("Name") is PropertyInfo nameProp
                && nameProp.GetValue(core) is string name
                && !string.IsNullOrWhiteSpace(name))
            {
                var sanitized = SanitizeFilename(name);
                if (!string.IsNullOrWhiteSpace(sanitized))
                {
                    return sanitized;
                }
            }
            return GetXmlName(type.CustomAttributes);
        }

        string GetXmlName(IEnumerable<CustomAttributeData> attributes)
        {
            var xmlName = attributes.First(data => data.AttributeType.Name.StartsWith("Xml")).ConstructorArguments[0].Value.ToString();
            return SanitizeFilename(xmlName);
        }

        string SanitizeFilename(string raw)
        {
            if (raw.IndexOfAny(InvalidChars) < 0)
            {
                return raw;
            }
            return EscapingRegex.Replace(raw, " ");
        }
    }
}
