// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class ReservedIdentifiers
    {
        /// <summary>
        ///     This name is special identifier of "no" category.
        /// </summary>
        public const string NoCategoryName = "(No Category)";

        /// <summary>
        ///     This name is special identifier of roster root.
        /// </summary>
        public const string RosterAncestorName = "roster";

        /// <summary>
        ///     This name is special identifier of ancestor force.
        /// </summary>
        public const string ForceAncestorName = "force type";

        /// <summary>
        ///     This name is special identifier of ancestor category.
        /// </summary>
        public const string CategoryAncestorName = "category";

        /// <summary>
        ///     This name is special identifier of direct parent ancestor.
        /// </summary>
        public const string DirectAncestorName = "direct parent";

        /// <summary>
        ///     This name is special identifier of "no child".
        /// </summary>
        public const string NoChildName = "no child";

        /// <summary>
        ///     This is the name of actual identifier id value.
        /// </summary>
        public const string ReferenceName = "reference";

        /// <summary>
        ///     This guid is special identifier of "no" category.
        /// </summary>
        public static readonly Guid NoCategoryId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        /// <summary>
        ///     This guid is special identifier of roster root.
        /// </summary>
        public static readonly Guid RosterAncestorId = Guid.Parse("00000000-0000-0000-0000-000000000002");

        /// <summary>
        ///     This guid is special identifier of ancestor force.
        /// </summary>
        public static readonly Guid ForceAncestorId = Guid.Parse("00000000-0000-0000-0000-000000000003");

        /// <summary>
        ///     This guid is special identifier of ancestor category.
        /// </summary>
        public static readonly Guid CategoryAncestorId = Guid.Parse("00000000-0000-0000-0000-000000000004");

        /// <summary>
        ///     This guid is special identifier of direct parent ancestor.
        /// </summary>
        public static readonly Guid DirectAncestorId = Guid.Parse("00000000-0000-0000-0000-000000000005");

        /// <summary>
        ///     This guid is special identifier of "no child".
        /// </summary>
        public static readonly Guid NoChildId = Guid.Parse("00000000-0000-0000-0000-000000000006");

        /// <summary>
        ///     This guid is special identifier of null.
        /// </summary>
        public static readonly Guid NullId = Guid.Empty;


        public static readonly IReadOnlyDictionary<string, Guid> IdDictionary = new Dictionary<string, Guid>
        {
            [CategoryAncestorName] = CategoryAncestorId,
            [DirectAncestorName] = DirectAncestorId,
            [ForceAncestorName] = ForceAncestorId,
            [NoCategoryName] = NoCategoryId,
            [NoChildName] = NoChildId,
            [RosterAncestorName] = RosterAncestorId
        };

        public static readonly IReadOnlyDictionary<Guid, string> NameDictionary =
            IdDictionary.Keys.Concat(new[] {(string) null})
                .ToDictionary(name => name == null ? NullId : IdDictionary[name]);
    }
}
