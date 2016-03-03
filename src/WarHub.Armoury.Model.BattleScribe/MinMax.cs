// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using System;
    using ModelBases;

    public class MinMax<T> : ModelBase, IMinMax<T>
    {
        private readonly Func<T> _maxGet;
        private readonly Action<T> _maxSet;
        private readonly Func<T> _minGet;
        private readonly Action<T> _minSet;

        public MinMax(
            Func<T> minGetter, Action<T> minSetter,
            Func<T> maxGetter, Action<T> maxSetter)
        {
            _minGet = minGetter;
            _minSet = minSetter;
            _maxGet = maxGetter;
            _maxSet = maxSetter;
        }

        public virtual T Max
        {
            get { return _maxGet(); }
            set { Set(_maxGet(), value, _maxSet); }
        }

        public virtual T Min
        {
            get { return _minGet(); }
            set { Set(_minGet(), value, _minSet); }
        }
    }
}
