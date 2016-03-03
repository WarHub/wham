namespace WarHub.Armoury.Model.BattleScribe
{
    using BattleScribeXml;
    using ModelBases;

    public class EntryLimits : XmlBackedModelBase<IEntryBase>, IEntryLimits
    {
        public EntryLimits(IEntryBase xml)
            : base(xml)
        {
            SelectionsLimit = new NonNegativeMinMax(
                () => XmlBackend.MinSelections, newMin => XmlBackend.MinSelections = newMin,
                () => XmlBackend.MaxSelections, newMax => XmlBackend.MaxSelections = newMax
                );
            PointsLimit = new MinMax<decimal>(
                () => XmlBackend.MinPoints, newMin => XmlBackend.MinPoints = newMin,
                () => XmlBackend.MaxPoints, newMax => XmlBackend.MaxPoints = newMax
                );
            InForceLimit = new NonNegativeMinMax(
                () => XmlBackend.MinInForce, newMin => XmlBackend.MinInForce = newMin,
                () => XmlBackend.MaxInForce, newMax => XmlBackend.MaxInForce = newMax
                );
            InRosterLimit = new NonNegativeMinMax(
                () => XmlBackend.MinInRoster, newMin => XmlBackend.MinInRoster = newMin,
                () => XmlBackend.MaxInRoster, newMax => XmlBackend.MaxInRoster = newMax
                );
        }

        public IMinMax<int> SelectionsLimit { get; }

        public IMinMax<decimal> PointsLimit { get; }

        public IMinMax<int> InForceLimit { get; }

        public IMinMax<int> InRosterLimit { get; }
    }
}
