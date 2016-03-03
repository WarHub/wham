// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using BattleScribeXml;
    using GameSystem = BattleScribe.GameSystem;
    using ProfileType = BattleScribe.ProfileType;

    internal class ProfileTypeNode
        : XmlBackedNodeSimple<IProfileType, ProfileType, BattleScribeXml.ProfileType, GameSystem>
    {
        public ProfileTypeNode(Func<IList<BattleScribeXml.ProfileType>> listGet, GameSystem parent)
            : base(parent, listGet, Transformation, Factory)
        {
        }

        protected override void ProcessItemAddition(IProfileType item)
        {
            item.Context = Parent.Context;
        }

        protected override void ProcessItemRemoval(IProfileType item)
        {
            item.Context = null;
        }

        private static ProfileType Factory()
        {
            var xml = new BattleScribeXml.ProfileType();
            xml.SetNewGuid();
            return Transformation(xml);
        }

        private static ProfileType Transformation(BattleScribeXml.ProfileType arg)
        {
            return new ProfileType(arg);
        }
    }
}
