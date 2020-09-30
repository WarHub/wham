using Xunit;

namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests
{
    public class GeneratedCoreTests
    {
        [Fact]
        public void ToBuilder_CopiesSimpleProperty()
        {
            const string Name = "Name";
            var subject = new ItemCore.Builder() { Name = Name }.ToImmutable();
            var builder = subject.ToBuilder();
            Assert.Equal(subject.Name, builder.Name);
        }

        [Fact]
        public void ToBuilder_CopiesCollectionProperty()
        {
            const string ItemId = "qwerty12345";
            var itemBuilder = new ItemCore.Builder() { Id = ItemId };
            var container = new ContainerCore.Builder() { Items = { itemBuilder } }.ToImmutable();
            var builder = container.ToBuilder();
            Assert.Collection(builder.Items, x => Assert.Same(ItemId, x.Id));
        }
    }
}
