// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Source
{
    /// <summary>
    /// Represents the kind of <see cref="SourceNode"/>. Wrapper struct for string.
    /// </summary>
    public struct SourceNodeKind
    {
        public SourceNodeKind(string stringValue)
        {
            StringValue = stringValue;
        }

        public static readonly SourceNodeKind Unspecified = default(SourceNodeKind);

        public string StringValue { get; }
    }
}
