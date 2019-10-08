using System.Collections.Generic;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    internal static class Extensions
    {
        public static KeyValuePair<TKey, TValue> ToKeyValuePair<TKey, TValue>(this (TKey key, TValue value) tuple)
        {
            return new KeyValuePair<TKey, TValue>(tuple.key, tuple.value);
        }
    }
}
