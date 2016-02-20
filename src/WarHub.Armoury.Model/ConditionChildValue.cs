// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    public struct ConditionChildValue
    {
        public decimal Number { get; set; }

        public bool IsInstanceOf { get; set; }

        public static implicit operator ConditionChildValue(decimal value)
        {
            return new ConditionChildValue {Number = value};
        }
    }
}
