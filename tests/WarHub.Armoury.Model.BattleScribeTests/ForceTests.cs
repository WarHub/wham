// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeTests
{
    using System;
    using BattleScribeXml;
    using Xunit;

    /// <summary>
    ///     Summary description for ForceTests
    /// </summary>
    public class ForceTests
    {
        [Fact]
        public void CatalogueChangeTest()
        {
            var xmlCatalogue1 = new Catalogue
            {
                Name = "Catalogue 1",
                Guid = Guid.NewGuid(),
                Revision = 1
            };
            var catalogue1 = new BattleScribe.Catalogue(xmlCatalogue1);
            var xmlCatalogue2 = new Catalogue
            {
                Name = "Catalogue 2",
                Guid = Guid.NewGuid(),
                Revision = 2
            };
            var catalogue2 = new BattleScribe.Catalogue(xmlCatalogue2);
            var xmlForce = new Force
            {
                CatalogueGuid = Guid.NewGuid(),
                CatalogueName = "Other catalogue",
                CatalogueRevision = 100,
                ForceTypeGuid = Guid.NewGuid(),
                ForceTypeName = "Other type",
                Guid = Guid.NewGuid()
            };
            var force = new BattleScribe.Force(xmlForce);
            force.CatalogueLink.Target = catalogue1;
            Assert.Equal(catalogue1.Id.Value, force.CatalogueLink.TargetId.Value);
            Assert.Equal(catalogue1.Name, force.CatalogueName);
            Assert.Equal(catalogue1.Revision, force.CatalogueRevision);
            force.CatalogueLink.Target = catalogue2;
            Assert.Equal(catalogue2.Id.Value, force.CatalogueLink.TargetId.Value);
            Assert.Equal(catalogue2.Name, force.CatalogueName);
            Assert.Equal(catalogue2.Revision, force.CatalogueRevision);
        }
    }
}
