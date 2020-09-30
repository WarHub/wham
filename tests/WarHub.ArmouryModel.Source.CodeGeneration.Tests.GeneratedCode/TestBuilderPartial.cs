namespace WarHub.ArmouryModel.Source
{
    // test to check builder is partial, won't compile otherwise
    [WhamNodeCore]
    public partial record TestBuilderPartial
    {
        public string? Name { get; init; }

#pragma warning disable CA1034 // Nested types should not be visible
        public partial class Builder
#pragma warning restore CA1034 // Nested types should not be visible
        {
        }

        public static void CallBuilder()
        {
            _ = new Builder
            {
                Name = "test"
            };
        }
    }
}
