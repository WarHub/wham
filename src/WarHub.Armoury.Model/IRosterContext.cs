// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    public interface IRosterContext : INotifyPointCostChanged, INotifyRosterChanged
    {
        IRegistry<IForce> Forces { get; }

        IRoster Roster { get; }
    }
}
