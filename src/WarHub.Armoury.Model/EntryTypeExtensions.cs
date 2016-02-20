// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System.Collections.Generic;
    using System.Linq;

    public static class EntryTypeExtensions
    {
        /// <summary>
        ///     Gets dictionary of symbols assigned to entry types.
        /// </summary>
        public static IDictionary<EntryType, string> EntryTypeSymbols { get; } = new Dictionary<EntryType, string>
        {
            [EntryType.Model] = "♦",
            [EntryType.Unit] = "❖",
            [EntryType.Upgrade] = "🔨"
        };

        /// <summary>
        ///     Gets dictionary of entry types assigned to their symbols.
        /// </summary>
        public static IDictionary<string, EntryType> SymbolEntryTypes { get; } =
            EntryTypeSymbols.Keys.ToDictionary(type => EntryTypeSymbols[type]);

        /// <summary>
        ///     Tries to get entry type with which given symbol is associated.
        /// </summary>
        /// <param name="entryTypeSymbol">Entry type symbol.</param>
        /// <returns>Entry type associated with the symbol or null if none was found.</returns>
        public static EntryType? GetEntryType(this string entryTypeSymbol)
        {
            EntryType value;
            return entryTypeSymbol != null && SymbolEntryTypes.TryGetValue(entryTypeSymbol, out value)
                ? value
                : (EntryType?) null;
        }

        /// <summary>
        ///     Tries to get symbol of given entry type.
        /// </summary>
        /// <param name="entryType">Entry type to find symbol of.</param>
        /// <returns>Symbol or empty string if no symbol is defined.</returns>
        public static string GetSymbol(this EntryType entryType)
        {
            string value;
            return EntryTypeSymbols.TryGetValue(entryType, out value) ? value : string.Empty;
        }
    }
}
