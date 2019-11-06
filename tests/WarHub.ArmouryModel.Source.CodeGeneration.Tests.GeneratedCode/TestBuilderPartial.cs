namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests.GeneratedCode
{
    // test to check builder is partial, won't compile otherwise
    [WhamNodeCore]
    public partial class TestBuilderPartial
    {
        public string Name { get; }

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
