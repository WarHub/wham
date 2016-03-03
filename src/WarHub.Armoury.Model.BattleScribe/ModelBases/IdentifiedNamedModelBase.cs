// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.ModelBases
{
    using System.Diagnostics;
    using BattleScribeXml;

    /// <summary>
    ///     In addition to taking care of Guid, provides access to Name property of given XML class.
    /// </summary>
    /// <typeparam name="TXml">XML object held in protected field.</typeparam>
    [DebuggerDisplay("{Name}")]
    public class IdentifiedNamedModelBase<TXml> : IdentifiedModelBase<TXml>, INameable
        where TXml : IIdentified, INamed
    {
        public IdentifiedNamedModelBase(TXml xml)
            : base(xml)
        {
        }

        public string Name
        {
            get { return XmlBackend.Name; }
            set { Set(XmlBackend.Name, value, () => XmlBackend.Name = value); }
        }
    }
}
