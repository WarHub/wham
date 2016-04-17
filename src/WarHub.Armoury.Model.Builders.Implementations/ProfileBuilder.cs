// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Builders.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ProfileBuilder : BuilderCore, IProfileBuilder
    {
        public ProfileBuilder(ProfileLinkPair profileLinkPair, IBuilderCore parentBuilder,
            IBuilderAncestorContext ancestorContext) : base(parentBuilder, ancestorContext)
        {
            if (profileLinkPair == null)
                throw new ArgumentNullException(nameof(profileLinkPair));
            ProfileLinkPair = profileLinkPair;
            var profile = profileLinkPair.Profile;
            CharacteristicBuilders =
                profile.TypeLink.Target.CharacteristicTypes.Zip(profile.Characteristics,
                    (type, characteristic) => new CharacteristicBuilder(type, characteristic)).ToArray();
        }

        public IApplicableVisibility ApplicableVisibility { get; } = new ApplicableVisibility();

        public ProfileLinkPair ProfileLinkPair { get; }

        public string ApplicableName { get; set; }

        public IReadOnlyCollection<ICharacteristicBuilder> CharacteristicBuilders { get; }

        public override IStatAggregate StatAggregate { get; } = new ProfileStatAggregate();

        public override bool IsForEntityId(Guid idValue) => ProfileLinkPair.AnyHasId(idValue);

        public override void ApplyModifiers()
        {
            throw new NotImplementedException();
        }

        private class ProfileStatAggregate : IStatAggregate
        {
            public IEnumerable<IStatAggregate> ChildrenAggregates => Enumerable.Empty<IStatAggregate>();

            public uint ChildSelectionsCount => 0;

            public decimal PointsTotal => 0;

            public decimal GetPointsTotal(Guid nodeGuid) => 0;
            public uint GetSelectionCount(Guid selectionGuid) => 0;
        }
    }
}
