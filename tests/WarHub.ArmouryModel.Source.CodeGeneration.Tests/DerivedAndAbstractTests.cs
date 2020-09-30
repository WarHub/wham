using FluentAssertions;
using Xunit;

namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests
{
    public class DerivedAndAbstractTests
    {
        [Fact]
        public void Node_of_abstract_core_is_abstract()
        {
            var type = typeof(AbstractBaseNode);

            type.Should().BeAbstract();
        }

        [Fact]
        public void Node_of_non_abstract_core_is_sealed()
        {
            var type = typeof(ItemNode);

            type.Should().BeSealed();
        }

        [Fact]
        public void With_OnAbstract_ReturnsSameActualType()
        {
            AbstractBaseCore abstractCore = new DerivedOnceCore.Builder().ToImmutable();
            var subject = abstractCore with { BaseName = "a name" };
            subject.Should().BeOfType<DerivedOnceCore>();
        }

        [Fact]
        public void ToNode_OnAbstract_ReturnsCorrectType()
        {
            AbstractBaseCore abstractCore = new DerivedOnceCore.Builder().ToImmutable();
            var subject = abstractCore.ToNode();
            subject.Should().BeOfType<DerivedOnceNode>();
        }
    }
}
