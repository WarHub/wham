// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using BattleScribeXml;
    using Repo;
    using Force = BattleScribe.Force;

    internal class ForceNode
        : XmlBackedNode<IForce, Force, BattleScribeXml.Force, IRosterContextProvider, ForceNodeArgument>
    {
        public ForceNode(Func<IList<BattleScribeXml.Force>> listGet, IRosterContextProvider parent)
            : base(parent, listGet, Transformation, Factory)
        {
        }

        protected override void ProcessItemAddition(IForce item)
        {
            item.Context = Parent.Context;
        }

        protected override void ProcessItemRemoval(IForce item)
        {
            item.Context = null;
        }

        private static Force Factory(ForceNodeArgument arg)
        {
            var catalogue = arg.Catalogue;
            var forceType = arg.ForceType;
            var xml = new BattleScribeXml.Force
            {
                CatalogueGuid = catalogue.Id.Value,
                CatalogueId = catalogue.Id.RawValue,
                CatalogueName = catalogue.Name,
                CatalogueRevision = catalogue.Revision,
                ForceTypeGuid = forceType.Id.Value,
                ForceTypeId = forceType.Id.RawValue,
                ForceTypeName = forceType.Name
            };
            xml.SetNewGuid();
            var noCategoryGuid = Guid.NewGuid();
            xml.Categories.Add(new CategoryMock
            {
                CategoryGuid = ReservedIdentifiers.NoCategoryId,
                CategoryId = ReservedIdentifiers.NoCategoryId.ToString(SampleDataInfos.GuidFormat),
                Guid = noCategoryGuid,
                Id = noCategoryGuid.ToString(SampleDataInfos.GuidFormat),
                Name = ReservedIdentifiers.NoCategoryName
            });
            var force = Transformation(xml);
            foreach (var category in forceType.Categories)
            {
                force.CategoryMocks.AddNew(category);
            }
            return force;
        }

        private static Force Transformation(BattleScribeXml.Force arg)
        {
            return new Force(arg);
        }
    }
}
