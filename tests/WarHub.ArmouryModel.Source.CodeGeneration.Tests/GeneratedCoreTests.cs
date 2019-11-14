using System;
using System.Collections.Immutable;
using FluentAssertions;
using Xunit;

namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests
{
    public class GeneratedCoreTests
    {
        [Fact]
        public void Update_with_current_values_returns_this_instance()
        {
            var subject = new ContainerCore("id", "name", ImmutableArray<ItemCore>.Empty);
            var result = subject.Update("id", "name", ImmutableArray<ItemCore>.Empty);
            result.Should().BeSameAs(subject);
        }

        [Fact]
        public void Update_on_abstract_and_derived_creates_changed_instance_of_the_same_type()
        {
            var original =
                new DerivedTwiceCore(
                    "name",
                    new DateTime(2000, 1, 1),
                    ImmutableArray.Create(new ItemCore("i1", "item")));
            AbstractDerivedCore @abstract = original;

            var result = @abstract.Update("x", original.BaseDateTime, original.BaseItems);

            result.Should()
                .BeOfType<DerivedTwiceCore>()
                .Which
                .BaseName.Should().Be("x");
        }

        [Theory]
        [InlineData(typeof(ItemCore), nameof(ItemCore.WithName))]
        [InlineData(typeof(ContainerCore), nameof(ContainerCore.WithItems))]
        public void Core_With_parameter_name_is_value(Type type, string withMethodName)
        {
            var parameter = type.GetMethod(withMethodName).GetParameters()[0];

            parameter.Name.Should().Be("value");
        }

        [Fact]
        public void With_SimpleProperty_DoesNotModifyInstance()
        {
            const string NewName = "New Name";
            var subject = new ItemCore.Builder().ToImmutable();
            subject.WithName(NewName);
            Assert.Null(subject.Name);
        }

        [Fact]
        public void With_SimpleProperty_when_new_value_equals_old_returns_this_instance()
        {
            var subject = new ItemCore("1", "item");
            var result = subject.WithName("item");
            result.Should().BeSameAs(subject);
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
        public void With_CollectionProperty_when_new_value_equals_old_returns_this_instance()
        {
            var subject = new ContainerCore.Builder().ToImmutable()
                .WithItems(ImmutableArray.Create(new ItemCore("1", "item")));
            var result = subject.WithItems(subject.Items);
            result.Should().BeSameAs(subject);
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
