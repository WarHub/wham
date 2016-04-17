// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.ConditionResolvers
{
    using Builders;

    public class CatalogueConditionResolver : ConditionResolverCore<IBuilderCore>
    {
        public CatalogueConditionResolver(IBuilderCore builder) : base(CatalogueChildValueExtractor.Extract, builder)
        {
        }
    }
}
