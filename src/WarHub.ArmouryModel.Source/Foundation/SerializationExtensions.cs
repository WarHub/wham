using System;

namespace WarHub.ArmouryModel.Source
{
    public interface IXmlSerializable
    {
        Type GetSerializationType();
        object GetSerializableObject();
    }
}
