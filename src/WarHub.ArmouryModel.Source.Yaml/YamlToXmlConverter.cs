using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using YamlDotNet.RepresentationModel;

namespace WarHub.ArmouryModel.Source.Yaml;

public static class YamlToXmlConverter
{
    // BattleScribe XML: plural container element wraps singular item elements.
    // Keys not in this map that hold sequences cause a YamlBattleScribeFormatException (strict mode).
    private static readonly Dictionary<string, string> ItemElementNames = new()
    {
        ["publications"] = "publication",
        ["costTypes"] = "costType",
        ["profileTypes"] = "profileType",
        ["characteristicTypes"] = "characteristicType",
        ["categoryEntries"] = "categoryEntry",
        ["forceEntries"] = "forceEntry",
        ["selectionEntries"] = "selectionEntry",
        ["selectionEntryGroups"] = "selectionEntryGroup",
        ["sharedSelectionEntries"] = "selectionEntry",
        ["sharedSelectionEntryGroups"] = "selectionEntryGroup",
        ["sharedProfiles"] = "profile",
        ["sharedRules"] = "rule",
        ["sharedInfoGroups"] = "infoGroup",
        ["entryLinks"] = "entryLink",
        ["infoLinks"] = "infoLink",
        ["infoGroups"] = "infoGroup",
        ["categoryLinks"] = "categoryLink",
        ["catalogueLinks"] = "catalogueLink",
        ["constraints"] = "constraint",
        ["conditions"] = "condition",
        ["conditionGroups"] = "conditionGroup",
        ["modifiers"] = "modifier",
        ["modifierGroups"] = "modifierGroup",
        ["repeats"] = "repeat",
        ["profiles"] = "profile",
        ["characteristics"] = "characteristic",
        ["rules"] = "rule",
        ["costs"] = "cost",
        ["costLimits"] = "costLimit",
    };

    // Keys that are BattleScribe XML *child text elements* rather than attributes.
    // Derived from src/dataformat/xml/schema/latest/Catalogue.xsd by finding every
    // xs:string-typed <xs:element> (as opposed to <xs:attribute>): "comment" (Commentable,
    // inherited by catalogue/gameSystem/roster and their entries), "description" (Rule
    // only - there is no attribute named "description" anywhere in the schema, so no
    // per-type disambiguation is needed), "customNotes" (Roster/RosterElementBase), and
    // "readme" (catalogue/gameSystem root only). Any key in this set is emitted as
    // <key>value</key> before falling back to the attribute case below.
    private static readonly HashSet<string> TextElementNames = new(StringComparer.Ordinal)
    {
        "comment",
        "description",
        "customNotes",
        "readme",
    };

    public static XDocument Convert(YamlMappingNode root)
    {
        var (rootKey, rootValue) = GetSingleEntry(root);
        var body = AsMapping(rootValue, rootKey);
        var xmlns = GetScalar(body, "xmlns")
            ?? throw new YamlBattleScribeFormatException("missing xmlns scalar on root");
        XNamespace ns = xmlns;
        var element = CreateElement(ns, rootKey, rootKey);
        FillElement(element, body, ns, skipKey: "xmlns");
        return new XDocument(element);
    }

    private static void FillElement(XElement element, YamlMappingNode map, XNamespace ns, string? skipKey = null)
    {
        foreach (var (keyNode, value) in map.Children)
        {
            var key = AsScalarKey(keyNode);
            if (key == skipKey) continue;
            switch (value)
            {
                case YamlScalarNode scalar when key == "$text":
                    element.Add(new XText(scalar.Value ?? ""));
                    break;
                case YamlScalarNode scalar when TextElementNames.Contains(key):
                    element.Add(CreateElement(ns, key, key, scalar.Value ?? ""));
                    break;
                case YamlScalarNode scalar:
                    SetAttribute(element, key, scalar.Value ?? "");
                    break;
                case YamlSequenceNode seq:
                    if (!ItemElementNames.TryGetValue(key, out var itemName))
                        throw new YamlBattleScribeFormatException($"unknown collection key '{key}'");
                    var container = CreateElement(ns, key, key);
                    foreach (var item in seq.Children)
                    {
                        var itemMap = AsMapping(item, key);
                        var itemElement = CreateElement(ns, itemName, key);
                        FillElement(itemElement, itemMap, ns);
                        container.Add(itemElement);
                    }
                    element.Add(container);
                    break;
                default:
                    throw new YamlBattleScribeFormatException($"unexpected node type for key '{key}'");
            }
        }
    }

    private static (string Key, YamlNode Value) GetSingleEntry(YamlMappingNode root)
    {
        if (root.Children.Count != 1)
            throw new YamlBattleScribeFormatException("expected exactly one root key (gameSystem/catalogue)");
        var entry = root.Children.First();
        return (AsScalarKey(entry.Key), entry.Value);
    }

    private static string? GetScalar(YamlMappingNode map, string key) =>
        map.Children.TryGetValue(new YamlScalarNode(key), out var v) ? AsScalar(v, key) : null;

    private static string AsScalarKey(YamlNode node) =>
        node is YamlScalarNode scalar
            ? scalar.Value ?? throw new YamlBattleScribeFormatException("map key scalar has no value")
            : throw new YamlBattleScribeFormatException($"expected a scalar map key, found {DescribeNodeType(node)}");

    private static string? AsScalar(YamlNode node, string key) =>
        node is YamlScalarNode scalar
            ? scalar.Value
            : throw new YamlBattleScribeFormatException($"expected a scalar value for key '{key}', found {DescribeNodeType(node)}");

    private static YamlMappingNode AsMapping(YamlNode node, string key) =>
        node as YamlMappingNode
            ?? throw new YamlBattleScribeFormatException($"expected a mapping for key '{key}', found {DescribeNodeType(node)}");

    private static string DescribeNodeType(YamlNode node) => node switch
    {
        YamlScalarNode => "a scalar",
        YamlSequenceNode => "a sequence",
        YamlMappingNode => "a mapping",
        _ => node.GetType().Name,
    };

    private static XElement CreateElement(XNamespace ns, string name, string sourceKey)
    {
        try
        {
            return new XElement(ns + name);
        }
        catch (XmlException ex)
        {
            throw new YamlBattleScribeFormatException($"'{sourceKey}' is not a valid XML element name: {ex.Message}", ex);
        }
    }

    private static XElement CreateElement(XNamespace ns, string name, string sourceKey, string value)
    {
        try
        {
            return new XElement(ns + name, value);
        }
        catch (XmlException ex)
        {
            throw new YamlBattleScribeFormatException($"'{sourceKey}' is not a valid XML element name: {ex.Message}", ex);
        }
    }

    private static void SetAttribute(XElement element, string key, string value)
    {
        try
        {
            element.SetAttributeValue(key, value);
        }
        catch (XmlException ex)
        {
            throw new YamlBattleScribeFormatException($"'{key}' is not a valid XML attribute name: {ex.Message}", ex);
        }
    }
}
