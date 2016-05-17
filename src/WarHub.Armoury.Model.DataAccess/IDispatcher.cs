// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess
{
    using System;
    using System.Threading.Tasks;

    public interface IDispatcher
    {
        Task InvokeOnUiAsync(Action action);
        Task InvokeOnUiAsync(Func<Task> asyncAction);
    }
}
