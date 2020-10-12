using Newtonsoft.Json;

namespace WarHub.ArmouryModel.Workspaces.Gitree.Serialization
{
    public static class JsonUtilities
    {
        public static JsonSerializer CreateSerializer()
        {
            return new JsonSerializer
            {
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                Formatting = Formatting.Indented,
                ContractResolver = new IgnoringEmptyCollectionsContractResolver(),
                Converters =
                {
                    new MultilineJsonStringConverter(),
                    new Newtonsoft.Json.Converters.StringEnumConverter()
                }
            };
        }
    }
}
