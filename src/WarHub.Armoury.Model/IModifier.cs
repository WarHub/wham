// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System.ComponentModel;

    /// <summary>
    ///     <b>Important:</b> Interface to be inherited by specialized Modifiers <i>only</i>.
    /// </summary>
    public interface IModifier<TValue, TAction, TField> : INotifyPropertyChanged
    {
        TAction Action { get; set; }

        TField Field { get; set; }

        IRepetitionInfo Repetition { get; }

        TValue Value { get; set; }
    }
}
