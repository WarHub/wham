using System;

namespace WarHub.ArmouryModel.Source.Yaml;

/// <summary>
/// Thrown when YAML input does not conform to the shape expected by <see cref="YamlToXmlConverter"/>.
/// </summary>
public sealed class YamlBattleScribeFormatException : Exception
{
    public YamlBattleScribeFormatException()
    {
    }

    public YamlBattleScribeFormatException(string message)
        : base(message)
    {
    }

    public YamlBattleScribeFormatException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
