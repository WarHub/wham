// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using BattleScribeXml;
    using ForceType = BattleScribe.ForceType;

    internal class ForceTypeNode
        : XmlBackedNodeSimple<IForceType, ForceType, BattleScribeXml.ForceType, IGameSystemContextProvider>
    {
        public ForceTypeNode(Func<IList<BattleScribeXml.ForceType>> listGet,
            IGameSystemContextProvider parent)
            : base(parent, listGet, Transformation, Factory)
        {
        }

        protected override void ProcessItemAddition(IForceType item)
        {
            item.Context = Parent.Context;
        }

        protected override void ProcessItemRemoval(IForceType item)
        {
            item.Context = null;
        }

        private static ForceType Factory()
        {
            var xml = new BattleScribeXml.ForceType();
            xml.SetNewGuid();
            return Transformation(xml);
        }

        private static ForceType Transformation(BattleScribeXml.ForceType arg)
        {
            return new ForceType(arg);
        }
    }
}
