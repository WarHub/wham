// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System.ComponentModel;
    using System.Xml.Serialization;

    public enum LimitField
    {
        [XmlEnum("minSelections")] MinSelections,

        [XmlEnum("maxSelections")] MaxSelections,

        [XmlEnum("minPoints")] MinPoints,

        [XmlEnum("maxPoints")] MaxPoints,

        [XmlEnum("minPercentage")] MinPercentage,

        [XmlEnum("maxPercentage")] MaxPercentage
    }

    public interface ILimits<TSelectionLim, TPointsLim, TPercentLim> : INotifyPropertyChanged
    {
        IMinMax<TPercentLim> PercentageLimit { get; }

        IMinMax<TPointsLim> PointsLimit { get; }

        IMinMax<TSelectionLim> SelectionsLimit { get; }
    }
}
