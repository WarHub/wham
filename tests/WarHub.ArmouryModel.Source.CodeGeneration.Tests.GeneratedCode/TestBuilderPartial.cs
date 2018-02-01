using System;
using System.Collections.Generic;
using System.Text;

namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests.GeneratedCode
{
    // test to check builder is partial, won't compile otherwise
    [WhamNodeCore]
    public partial class TestBuilderPartial
    {
        public string Name { get; }

        partial class Builder
        {

        }

        private void CallBuilder()
        {
            var builder = new Builder
            {
                Name = "test"
            };
        }
    }
}
