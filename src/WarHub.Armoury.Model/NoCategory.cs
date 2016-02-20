// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Repo;

    public class NoCategory : ModelBase, ICategory
    {
        private IGameSystemContext _context;

        public NoCategory()
        {
            CategoryModifiers = new NodeSimple<ICategoryModifier>(CategoryModifierFactory);
            Id = new Identifier();
            IsAddedToParent = new AddedToParentLimits();
            Limits = new CountLimits();
        }

        public INodeSimple<ICategoryModifier> CategoryModifiers { get; }

        public IGameSystemContext Context
        {
            get { return _context; }
            set
            {
                if (EqualityComparer<IGameSystemContext>.Default.Equals(_context, value))
                {
                    return;
                }
                _context = value;
                RaisePropertyChanged();
            }
        }

        public IIdentifier Id { get; }

        public ILimits<bool, bool, bool> IsAddedToParent { get; }

        public ILimits<int, decimal, int> Limits { get; }

        public string Name
        {
            get { return ReservedIdentifiers.NoCategoryName; }
            set { throw Error.NoCategoryIsUnmodifiable(); }
        }

        private static ICategoryModifier CategoryModifierFactory()
        {
            throw Error.NoCategoryIsUnmodifiable();
        }

        private class Identifier : ModelBase, IIdentifier
        {
            public string RawValue => ReservedIdentifiers.NoCategoryId.ToString(SampleDataInfos.GuidFormat);

            public Guid Value
            {
                get { return ReservedIdentifiers.NoCategoryId; }
                set { throw Error.NoCategoryIsUnmodifiable(); }
            }

#pragma warning disable 0067
            // warnig disabled because it's a mock implementation where Id is never changing
            public event IdChangedEventHandler IdChanged;
#pragma warning restore 0067
        }

        private class CountLimits : ModelBase, ILimits<int, decimal, int>
        {
            public CountLimits()
            {
                PercentageLimit = new NoMinMax<int>(0, -1);
                PointsLimit = new NoMinMax<decimal>(0m, -1m);
                SelectionsLimit = new NoMinMax<int>(0, -1);
            }

            public IMinMax<int> PercentageLimit { get; }

            public IMinMax<decimal> PointsLimit { get; }

            public IMinMax<int> SelectionsLimit { get; }
        }

        private class NoMinMax<T> : ModelBase, IMinMax<T>
        {
            private readonly T _max;
            private readonly T _min;

            public NoMinMax(T min, T max)
            {
                _min = min;
                _max = max;
            }

            public T Max
            {
                get { return _max; }
                set { throw Error.NoCategoryIsUnmodifiable(); }
            }

            public T Min
            {
                get { return _min; }
                set { throw Error.NoCategoryIsUnmodifiable(); }
            }
        }

        private class AddedToParentLimits : ModelBase, ILimits<bool, bool, bool>
        {
            public AddedToParentLimits()
            {
                PercentageLimit = new NoMinMax<bool>(false, false);
                PointsLimit = new NoMinMax<bool>(false, false);
                SelectionsLimit = new NoMinMax<bool>(false, false);
            }

            public IMinMax<bool> PercentageLimit { get; }

            public IMinMax<bool> PointsLimit { get; }

            public IMinMax<bool> SelectionsLimit { get; }
        }

        public static class Error
        {
            public static Exception NoCategoryIsUnmodifiable()
            {
                return new NotSupportedException("No Category is unmodifiable!");
            }
        }
    }

    public class ModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
