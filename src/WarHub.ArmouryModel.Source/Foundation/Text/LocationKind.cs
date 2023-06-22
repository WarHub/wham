namespace WarHub.ArmouryModel.Source;

/// <summary>
/// Specifies the kind of location (source vs. metadata).
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1028:Enum Storage should be Int32", Justification = "Used in structs")]
public enum LocationKind : byte
{
    /// <summary>
    /// Unspecified location.
    /// </summary>
    None = 0,

    /// <summary>
    /// The location represents a position in a source file.
    /// </summary>
    SourceFile = 1,

    ///// <summary>
    ///// The location represents a metadata file.
    ///// </summary>
    //MetadataFile = 2,

    ///// <summary>
    ///// The location represents a position in an XML file.
    ///// </summary>
    //XmlFile = 3,

    ///// <summary>
    ///// The location in some external file.
    ///// </summary>
    //ExternalFile = 4,
}
