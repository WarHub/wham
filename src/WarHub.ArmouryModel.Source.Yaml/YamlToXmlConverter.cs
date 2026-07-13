using System;
using System.Collections.Generic;
using System.Linq;
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
        ["associations"] = "association",
    };

    public static XDocument Convert(YamlMappingNode root)
    {
        var (rootKey, rootValue) = GetSingleEntry(root);
        var body = (YamlMappingNode)rootValue;
        var xmlns = GetScalar(body, "xmlns")
            ?? throw new YamlBattleScribeFormatException("missing xmlns scalar on root");
        XNamespace ns = xmlns;
        var element = new XElement(ns + rootKey);
        FillElement(element, body, ns, skipKey: "xmlns");
        return new XDocument(element);
    }

    private static void FillElement(XElement element, YamlMappingNode map, XNamespace ns, string? skipKey = null)
    {
        foreach (var (keyNode, value) in map.Children)
        {
            var key = ((YamlScalarNode)keyNode).Value!;
            if (key == skipKey) continue;
            switch (value)
            {
                case YamlScalarNode scalar when key == "$text":
                    element.Add(new XText(scalar.Value ?? ""));
                    break;
                case YamlScalarNode scalar:
                    element.SetAttributeValue(key, scalar.Value ?? "");
                    break;
                case YamlSequenceNode seq:
                    if (!ItemElementNames.TryGetValue(key, out var itemName))
                        throw new YamlBattleScribeFormatException($"unknown collection key '{key}'");
                    var container = new XElement(ns + key);
                    foreach (var item in seq.Children)
                    {
                        var itemElement = new XElement(ns + itemName);
                        FillElement(itemElement, (YamlMappingNode)item, ns);
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
        return (((YamlScalarNode)entry.Key).Value!, entry.Value);
    }

    private static string? GetScalar(YamlMappingNode map, string key) =>
        map.Children.TryGetValue(new YamlScalarNode(key), out var v) ? ((YamlScalarNode)v).Value : null;
}
