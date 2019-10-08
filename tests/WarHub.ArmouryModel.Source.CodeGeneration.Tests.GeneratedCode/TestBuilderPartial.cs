namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests.GeneratedCode
{
    // test to check builder is partial, won't compile otherwise
    [WhamNodeCore]
    public partial class TestBuilderPartial
    {
        public string Name { get; }

        public partial class Builder
        {
        }

        public void CallBuilder()
        {
            _ = new Builder
            {
                Name = "test"
            };
        }
    }
}
