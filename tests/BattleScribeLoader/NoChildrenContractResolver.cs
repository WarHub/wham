using System;
using System.Collections;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BattleScribeLoader
{
    /// <summary>
    /// Completely ignores properties whose type implements <see cref="ICollection"/>.
    /// </summary>
    public class NoChildrenContractResolver : CamelCasePropertyNamesContractResolver
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
            prop.Ignored = true;
            return prop;
        }
    }
}
