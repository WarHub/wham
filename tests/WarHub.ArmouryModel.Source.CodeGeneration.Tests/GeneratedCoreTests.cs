using System.Collections.Immutable;
using WarHub.ArmouryModel.Source.CodeGeneration.Tests.GeneratedCode;
using Xunit;

namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests
{
    public class GeneratedCoreTests
    {
        [Fact]
        public void With_SimpleProperty_DoesNotModifyInstance()
        {
            const string NewName = "New Name";
            var subject = new ItemCore.Builder().ToImmutable();
            subject.WithName(NewName);
            Assert.Null(subject.Name);
        }

        [Fact]
        public void With_SimpleProperty_CreatesModifiedInstance()
        {
            const string NewName = "New Name";
            var subject = new ItemCore.Builder().ToImmutable();
            var newInstance = subject.WithName(NewName);
            Assert.Equal(NewName, newInstance.Name);
        }

        [Fact]
        public void With_CollectionProperty_DoesNotModifyInstance()
        {
            var item = new ItemCore.Builder().ToImmutable();
            var subject = new ContainerCore.Builder().ToImmutable();
            subject.WithItems(new[] { item }.ToImmutableArray());
            Assert.Empty(subject.Items);
        }

        [Fact]
        public void With_CollectionProperty_CreatesModifiedInstance()
        {
            var item = new ItemCore.Builder().ToImmutable();
            var subject = new ContainerCore.Builder().ToImmutable();
            var newInstance = subject.WithItems(new[] { item }.ToImmutableArray());
            Assert.Collection(newInstance.Items, x => Assert.Same(item, x));
        }

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
