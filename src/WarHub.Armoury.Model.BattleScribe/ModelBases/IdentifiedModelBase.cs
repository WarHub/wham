// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.ModelBases
{
    using BattleScribeXml;

    /// <summary>
    ///     Takes care of initializing Guid property and assigns the XML object ref to protected field.
    /// </summary>
    /// <typeparam name="TXml">Type of XML object held in field.</typeparam>
    public class IdentifiedModelBase<TXml> : XmlBackedModelBase<TXml>, IIdentifiable
        where TXml : IIdentified
    {
        protected IdentifiedModelBase(TXml xml)
            : base(xml)
        {
            Id = new Identifier(xml);
        }

        public IIdentifier Id { get; }
    }
}
