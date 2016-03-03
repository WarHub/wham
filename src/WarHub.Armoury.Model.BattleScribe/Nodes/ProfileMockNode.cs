namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;

    internal class ProfileMockNode
        : XmlBackedObservableCollection<IProfileMock, ProfileMock, BattleScribeXml.ProfileMock, Selection>
    {
        public ProfileMockNode(Func<IList<BattleScribeXml.ProfileMock>> listGet, Selection parent)
            : base(parent, listGet, Transformation)
        {
        }

        protected override void ProcessItemAddition(IProfileMock item)
        {
            item.ForceContext = Parent.ForceContext;
        }

        protected override void ProcessItemRemoval(IProfileMock item)
        {
            item.ForceContext = null;
        }

        private static ProfileMock Transformation(BattleScribeXml.ProfileMock arg)
        {
            return new ProfileMock(arg);
        }
    }
}
