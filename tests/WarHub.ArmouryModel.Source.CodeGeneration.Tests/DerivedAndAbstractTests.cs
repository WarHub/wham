using System.Collections.Immutable;
using WarHub.ArmouryModel.Source.CodeGeneration.Tests.GeneratedCode;
using Xunit;

namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests
{
    public class DerivedAndAbstractTests
    {
        [Fact]
        public void With_OnAbstract_ReturnsSameActualType()
        {
            AbstractBaseCore derivedCore = EmptyDerivedOnceCore;
            var subject = derivedCore.WithBaseName("a name");
            Assert.IsType<DerivedOnceCore>(subject);
        }

        [Fact]
        public void ToNode_OnAbstract_ReturnsCorrectType()
        {
            AbstractBaseCore derivedCore = EmptyDerivedOnceCore;
            var subject = derivedCore.ToNode();
            Assert.IsType<DerivedOnceNode>(subject);
        }

        private static DerivedOnceCore EmptyDerivedOnceCore = new DerivedOnceCore.Builder().ToImmutable();
    }
}
