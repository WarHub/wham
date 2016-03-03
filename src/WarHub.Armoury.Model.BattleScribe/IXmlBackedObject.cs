// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    public interface IXmlBackedObject<out TXml>
    {
        TXml XmlBackend { get; }
    }
}
