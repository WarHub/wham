// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    public interface IXmlProperties
    {
        string DefaultXmlNamespace { get; }
    }

    public partial class Catalogue : IXmlProperties
    {
        public const string CatalogueXmlNamespace = "http://www.battlescribe.net/schema/catalogueSchema";

        public string DefaultXmlNamespace => CatalogueXmlNamespace;
    }
}
