using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace WarHub.ArmouryModel.Source.XmlFormat
{
    public readonly struct VersionedElementInfo : IComparable<VersionedElementInfo>, IEquatable<VersionedElementInfo>
    {
        public VersionedElementInfo(RootElement element, BattleScribeVersion version)
        {
            Element = element;
            Version = version;
        }

        public RootElement Element { get; }

        public BattleScribeVersion Version { get; }

        public ImmutableList<VersionedElementInfo> AvailableMigrations()
        {
            var self = this;
            return
                self.Version != null
                && self.Version != self.Element.Info().CurrentVersion
                && Resources.XslMigrations.TryGetValue(self.Element, out var migrations)
                ? migrations.Where(x => x.Version > self.Version).ToImmutableList()
                : ImmutableList<VersionedElementInfo>.Empty;
        }

        public int CompareTo(VersionedElementInfo other)
        {
            return
                Element.CompareTo(other.Element) is var elementResult && elementResult != 0 ? elementResult
                : BattleScribeVersion.Compare(Version, other.Version);
        }

        public static int Compare(VersionedElementInfo left, VersionedElementInfo right)
        {
            return left.CompareTo(right);
        }

        public override string ToString() => $"{Element}, Version={Version}";

        public override bool Equals(object obj)
        {
            return obj is VersionedElementInfo info && Equals(info);
        }

        public bool Equals(VersionedElementInfo other)
        {
            return Element == other.Element && BattleScribeVersion.Equals(Version, other.Version);
        }

        public override int GetHashCode()
        {
            var hashCode = 870344294;
            hashCode = hashCode * -1521134295 + Element.GetHashCode();
            hashCode = hashCode * -1521134295 + Version?.GetHashCode() ?? 0;
            return hashCode;
        }

        public static bool operator ==(VersionedElementInfo left, VersionedElementInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VersionedElementInfo left, VersionedElementInfo right)
        {
            return !(left == right);
        }
    }
}
