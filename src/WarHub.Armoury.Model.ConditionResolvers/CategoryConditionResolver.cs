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
