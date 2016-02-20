// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Repo
{
    using System;

    public class SampleDataInfos
    {
        public const string Author = "AuthorName";
        public const string GuidFormat = "D";
        public const string WarhubRev = "WH v1.1.0.3";

        public SampleDataInfos()
        {
            SampleGstInfo = new GameSystemInfo(
                "Sample Game System",
                Guid.NewGuid().ToString(GuidFormat),
                123,
                WarhubRev,
                "Sample sourcebook",
                Author);
            SampleCatInfo = new CatalogueInfo(
                "US Marine Corps Codex",
                Guid.NewGuid().ToString(GuidFormat),
                321,
                SampleGstInfo.RawId,
                WarhubRev,
                "Sample sourcebook",
                Author);
            SampleRosInfo = new RosterInfo(
                "Sample Roster of Marine Corps",
                Guid.NewGuid().ToString(GuidFormat),
                SampleGstInfo.RawId,
                WarhubRev,
                999.0m,
                1000.0m);
        }

        public CatalogueInfo SampleCatInfo { get; private set; }

        public GameSystemInfo SampleGstInfo { get; }

        public RosterInfo SampleRosInfo { get; private set; }
    }
}
