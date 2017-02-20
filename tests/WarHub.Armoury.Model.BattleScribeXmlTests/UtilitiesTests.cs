using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WarHub.Armoury.Model.BattleScribeXmlTests
{
    using BattleScribeXml;
    using Xunit;

    public class UtilitiesTests
    {
        private static string CatalogueIdSelector(Force force) => force.CatalogueId;

        [Fact]
        public void SelectWithNestedForces_ListNullException()
        {
            IEnumerable<Force> collection = null;
            Func<Force, string> selector = CatalogueIdSelector;
            Assert.Throws<ArgumentNullException>(() => collection.SelectWithNestedForces(selector));
        }

        [Fact]
        public void SelectWithNestedForces_SelectorNullException()
        {
            IEnumerable<Force> collection = Enumerable.Empty<Force>();
            Func<Force, string> selector = null;
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.Throws<ArgumentNullException>(() => collection.SelectWithNestedForces(selector));
        }

        [Theory]
        public void SelectWithNestedForces_CorrectCount()
        {
            var pack = Forces1;
            var count = pack.Forces.SelectWithNestedForces(x => x).Count();

            Assert.Equal(pack.Count, count);
        }

        private struct ForcesTestPack
        {
            public IEnumerable<Force> Forces { get; set; }

            public int Count { get; set; }
        }

        private static ForcesTestPack Forces1 =>
            new ForcesTestPack
            {
                Forces = new[]
            {
                new Force
                {
                    Forces =
                    {
                        new Force()
                    }
                }
            },
                Count = 2
            };
    }
}
