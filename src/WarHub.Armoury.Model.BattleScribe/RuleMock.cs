// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using System.Linq;
    using ModelBases;

    public class RuleMock : NamedIndexedModelBase<BattleScribeXml.RuleMock>, IRuleMock
    {
        private readonly LinkPath<IRule> _originPath;
        private IForceContext _forceContext;

        public RuleMock(BattleScribeXml.RuleMock xml)
            : base(xml)
        {
            _originPath = new LinkPath<IRule>(
                XmlBackend.Guids,
                newList => XmlBackend.Guids = newList,
                () => XmlBackend.Id);
        }

        public string DescriptionText
        {
            get { return XmlBackend.Description; }
            set { Set(XmlBackend.Description, value, () => XmlBackend.Description = value); }
        }

        public IForceContext ForceContext
        {
            get { return _forceContext; }
            set
            {
                if (!Set(ref _forceContext, value))
                {
                    return;
                }
                ICatalogueContext catalogueContext = null;
                if (value != null)
                {
                    catalogueContext = value.SourceCatalogue.Context;
                    catalogueContext.Rules.SetTargetOf(OriginRulePath);
                }
                _originPath.SetCatalogueContext(catalogueContext);
            }
        }

        public bool IsHidden
        {
            get { return XmlBackend.Hidden; }
            set { Set(XmlBackend.Hidden, value, () => { XmlBackend.Hidden = value; }); }
        }

        public ILinkPath<IRule> OriginRulePath => _originPath;

        public static RuleMock CreateFrom(CataloguePath path)
        {
            var rule = (IRule) path.Last();
            var xml = new BattleScribeXml.RuleMock
            {
                Book = rule.Book.Title,
                Description = rule.DescriptionText,
                Guids = path.GetRuleMockGuids(),
                Hidden = rule.IsHidden,
                Id = path.GetRuleMockId(),
                Name = rule.Name,
                Page = rule.Book.Page
            };
            return new RuleMock(xml);
        }
    }
}
