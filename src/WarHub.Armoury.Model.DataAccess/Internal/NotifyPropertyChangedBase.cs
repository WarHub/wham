// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess.Internal
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class NotifyPropertyChangedBase
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected static bool CheckEquals<T>(ref T v1, T v2)
        {
            return EqualityComparer<T>.Default.Equals(v1, v2);
        }

        protected bool Set<T>(T oldValue, T newValue, Action<T> assignment,
            [CallerMemberName] string propertyName = null)
        {
            if (CheckEquals(ref oldValue, newValue))
            {
                return false;
            }
            assignment(newValue);
            RaiseSomePropertyChanged(propertyName);
            return true;
        }

        protected bool Set<T>(T oldValue, T newValue, Action assignment,
            [CallerMemberName] string propertyName = null)
        {
            if (CheckEquals(ref oldValue, newValue))
            {
                return false;
            }
            assignment();
            RaiseSomePropertyChanged(propertyName);
            return true;
        }

        protected bool Set<T>(string propertyName, ref T field, T newValue)
        {
            if (CheckEquals(ref field, newValue))
            {
                return false;
            }
            field = newValue;
            RaiseSomePropertyChanged(propertyName);
            return true;
        }

        protected bool Set<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (CheckEquals(ref field, newValue))
            {
                return false;
            }
            field = newValue;
            RaiseSomePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void RaiseSomePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
