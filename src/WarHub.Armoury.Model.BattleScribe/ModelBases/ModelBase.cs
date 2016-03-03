// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.ModelBases
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected static bool CheckEquals<T>(T v1, T v2)
        {
            return EqualityComparer<T>.Default.Equals(v1, v2);
        }

        protected bool Set<T>(T oldValue, T newValue, Action<T> assignment,
            [CallerMemberName] string propertyName = null)
        {
            if (CheckEquals(oldValue, newValue))
            {
                return false;
            }
            assignment(newValue);
            RaisePropertyChanged(propertyName);
            return true;
        }

        protected bool Set<T>(T oldValue, T newValue, Action assignment,
            [CallerMemberName] string propertyName = null)
        {
            if (CheckEquals(oldValue, newValue))
            {
                return false;
            }
            assignment();
            RaisePropertyChanged(propertyName);
            return true;
        }

        protected bool Set<T>(string propertyName, ref T field, T newValue)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;
            field = newValue;
            RaisePropertyChanged(propertyName);
            return true;
        }

        protected bool Set<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            return Set(propertyName, ref field, newValue);
        }

        /// <summary>
        ///     If <paramref name="oldValue" /> is different from <paramref name="newValue" /> , following
        ///     actions are taken:
        ///     1. XmlName attribute of <paramref name="newValue" /> is read;
        ///     2. that string is provided as argument for <paramref name="xmlAssignment" /> which is called;
        ///     3. <paramref name="oldValue" /> is assigned <paramref name="newValue" /> ;
        ///     4. RaisePropertyChanged is called;
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        /// <param name="xmlAssignment"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected bool EnumSet<TEnum>(ref TEnum oldValue, TEnum newValue,
            Action<string> xmlAssignment, [CallerMemberName] string propertyName = null)
        {
            if (CheckEquals(oldValue, newValue))
            {
                return false;
            }
            var name = ((Enum) (object) oldValue).XmlName();
            xmlAssignment(name);
            oldValue = newValue;
            RaisePropertyChanged(propertyName);
            return true;
        }

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void RaiseCallingPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
