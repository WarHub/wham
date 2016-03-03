// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using System;

    public class NonNegativeMinMax : MinMax<int>
    {
        public NonNegativeMinMax(Func<int> minGetter, Action<int> minSetter, Func<int> maxGetter, Action<int> maxSetter)
            : base(minGetter, minSetter, maxGetter, maxSetter)
        {
        }

        public override int Max
        {
            get { return base.Max < 0 ? -1 : base.Max; }
            set { base.Max = value < 0 ? -1 : value; }
        }

        public override int Min
        {
            get { return base.Min < 0 ? 0 : base.Min; }
            set { base.Min = value < 0 ? 0 : value; }
        }
    }
}
