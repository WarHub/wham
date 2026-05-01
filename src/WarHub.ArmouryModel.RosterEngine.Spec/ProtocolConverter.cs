using BattleScribeSpec.Protocol;
using WarHub.ArmouryModel.Concrete;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.RosterEngine.Spec;

/// <summary>
/// Converts BattleScribeSpec Protocol types into wham <see cref="SourceNode"/> trees
/// and assembles them into a <see cref="WhamCompilation"/> with resolved symbols.
/// </summary>
public static class ProtocolConverter
{
    /// <summary>
    /// Converts a protocol game system and its catalogues into a fully resolved
    /// <see cref="WhamCompilation"/>.
    /// </summary>
    public static WhamCompilation CreateCompilation(
        ProtocolGameSystem gameSystem,
        ProtocolCatalogue[] catalogues)
    {
        var gstNode = ConvertGameSystem(gameSystem);
        var catNodes = catalogues.Select(c => ConvertCatalogue(c, gameSystem.Id)).ToArray();
        var trees = new[] { SourceTree.CreateForRoot(gstNode) }
            .Concat(catNodes.Select(SourceTree.CreateForRoot))
            .ToImmutableArray();
        return WhamCompilation.Create(trees);
    }

    // ===== Root-level converters =====

    private static GamesystemNode ConvertGameSystem(ProtocolGameSystem gs)
    {
        var core = new GamesystemCore
        {
            Id = gs.Id,
            Name = gs.Name,
            Revision = 1,
            BattleScribeVersion = "2.03",
            Publications = ConvertList(gs.Publications, ConvertPublication),
            CostTypes = ConvertList(gs.CostTypes, ConvertCostType),
            ProfileTypes = ConvertList(gs.ProfileTypes, ConvertProfileType),
            CategoryEntries = ConvertList(gs.CategoryEntries, ConvertCategoryEntry),
            ForceEntries = ConvertList(gs.ForceEntries, ConvertForceEntry),
            SelectionEntries = ConvertList(gs.SelectionEntries, ConvertSelectionEntry),
            EntryLinks = ConvertList(gs.EntryLinks, ConvertEntryLink),
            Rules = ConvertList(gs.Rules, ConvertRule),
            InfoLinks = ConvertList(gs.InfoLinks, ConvertInfoLink),
            SharedSelectionEntries = ConvertList(gs.SharedSelectionEntries, ConvertSelectionEntry),
            SharedSelectionEntryGroups = ConvertList(gs.SharedSelectionEntryGroups, ConvertSelectionEntryGroup),
            SharedRules = ConvertList(gs.SharedRules, ConvertRule),
            SharedProfiles = ConvertList(gs.SharedProfiles, ConvertProfile),
            SharedInfoGroups = ConvertList(gs.SharedInfoGroups, ConvertInfoGroup),
        };
        return core.ToNode();
    }

    private static CatalogueNode ConvertCatalogue(ProtocolCatalogue cat, string gameSystemId)
    {
        var core = new CatalogueCore
        {
            Id = cat.Id,
            Name = cat.Name,
            Revision = 1,
            BattleScribeVersion = "2.03",
            GamesystemId = cat.GameSystemId is { Length: > 0 } gsId ? gsId : gameSystemId,
            GamesystemRevision = 1,
            Publications = ConvertList(cat.Publications, ConvertPublication),
            CostTypes = ConvertList(cat.CostTypes, ConvertCostType),
            ProfileTypes = ConvertList(cat.ProfileTypes, ConvertProfileType),
            CategoryEntries = ConvertList(cat.CategoryEntries, ConvertCategoryEntry),
            ForceEntries = ConvertList(cat.ForceEntries, ConvertForceEntry),
            SelectionEntries = ConvertList(cat.SelectionEntries, ConvertSelectionEntry),
            EntryLinks = ConvertList(cat.EntryLinks, ConvertEntryLink),
            Rules = ConvertList(cat.Rules, ConvertRule),
            InfoLinks = ConvertList(cat.InfoLinks, ConvertInfoLink),
            SharedSelectionEntries = ConvertList(cat.SharedSelectionEntries, ConvertSelectionEntry),
            SharedSelectionEntryGroups = ConvertList(cat.SharedSelectionEntryGroups, ConvertSelectionEntryGroup),
            SharedRules = ConvertList(cat.SharedRules, ConvertRule),
            SharedProfiles = ConvertList(cat.SharedProfiles, ConvertProfile),
            SharedInfoGroups = ConvertList(cat.SharedInfoGroups, ConvertInfoGroup),
            CatalogueLinks = ConvertList(cat.CatalogueLinks, ConvertCatalogueLink),
        };
        return core.ToNode();
    }

    // ===== Entry converters =====

    private static ForceEntryCore ConvertForceEntry(ProtocolForceEntry fe) => new()
    {
        Id = fe.Id,
        Name = fe.Name,
        Hidden = fe.Hidden,
        Page = fe.Page,
        PublicationId = fe.PublicationId,
        Constraints = ConvertList(fe.Constraints, ConvertConstraint),
        Modifiers = ConvertList(fe.Modifiers, ConvertModifier),
        ModifierGroups = ConvertList(fe.ModifierGroups, ConvertModifierGroup),
        CategoryLinks = ConvertList(fe.CategoryLinks, ConvertCategoryLink),
        ForceEntries = ConvertList(fe.ForceEntries, ConvertForceEntry),
        Profiles = ConvertList(fe.Profiles, ConvertProfile),
        Rules = ConvertList(fe.Rules, ConvertRule),
        InfoGroups = ConvertList(fe.InfoGroups, ConvertInfoGroup),
        InfoLinks = ConvertList(fe.InfoLinks, ConvertInfoLink),
    };

    private static SelectionEntryCore ConvertSelectionEntry(ProtocolSelectionEntry se) => new()
    {
        Id = se.Id,
        Name = se.Name,
        Type = ParseSelectionEntryKind(se.Type),
        Hidden = se.Hidden,
        Exported = se.Import,
        Collective = se.Collective,
        Page = se.Page,
        PublicationId = se.PublicationId,
        Costs = ConvertList(se.Costs, ConvertCost),
        Constraints = ConvertList(se.Constraints, ConvertConstraint),
        Modifiers = ConvertList(se.Modifiers, ConvertModifier),
        ModifierGroups = ConvertList(se.ModifierGroups, ConvertModifierGroup),
        SelectionEntries = ConvertList(se.SelectionEntries, ConvertSelectionEntry),
        SelectionEntryGroups = ConvertList(se.SelectionEntryGroups, ConvertSelectionEntryGroup),
        EntryLinks = ConvertList(se.EntryLinks, ConvertEntryLink),
        CategoryLinks = ConvertList(se.CategoryLinks, ConvertCategoryLink),
        Profiles = ConvertList(se.Profiles, ConvertProfile),
        Rules = ConvertList(se.Rules, ConvertRule),
        InfoGroups = ConvertList(se.InfoGroups, ConvertInfoGroup),
        InfoLinks = ConvertList(se.InfoLinks, ConvertInfoLink),
    };

    private static SelectionEntryGroupCore ConvertSelectionEntryGroup(ProtocolSelectionEntryGroup seg) => new()
    {
        Id = seg.Id,
        Name = seg.Name,
        Hidden = seg.Hidden,
        Exported = seg.Import,
        Collective = seg.Collective,
        DefaultSelectionEntryId = seg.DefaultSelectionEntryId,
        Page = seg.Page,
        PublicationId = seg.PublicationId,
        Constraints = ConvertList(seg.Constraints, ConvertConstraint),
        Modifiers = ConvertList(seg.Modifiers, ConvertModifier),
        ModifierGroups = ConvertList(seg.ModifierGroups, ConvertModifierGroup),
        SelectionEntries = ConvertList(seg.SelectionEntries, ConvertSelectionEntry),
        SelectionEntryGroups = ConvertList(seg.SelectionEntryGroups, ConvertSelectionEntryGroup),
        EntryLinks = ConvertList(seg.EntryLinks, ConvertEntryLink),
        CategoryLinks = ConvertList(seg.CategoryLinks, ConvertCategoryLink),
        Profiles = ConvertList(seg.Profiles, ConvertProfile),
        Rules = ConvertList(seg.Rules, ConvertRule),
        InfoGroups = ConvertList(seg.InfoGroups, ConvertInfoGroup),
        InfoLinks = ConvertList(seg.InfoLinks, ConvertInfoLink),
    };

    private static EntryLinkCore ConvertEntryLink(ProtocolEntryLink el) => new()
    {
        Id = el.Id,
        Name = el.Name,
        TargetId = el.TargetId,
        Type = ParseEntryLinkKind(el.Type),
        Hidden = el.Hidden,
        Exported = el.Import,
        Collective = el.Collective,
        Page = el.Page,
        PublicationId = el.PublicationId,
        Costs = ConvertList(el.Costs, ConvertCost),
        Constraints = ConvertList(el.Constraints, ConvertConstraint),
        Modifiers = ConvertList(el.Modifiers, ConvertModifier),
        ModifierGroups = ConvertList(el.ModifierGroups, ConvertModifierGroup),
        CategoryLinks = ConvertList(el.CategoryLinks, ConvertCategoryLink),
        SelectionEntries = ConvertList(el.SelectionEntries, ConvertSelectionEntry),
        SelectionEntryGroups = ConvertList(el.SelectionEntryGroups, ConvertSelectionEntryGroup),
        EntryLinks = ConvertList(el.EntryLinks, ConvertEntryLink),
        Profiles = ConvertList(el.Profiles, ConvertProfile),
        Rules = ConvertList(el.Rules, ConvertRule),
        InfoGroups = ConvertList(el.InfoGroups, ConvertInfoGroup),
        InfoLinks = ConvertList(el.InfoLinks, ConvertInfoLink),
    };

    private static CategoryEntryCore ConvertCategoryEntry(ProtocolCategoryEntry ce) => new()
    {
        Id = ce.Id,
        Name = ce.Name,
        Hidden = ce.Hidden,
        Page = ce.Page,
        PublicationId = ce.PublicationId,
        Constraints = ConvertList(ce.Constraints, ConvertConstraint),
        Modifiers = ConvertList(ce.Modifiers, ConvertModifier),
        ModifierGroups = ConvertList(ce.ModifierGroups, ConvertModifierGroup),
        Profiles = ConvertList(ce.Profiles, ConvertProfile),
        Rules = ConvertList(ce.Rules, ConvertRule),
        InfoGroups = ConvertList(ce.InfoGroups, ConvertInfoGroup),
        InfoLinks = ConvertList(ce.InfoLinks, ConvertInfoLink),
    };

    private static CategoryLinkCore ConvertCategoryLink(ProtocolCategoryLink cl) => new()
    {
        Id = cl.Id,
        Name = cl.Name,
        TargetId = cl.TargetId,
        Primary = cl.Primary,
        Hidden = cl.Hidden,
        Page = cl.Page,
        PublicationId = cl.PublicationId,
        Constraints = ConvertList(cl.Constraints, ConvertConstraint),
        Modifiers = ConvertList(cl.Modifiers, ConvertModifier),
        ModifierGroups = ConvertList(cl.ModifierGroups, ConvertModifierGroup),
        Profiles = ConvertList(cl.Profiles, ConvertProfile),
        Rules = ConvertList(cl.Rules, ConvertRule),
        InfoGroups = ConvertList(cl.InfoGroups, ConvertInfoGroup),
        InfoLinks = ConvertList(cl.InfoLinks, ConvertInfoLink),
    };

    // ===== Query / modifier converters =====

    private static ConstraintCore ConvertConstraint(ProtocolConstraint c) => new()
    {
        Id = c.Id,
        Type = ParseConstraintKind(c.Type),
        Value = (decimal)c.Value,
        Field = c.Field,
        Scope = c.Scope,
        Shared = c.Shared,
        IncludeChildSelections = c.IncludeChildSelections,
        IncludeChildForces = c.IncludeChildForces,
        IsValuePercentage = c.PercentValue,
    };

    private static ModifierCore ConvertModifier(ProtocolModifier m) => new()
    {
        Type = ParseModifierKind(m.Type),
        Field = m.Field,
        Value = m.Value,
        Conditions = ConvertList(m.Conditions, ConvertCondition),
        ConditionGroups = ConvertList(m.ConditionGroups, ConvertConditionGroup),
        Repeats = ConvertList(m.Repeats, ConvertRepeat),
    };

    private static ModifierGroupCore ConvertModifierGroup(ProtocolModifierGroup mg) => new()
    {
        Conditions = ConvertList(mg.Conditions, ConvertCondition),
        ConditionGroups = ConvertList(mg.ConditionGroups, ConvertConditionGroup),
        Repeats = ConvertList(mg.Repeats, ConvertRepeat),
        Modifiers = ConvertList(mg.Modifiers, ConvertModifier),
        ModifierGroups = ConvertList(mg.ModifierGroups, ConvertModifierGroup),
    };

    private static ConditionCore ConvertCondition(ProtocolCondition c) => new()
    {
        Type = ParseConditionKind(c.Type),
        Value = (decimal)c.Value,
        Field = c.Field,
        Scope = c.Scope,
        ChildId = c.ChildId,
        Shared = c.Shared,
        IncludeChildSelections = c.IncludeChildSelections,
        IncludeChildForces = c.IncludeChildForces,
        IsValuePercentage = c.PercentValue,
    };

    private static ConditionGroupCore ConvertConditionGroup(ProtocolConditionGroup cg) => new()
    {
        Type = ParseConditionGroupKind(cg.Type),
        Conditions = ConvertList(cg.Conditions, ConvertCondition),
        ConditionGroups = ConvertList(cg.ConditionGroups, ConvertConditionGroup),
    };

    private static RepeatCore ConvertRepeat(ProtocolRepeat r) => new()
    {
        Value = (decimal)r.Value,
        RepeatCount = r.Repeats,
        Field = r.Field,
        Scope = r.Scope,
        ChildId = r.ChildId,
        Shared = r.Shared,
        IncludeChildSelections = r.IncludeChildSelections,
        IncludeChildForces = r.IncludeChildForces,
        IsValuePercentage = r.PercentValue,
        RoundUp = r.RoundUp,
    };

    // ===== Info / profile converters =====

    private static ProfileCore ConvertProfile(ProtocolProfile p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        TypeId = p.TypeId,
        TypeName = p.TypeName,
        Hidden = p.Hidden,
        Page = p.Page,
        PublicationId = p.PublicationId,
        Characteristics = ConvertList(p.Characteristics, ConvertCharacteristic),
        Modifiers = ConvertList(p.Modifiers, ConvertModifier),
        ModifierGroups = ConvertList(p.ModifierGroups, ConvertModifierGroup),
    };

    private static CharacteristicCore ConvertCharacteristic(ProtocolCharacteristic c) => new()
    {
        Name = c.Name,
        TypeId = c.TypeId,
        Value = c.Value,
    };

    private static RuleCore ConvertRule(ProtocolRule r) => new()
    {
        Id = r.Id,
        Name = r.Name,
        Description = r.Description,
        Hidden = r.Hidden,
        Page = r.Page,
        PublicationId = r.PublicationId,
        Modifiers = ConvertList(r.Modifiers, ConvertModifier),
        ModifierGroups = ConvertList(r.ModifierGroups, ConvertModifierGroup),
    };

    private static InfoGroupCore ConvertInfoGroup(ProtocolInfoGroup ig) => new()
    {
        Id = ig.Id,
        Name = ig.Name,
        Hidden = ig.Hidden,
        PublicationId = ig.PublicationId,
        Page = ig.Page,
        Profiles = ConvertList(ig.Profiles, ConvertProfile),
        Rules = ConvertList(ig.Rules, ConvertRule),
        Modifiers = ConvertList(ig.Modifiers, ConvertModifier),
        ModifierGroups = ConvertList(ig.ModifierGroups, ConvertModifierGroup),
        InfoLinks = ConvertList(ig.InfoLinks, ConvertInfoLink),
        InfoGroups = ConvertList(ig.InfoGroups, ConvertInfoGroup),
    };

    private static InfoLinkCore ConvertInfoLink(ProtocolInfoLink il) => new()
    {
        Id = il.Id,
        Name = il.Name,
        TargetId = il.TargetId,
        Type = ParseInfoLinkKind(il.Type),
        Hidden = il.Hidden,
        PublicationId = il.PublicationId,
        Page = il.Page,
        Modifiers = ConvertList(il.Modifiers, ConvertModifier),
        ModifierGroups = ConvertList(il.ModifierGroups, ConvertModifierGroup),
    };

    // ===== Catalogue / type-definition converters =====

    private static CatalogueLinkCore ConvertCatalogueLink(ProtocolCatalogueLink cl) => new()
    {
        Id = cl.Id,
        Name = cl.Name,
        TargetId = cl.TargetId,
        Type = CatalogueLinkKind.Catalogue,
        ImportRootEntries = cl.ImportRootEntries,
    };

    private static CostTypeCore ConvertCostType(ProtocolCostType ct) => new()
    {
        Id = ct.Id,
        Name = ct.Name,
        DefaultCostLimit = ct.DefaultCostLimit is { } limit ? (decimal)limit : -1m,
        Hidden = ct.Hidden,
    };

    private static CostCore ConvertCost(ProtocolCostValue cv) => new()
    {
        Name = cv.Name,
        TypeId = cv.TypeId,
        Value = (decimal)cv.Value,
    };

    private static ProfileTypeCore ConvertProfileType(ProtocolProfileType pt) => new()
    {
        Id = pt.Id,
        Name = pt.Name,
        CharacteristicTypes = ConvertList(pt.CharacteristicTypes, ConvertCharacteristicType),
    };

    private static CharacteristicTypeCore ConvertCharacteristicType(ProtocolCharacteristicType ct) => new()
    {
        Id = ct.Id,
        Name = ct.Name,
    };

    private static PublicationCore ConvertPublication(ProtocolPublication p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        ShortName = p.ShortName,
        Publisher = p.Publisher,
        PublicationDate = p.PublicationDate,
        PublisherUrl = p.PublisherUrl,
    };

    // ===== Collection helper =====

    private static ImmutableArray<TResult> ConvertList<TSource, TResult>(
        List<TSource>? source, Func<TSource, TResult> converter)
    {
        if (source is null or { Count: 0 })
        {
            return ImmutableArray<TResult>.Empty;
        }
        var builder = ImmutableArray.CreateBuilder<TResult>(source.Count);
        foreach (var item in source)
        {
            builder.Add(converter(item));
        }
        return builder.MoveToImmutable();
    }

    // ===== Enum parsing =====

    private static SelectionEntryKind ParseSelectionEntryKind(string value) => value switch
    {
        "unit" => SelectionEntryKind.Unit,
        "model" => SelectionEntryKind.Model,
        "upgrade" => SelectionEntryKind.Upgrade,
        _ => SelectionEntryKind.Upgrade,
    };

    private static ConstraintKind ParseConstraintKind(string value) => value switch
    {
        "min" => ConstraintKind.Minimum,
        "max" => ConstraintKind.Maximum,
        _ => ConstraintKind.Minimum,
    };

    private static ConditionKind ParseConditionKind(string value) => value switch
    {
        "lessThan" => ConditionKind.LessThan,
        "greaterThan" => ConditionKind.GreaterThan,
        "equalTo" => ConditionKind.EqualTo,
        "notEqualTo" => ConditionKind.NotEqualTo,
        "atLeast" => ConditionKind.AtLeast,
        "atMost" => ConditionKind.AtMost,
        "instanceOf" => ConditionKind.InstanceOf,
        "notInstanceOf" => ConditionKind.NotInstanceOf,
        _ => ConditionKind.EqualTo,
    };

    private static ConditionGroupKind ParseConditionGroupKind(string value) => value switch
    {
        "and" => ConditionGroupKind.And,
        "or" => ConditionGroupKind.Or,
        _ => ConditionGroupKind.And,
    };

    private static ModifierKind ParseModifierKind(string value) => value switch
    {
        "set" => ModifierKind.Set,
        "increment" => ModifierKind.Increment,
        "decrement" => ModifierKind.Decrement,
        "append" => ModifierKind.Append,
        "add" => ModifierKind.Add,
        "remove" => ModifierKind.Remove,
        "set-primary" => ModifierKind.SetPrimary,
        "unset-primary" => ModifierKind.UnsetPrimary,
        _ => ModifierKind.Set,
    };

    private static EntryLinkKind ParseEntryLinkKind(string value) => value switch
    {
        "selectionEntry" => EntryLinkKind.SelectionEntry,
        "selectionEntryGroup" => EntryLinkKind.SelectionEntryGroup,
        _ => EntryLinkKind.SelectionEntry,
    };

    private static InfoLinkKind ParseInfoLinkKind(string value) => value switch
    {
        "infoGroup" => InfoLinkKind.InfoGroup,
        "profile" => InfoLinkKind.Profile,
        "rule" => InfoLinkKind.Rule,
        _ => InfoLinkKind.Profile,
    };
}
