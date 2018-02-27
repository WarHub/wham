using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace WarHub.ArmouryModel.Workspaces.JsonFolder
{
    public class JsonWorkspace
    {
        private JsonWorkspace(DirectoryInfo directory)
        {
            Serializer = CreateSerializer();
            Directory = directory;
            Root = new JsonFolder(directory, this);
        }

        public JsonFolder Root { get; }

        internal JsonSerializer Serializer { get; }

        private DirectoryInfo Directory { get; }
        
        public static JsonWorkspace CreateFromDirectory(string path)
        {
            var dirInfo = new DirectoryInfo(path);
            return new JsonWorkspace(dirInfo);
        }

        public static JsonSerializer CreateSerializer()
        {
            return new JsonSerializer
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting = Formatting.Indented,
                ContractResolver = new IgnoringEmptyCollectionsContractResolver(),
                Converters = { new MultilineJsonStringConverter() }
            };
        }
    }
}
