using System;
using System.Collections;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BattleScribeLoader
{
    /// <summary>
    /// Ignores "defaultXmlNamespace" property, as well as collections with no elements.
    /// </summary>
    public class IgnoringEmptyCollectionsContractResolver : CamelCasePropertyNamesContractResolver
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
