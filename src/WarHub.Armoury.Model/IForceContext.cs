// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    public interface IForceContext
    {
        IForce Force { get; }

        IRoster Roster { get; }

        ISelectionRegistry Selections { get; }

        ICatalogue SourceCatalogue { get; }
    }
}
