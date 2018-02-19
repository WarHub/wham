using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WarHub.ArmouryModel.CliTool.JsonUtilities
{
    /// <summary>
    /// Ignores "defaultXmlNamespace" property, as well as collections with no elements.
    /// </summary>
    class IgnoringEmptyCollectionsContractResolver : CamelCasePropertyNamesContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);
            if (prop.PropertyName.Equals("defaultXmlNamespace", StringComparison.OrdinalIgnoreCase))
            {
                prop.Ignored = true;
                return prop;
            }
            if (!typeof(ICollection).IsAssignableFrom(prop.PropertyType))
            {
                return prop;
            }
            prop.ShouldSerialize = IsNotEmptyImmutableArray;
            return prop;
            bool IsNotEmptyImmutableArray(object instance)
            {
                return ((ICollection)prop.ValueProvider.GetValue(instance)).Count > 0;
            }
        }
    }
}
