// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    public interface IForceItem : IForceContextProvider
    {
        new IForceContext ForceContext { get; set; }
    }
}
