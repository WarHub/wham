using System;
using System.Collections.Immutable;

namespace WarHub.ArmouryModel.Source
{
    public interface IXmlSerializable
    {
        Type GetSerializationType();
        object GetSerializableObject();

    }

    internal static class SerializationExtensions
    {
        // generate similar for every type
        //internal static CharacteristicType.FastSerializationEnumerable ToFastSerializationEnumerable(this ImmutableArray<CharacteristicType> enumerable)
        //{
        //    return new CharacteristicType.FastSerializationEnumerable(enumerable);
        //}
    }
}