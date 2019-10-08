using WarHub.ArmouryModel.Source.CodeGeneration.Tests.GeneratedCode;
using Xunit;

namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests
{
    public class DerivedAndAbstractTests
    {
        [Fact]
        public void With_OnAbstract_ReturnsSameActualType()
        {
            AbstractBaseCore abstractCore = new DerivedOnceCore.Builder().ToImmutable();
            var subject = abstractCore.WithBaseName("a name");
            Assert.IsType<DerivedOnceCore>(subject);
        }

        [Fact]
        public void ToNode_OnAbstract_ReturnsCorrectType()
        {
            AbstractBaseCore abstractCore = new DerivedOnceCore.Builder().ToImmutable();
            var subject = abstractCore.ToNode();
            Assert.IsType<DerivedOnceNode>(subject);
        }
    }
}
