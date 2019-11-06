using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;

namespace WarHub.ArmouryModel.Source.XmlFormat
{
    public sealed class BattleScribeVersion : IComparable<BattleScribeVersion>, IEquatable<BattleScribeVersion>
    {
        private BattleScribeVersion(int major, int minor, string suffix)
        {
            Major = major;
            Minor = minor;
            Suffix = suffix;
        }

        /// <summary>
        /// BattleScribe v1.13b is the oldest "compatible"-ish format.
        /// </summary>
        public static BattleScribeVersion V1x13b { get; } = Create(1, 13, "b");
        public static BattleScribeVersion V1x15b { get; } = Create(1, 15, "b");
        public static BattleScribeVersion V1x15 { get; } = Create(1, 15);
        public static BattleScribeVersion V2x00 { get; } = Create(2, 0);
        public static BattleScribeVersion V2x01 { get; } = Create(2, 1);
        public static BattleScribeVersion V2x02 { get; } = Create(2, 2);
        public static BattleScribeVersion V2x03 { get; } = Create(2, 3);

        public static ImmutableSortedSet<BattleScribeVersion> WellKnownVersions { get; }
            = ImmutableSortedSet.Create(
                V1x13b,
                V1x15b,
                V1x15,
                V2x00,
                V2x01,
                V2x02,
                V2x03);

        public int Major { get; }

        public int Minor { get; }

        public string Suffix { get; }

        public bool IsStable => string.IsNullOrEmpty(Suffix);

        public bool IsPrerelease => !IsStable;

        public string BattleScribeString => $"{Major}.{Minor:D2}{Suffix}";

        public string FilepathString => $"{Major}_{Minor:D2}{Suffix}";

        public BattleScribeVersion WithMajor(int major) => Create(major, Minor, Suffix);

        public BattleScribeVersion WithMinor(int minor) => Create(Major, minor, Suffix);

        public BattleScribeVersion WithSuffix(string suffix) => Create(Major, Minor, suffix);

        public static BattleScribeVersion Parse(string version)
        {
            var match = Regex.Match(version, @"^(\d+)\.(\d+)(.*)$");
            if (match == null || match.Groups.Count != 4)
            {
                throw new FormatException("Invalid BattleScribe data format");
            }
            var major = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            var minor = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            var suffix = match.Groups[3].Value;
            return Create(major, minor, suffix);
        }

        public static BattleScribeVersion Create(int major, int minor, string suffix = null)
        {
            if (major < 0)
                throw new ArgumentOutOfRangeException(nameof(major), "Must be >= 0.");
            if (minor < 0)
                throw new ArgumentOutOfRangeException(nameof(minor), "Must be >= 0.");
            var suffixNormalized = string.IsNullOrEmpty(suffix) ? null : suffix;
            return new BattleScribeVersion(major, minor, suffixNormalized);
        }

        public int CompareTo(BattleScribeVersion other)
        {
            return
                other is null ? 1
                : Major.CompareTo(other.Major) is var majorResult && majorResult != 0 ? majorResult
                : Minor.CompareTo(other.Minor) is var minorResult && minorResult != 0 ? minorResult
                : IsStable.CompareTo(other.IsStable) is var stableResult && stableResult != 0 ? stableResult
                : string.CompareOrdinal(Suffix, other.Suffix);
        }

        public override bool Equals(object obj) => Equals(obj as BattleScribeVersion);

        public bool Equals(BattleScribeVersion other) => CompareTo(other) == 0;

        public override string ToString() => BattleScribeString;

        public override int GetHashCode()
        {
            var hashCode = -1092680650;
            hashCode = (hashCode * -1521134295) + Major.GetHashCode();
            hashCode = (hashCode * -1521134295) + Minor.GetHashCode();
            hashCode = (hashCode * -1521134295) + Suffix?.GetHashCode() ?? 0;
            return hashCode;
        }

        public static int Compare(BattleScribeVersion left, BattleScribeVersion right)
            => left is null ? (right is null ? 0 : -1) : left.CompareTo(right);

        public static bool Equals(BattleScribeVersion left, BattleScribeVersion right)
            => left is null ? right is null : left.Equals(right);

        public static bool operator <(BattleScribeVersion left, BattleScribeVersion right)
            => Compare(left, right) < 0;

        public static bool operator <=(BattleScribeVersion left, BattleScribeVersion right)
            => Compare(left, right) <= 0;

        public static bool operator >(BattleScribeVersion left, BattleScribeVersion right)
            => Compare(left, right) > 0;

        public static bool operator >=(BattleScribeVersion left, BattleScribeVersion right)
            => Compare(left, right) >= 0;

        public static bool operator ==(BattleScribeVersion left, BattleScribeVersion right)
            => left?.Equals(right) ?? right is null;

        public static bool operator !=(BattleScribeVersion left, BattleScribeVersion right)
            => !(left == right);
    }
}
