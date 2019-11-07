using NSubstitute;
using Xunit;
using static WarHub.ArmouryModel.Source.CodeGeneration.Tests.TestHelpers;

namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests
{
    public class SourceVisitorTests
    {
        [Fact]
        public void Visit_Null()
        {
            var node = default(SourceNode);
            var visitor = Substitute.ForPartsOf<SourceVisitor>();

            visitor.Visit(node);

            visitor.DidNotReceiveWithAnyArgs().DefaultVisit(node);
        }

        [Fact]
        public void Visit_SingleItem()
        {
            var node = EmptyItemNode;
            var visitor = Substitute.ForPartsOf<SourceVisitor>();

            visitor.Visit(node);

            visitor.Received(1).VisitItem(node);
            visitor.Received(1).DefaultVisit(node);
        }

        [Fact]
        public void VisitGeneric_SingleItem()
        {
            var expected = new object();
            var node = EmptyItemNode;
            var visitor = Substitute.ForPartsOf<SourceVisitor<object>>();
            visitor.When(x => x.VisitItem(node)).DoNotCallBase();
            visitor.VisitItem(node).Returns(expected);

            var result = visitor.Visit(node);

            Assert.Same(expected, result);
        }
    }
}
