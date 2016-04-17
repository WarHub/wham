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
