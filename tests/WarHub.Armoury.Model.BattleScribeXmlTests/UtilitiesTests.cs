// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXmlTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
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
            var collection = Enumerable.Empty<Force>();
            Func<Force, string> selector = null;
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.Throws<ArgumentNullException>(() => collection.SelectWithNestedForces(selector));
        }

        [Theory]
        [ClassData(typeof(TestPacks))]
        public void SelectWithNestedForces_CorrectCount(ForcesTestPack pack)
        {
            var count = pack.Forces.SelectWithNestedForces(x => x).Count();

            Assert.Equal(pack.Count, count);
        }

        public class TestPacks : IEnumerable<object[]>
        {
            public IEnumerable<object[]> ForceDataRows { get; } =
                new[]
                {
                    new object[] {Forces0},
                    new object[] {Forces1},
                    new object[] {Forces2},
                    new object[] {Forces3}
                };

            private static ForcesTestPack Forces0 =>
                new ForcesTestPack
                {
                    Forces = new Force[]
                    {
                    },
                    Count = 0
                };

            private static ForcesTestPack Forces1 =>
                new ForcesTestPack
                {
                    Forces = new[]
                    {
                        new Force()
                    },
                    Count = 1
                };

            private static ForcesTestPack Forces2 =>
                new ForcesTestPack
                {
                    Forces = new[]
                    {
                        new Force
                        {
                            Forces =
                            {
                                new Force(),
                                new Force(),
                                new Force(),
                                new Force()
                            }
                        }
                    },
                    Count = 5
                };

            private static ForcesTestPack Forces3 =>
                new ForcesTestPack
                {
                    Forces = new[]
                    {
                        new Force(),
                        new Force
                        {
                            Forces =
                            {
                                new Force(),
                                new Force
                                {
                                    Forces =
                                    {
                                        new Force(),
                                        new Force()
                                    }
                                }
                            }
                        }
                    },
                    Count = 6
                };

            public IEnumerator<object[]> GetEnumerator()
            {
                return ForceDataRows.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public struct ForcesTestPack
        {
            public IEnumerable<Force> Forces { get; set; }

            public int Count { get; set; }
        }
    }
}
