using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace WarHub.ArmouryModel.ProjectSystem
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
                Converters = { new MultilineJsonStringConverter() }
            };
        }
    }
}
