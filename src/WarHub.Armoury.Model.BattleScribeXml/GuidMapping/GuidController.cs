// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml.GuidMapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Repo;

    /// <summary>
    ///     Prevents unformatted ids to happen across Xml classes. Each unformatted id is assigned a new
    ///     Guid and every next appearance returns that Guid. Processing the same object more than once
    ///     will result in an exception. Processing also subscribes controller to changes, so any
    ///     changes to Guid properties will be immediately parsed to string properties.
    /// </summary>
    /// <para>
    ///     It's suggested to be used as follows:
    ///     1. load roster;
    ///     2. gather info which catalogues and game system are needed;
    ///     3. load them;
    ///     4. Process Game System
    ///     5. Process Catalogues
    ///     6. Process Roster
    /// </para>
    /// For saving changes, objects should be Reprocessed. That will assert any changes made to Guid
    /// properties will be mirrored into Id (string) properties.
    //public class GuidController
    //{
    //    public const string EntryIdSeparator = "::";
    //    public const string GuidFormat = SampleDataInfos.GuidFormat;
    //    private readonly object _processingLock = new object();

    //    public GuidController(GuidControllerMode mode)
    //    {
    //        Mode = mode;
    //        Validator = new RequirementValidator(this);
    //        CopyDict(ReservedIdentifiers.IdDictionary, GuidOfId);
    //        CopyDict(ReservedIdentifiers.NameDictionary, IdOfGuid);
    //    }

    //    public int GeneratedGuidCount { get; private set; }

    //    public GuidControllerMode Mode { get; }

    //    internal Dictionary<string, Catalogue> Catalogues { get; } = new Dictionary<string, Catalogue>();

    //    internal GameSystem GameSystem { get; private set; }

    //    internal Dictionary<string, Roster> Rosters { get; } = new Dictionary<string, Roster>();

    //    /// <summary>
    //    ///     Allows for finding Guid assigned to not-well formatted string Id.
    //    /// </summary>
    //    private Dictionary<string, Guid> GuidOfId { get; } = new Dictionary<string, Guid>();

    //    /// <summary>
    //    ///     Allows for finding not-well formatted Id to which given Guid was assigned.
    //    /// </summary>
    //    private Dictionary<Guid, string> IdOfGuid { get; } = new Dictionary<Guid, string>();

    //    private RequirementValidator Validator { get; }

    //    /// <summary>
    //    ///     Combines linked ids with '::' separator.
    //    /// </summary>
    //    /// <param name="linkedIds"></param>
    //    /// <returns></returns>
    //    public static string CombineLinkedId(List<string> linkedIds)
    //    {
    //        if (linkedIds == null)
    //            throw new ArgumentNullException(nameof(linkedIds));
    //        return linkedIds.Count == 0 ? null : string.Join(EntryIdSeparator, linkedIds);
    //    }

    //    /// <summary>
    //    ///     Parses every string Id as Guid, and if it fails, assigns a new Guid. Only one
    //    ///     (Re)Process call on single GuidController instance can be called.
    //    /// </summary>
    //    /// <param name="gameSystem">Game System to call Process on.</param>
    //    public void Process(GameSystem gameSystem)
    //    {
    //        lock (_processingLock)
    //        {
    //            Validator.ValidateRequirements(gameSystem);
    //            GameSystem = gameSystem;
    //            gameSystem.Process(this);
    //        }
    //    }

    //    /// <summary>
    //    ///     Parses every string Id as Guid, and if it fails, assigns a new Guid. Only one
    //    ///     (Re)Process call on single GuidController instance can be called.
    //    /// </summary>
    //    /// <param name="catalogue">Catalogue to call Process on.</param>
    //    public void Process(Catalogue catalogue)
    //    {
    //        lock (_processingLock)
    //        {
    //            Validator.ValidateRequirements(catalogue);
    //            Catalogues.Add(catalogue.Id, catalogue);
    //            catalogue.Process(this);
    //        }
    //    }

    //    /// <summary>
    //    ///     Parses every string Id as Guid, and if it fails, assigns a new Guid. Only one
    //    ///     (Re)Process call on single GuidController instance can be called.
    //    /// </summary>
    //    /// <param name="roster">Roster to call Process on.</param>
    //    public void Process(Roster roster)
    //    {
    //        lock (_processingLock)
    //        {
    //            Validator.ValidateRequirements(roster);
    //            Rosters.Add(roster.Id, roster);
    //            roster.Process(this);
    //        }
    //    }

    //    /// <summary>
    //    ///     Subscribes to guid changes of an object.
    //    /// </summary>
    //    /// <param name="notifier"></param>
    //    public void SubscribeGuidChanges(INotifyGuidChanged notifier)
    //    {
    //        notifier.GuidChanged += OnGuidChanged;
    //    }

    //    /// <summary>
    //    ///     Subscribes to guid list changes of an object.
    //    /// </summary>
    //    /// <param name="notifier"></param>
    //    public void SubscribeGuidListChanges(INotifyGuidListChanged notifier)
    //    {
    //        notifier.GuidListChanged += OnGuidListChanged;
    //    }

    //    internal string ParseGuid(Guid guid)
    //    {
    //        string id;
    //        return IdOfGuid.TryGetValue(guid, out id) ? id : guid.ToString(GuidFormat);
    //    }

    //    /// <summary>
    //    ///     Main functionality. If parsing string to Guid fails, that string Id is assigned new
    //    ///     Guid, and every next appearance of such will return that Guid.
    //    /// </summary>
    //    /// <param name="id">to be parsed as Guid</param>
    //    /// <returns>
    //    ///     Parsed Guid or in case parsing failed: Guid assigned to that id (may generate new Guid)
    //    /// </returns>
    //    internal Guid ParseId(string id)
    //    {
    //        var guid = Guid.Empty;
    //        if (string.IsNullOrEmpty(id) || Guid.TryParseExact(id, GuidFormat, out guid)
    //            || GuidOfId.TryGetValue(id, out guid))
    //        {
    //            return guid;
    //        }
    //        return GenerateGuid(id);
    //    }

    //    /// <summary>
    //    ///     Splits string by '::' and parses items as Guids using Process call.
    //    /// </summary>
    //    /// <param name="entryId">id list to split</param>
    //    /// <returns>Splitted and parsed Guid list.</returns>
    //    internal List<Guid> ParseLinkedId(string entryId)
    //    {
    //        if (string.IsNullOrEmpty(entryId))
    //        {
    //            return new List<Guid>(0);
    //        }
    //        var ids = entryId.Split(new[] {EntryIdSeparator},
    //            StringSplitOptions.RemoveEmptyEntries);
    //        var guidList = new List<Guid>(ids.Length);
    //        guidList.AddRange(ids.Select(ParseId));
    //        return guidList;
    //    }

    //    internal void Process<T>(IEnumerable<T> collection)
    //        where T : IGuidControllable
    //    {
    //        foreach (var item in collection)
    //        {
    //            item.Process(this);
    //        }
    //    }

    //    private static void CopyDict<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> from, IDictionary<TKey, TValue> to)
    //    {
    //        foreach (var pair in from)
    //        {
    //            to[pair.Key] = pair.Value;
    //        }
    //    }

    //    private Guid GenerateGuid(string id)
    //    {
    //        GeneratedGuidCount++;
    //        var guid = Guid.NewGuid();
    //        GuidOfId.Add(id, guid);
    //        IdOfGuid.Add(guid, id);
    //        return guid;
    //    }

    //    /// <summary>
    //    ///     Assigns new string Id value (using setter from event args) based on new guid from event args.
    //    /// </summary>
    //    /// <param name="sender">Unused.</param>
    //    /// <param name="e">Event args used to perform update operation.</param>
    //    private void OnGuidChanged(object sender, GuidChangedEventArgs e)
    //    {
    //        e.IdSetter(ParseGuid(e.NewGuid));
    //    }

    //    /// <summary>
    //    ///     Assigns new string Id value (using setter from event args) based on new guid list from
    //    ///     event args.
    //    /// </summary>
    //    /// <param name="sender">Unused.</param>
    //    /// <param name="e">Event args used to perform update operation.</param>
    //    private void OnGuidListChanged(object sender, GuidListChangedEventArgs e)
    //    {
    //        e.IdSetter(CombineLinkedId(e.NewGuidList.Select(ParseGuid).ToList()));
    //    }
    //}
}
