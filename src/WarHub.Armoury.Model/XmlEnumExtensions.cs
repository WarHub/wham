// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Serialization;

    public static class XmlEnumExtensions
    {
        /// <summary>
        ///     Returns enum value with given <see cref="XmlEnumAttribute.Name" /> property.
        /// </summary>
        /// <typeparam name="T">Type of enum to check.</typeparam>
        /// <param name="xmlName">XmlEnum.Name property of seeked enum.</param>
        /// <returns>enum value with given <see cref="XmlEnumAttribute.Name" /> value.</returns>
        public static T ParseXml<T>(this string xmlName)
        {
            T result;
            xmlName.TryParseXml(out result);
            return result;
        }

        /// <summary>
        ///     Returns enum value with given <see cref="XmlEnumAttribute.Name" /> property.
        /// </summary>
        /// <typeparam name="T">Type of enum to check.</typeparam>
        /// <param name="xmlName">XmlEnum.Name property of seeked enum.</param>
        /// <param name="result">enum value with given <see cref="XmlEnumAttribute.Name" /> value.</param>
        public static void ParseXml<T>(this string xmlName, out T result)
        {
            if (!xmlName.TryParseXml(out result))
            {
                throw new ArgumentException($"No enum value having such {typeof(XmlEnumAttribute).Name} exists.");
            }
        }

        /// <summary>
        ///     Sets out parameter to enum value with given <see cref="XmlEnumAttribute.Name" /> property.
        /// </summary>
        /// <typeparam name="T">Type of enum to check.</typeparam>
        /// <param name="xmlName">XmlEnum.Name property of seeked enum.</param>
        /// <param name="result">
        ///     enum value with given <see cref="XmlEnumAttribute.Name" /> value, or default value if parsing
        ///     didn't succeed.
        /// </param>
        /// <returns>True if parsing succeeded, false if not.</returns>
        public static bool TryParseXml<T>(this string xmlName, out T result)
        {
            var fieldInfos = typeof(T).GetRuntimeFields();
            var fieldValue =
                fieldInfos.FirstOrDefault(
                    info =>
                        info.GetCustomAttributes(typeof(XmlEnumAttribute), false)
                            .Cast<XmlEnumAttribute>()
                            .FirstOrDefault(attribute => attribute.Name == xmlName) != null)?.GetValue(null);
            result = fieldValue != null ? (T) fieldValue : default(T);
            return fieldValue != null;
        }

        /// <summary>
        ///     Retrieves XmlEnum.Name property of given enum value.
        /// </summary>
        /// <param name="e">The enum to retrieve it's XmlEnum.Name.</param>
        /// <returns>Retrieved Name.</returns>
        public static string XmlName(this Enum e)
        {
            var t = e.GetType();
            var info = t.GetRuntimeField(e.ToString("G"));
            if (!info.IsDefined(typeof(XmlEnumAttribute), false))
            {
                return e.ToString("G");
            }
            var xmlAtt =
                (XmlEnumAttribute)
                    info.GetCustomAttributes(typeof(XmlEnumAttribute), false)
                        .FirstOrDefault(att => att is XmlEnumAttribute);
            if (xmlAtt == null)
            {
                throw new ArgumentException($"Enum value doesn't have {typeof(XmlEnumAttribute).Name}.");
            }
            return xmlAtt.Name;
        }
    }
}
