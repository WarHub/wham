// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.ConditionResolvers
{
    using Builders;

    public class CategoryConditionResolver : ConditionResolverCore<ICategoryBuilder>
    {
        public CategoryConditionResolver(ICategoryBuilder builder) : base(CategoryChildValueExtractor.Extract, builder)
        {
        }
    }
}
