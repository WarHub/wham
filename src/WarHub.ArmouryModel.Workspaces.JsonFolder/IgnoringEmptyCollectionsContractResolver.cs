using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WarHub.ArmouryModel.Workspaces.JsonFolder
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
            if (!prop.PropertyType.IsGenericType || prop.PropertyType.GetGenericTypeDefinition() != typeof(ImmutableArray<>))
            {
                return prop;
            }
            prop.DefaultValue = prop.PropertyType.GetField(nameof(ImmutableArray<int>.Empty)).GetValue(null);
            prop.ShouldSerialize = IsNotEmptyImmutableArray;
            return prop;
            bool IsNotEmptyImmutableArray(object instance)
            {
                return ((ICollection)prop.ValueProvider.GetValue(instance)).Count > 0;
            }
        }
    }
}
