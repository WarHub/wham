using System;
using System.Collections.Generic;
using System.Text;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    static class Extensions
    {
        public static KeyValuePair<TKey, TValue> ToKeyValuePair<TKey, TValue>(this (TKey key, TValue value) tuple)
        {
            return new KeyValuePair<TKey, TValue>(tuple.key, tuple.value);
        }
    }
}
