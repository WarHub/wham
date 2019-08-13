using System;
using System.Collections.Immutable;
using System.Linq;

namespace WarHub.ArmouryModel.Source.XmlFormat
{
    public readonly struct VersionedElementInfo : IComparable<VersionedElementInfo>
    {
        public VersionedElementInfo(RootElement rootElement, BattleScribeVersion version)
        {
            RootElement = rootElement;
            Version = version;
        }

        public RootElement RootElement { get; }

        public BattleScribeVersion Version { get; }

        public ImmutableList<VersionedElementInfo> AvailableMigrations()
        {
            var self = this;
            return
                self.Version != null
                && self.Version != self.RootElement.Info().CurrentVersion
                && Resources.XslMigrations.TryGetValue(self.RootElement, out var migrations)
                ? migrations.Where(x => x.Version > self.Version).ToImmutableList()
                : ImmutableList<VersionedElementInfo>.Empty;
        }

        public int CompareTo(VersionedElementInfo other)
        {
            return Compare(this, other);
        }

        public static int Compare(VersionedElementInfo left, VersionedElementInfo right)
        {
            return
                left.RootElement.CompareTo(right.RootElement) is var elementResult && elementResult != 0 ? elementResult
                : BattleScribeVersion.Compare(left.Version, right.Version);
        }

        public override string ToString() => $"{RootElement}, Version={Version}";
    }
}
