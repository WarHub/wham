using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal static class GeneralExtensions
    {
        /// <summary>
        /// Returns result of invoking <paramref name="mutation"/> on <paramref name="original"/>
        /// if <paramref name="condition"/> is true, else <paramref name="original"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <param name="condition"></param>
        /// <param name="mutation"></param>
        /// <returns></returns>
        public static T MutateIf<T>(this T original, bool condition, Func<T, T> mutation)
        {
            return condition ? mutation(original) : original;
        }

        public static T MutateIf<T>(this T original, bool condition, Func<T, T> mutationIf, Func<T, T> mutationElse)
        {
            return condition ? mutationIf(original) : mutationElse(original);
        }

        public static T Mutate<T>(this T original, Func<T, T> mutation) => mutation(original);

        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
        {
            key = pair.Key;
            value = pair.Value;
        }

        public static string? GetStringOrThrow(this TypedConstant constant) => constant switch
        {
            { IsNull: true } => null,
            { Kind: TypedConstantKind.Primitive, Type: { SpecialType: SpecialType.System_String } } => constant.Value!.ToString(),
            _ => throw new InvalidOperationException("Can't read constant.")
        };

        public static INamedTypeSymbol GetTypeByMetadataNameOrThrow(this Compilation compilation, string fullyQualifiedMetadataName) =>
            compilation.GetTypeByMetadataName(fullyQualifiedMetadataName)
            ?? throw new InvalidOperationException("Symbol not found: " + fullyQualifiedMetadataName);
    }
}
