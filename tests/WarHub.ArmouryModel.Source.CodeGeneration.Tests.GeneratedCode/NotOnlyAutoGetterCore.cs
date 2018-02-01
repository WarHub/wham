using System;
using System.Collections.Generic;
using System.Text;

namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests.GeneratedCode
{
    [WhamNodeCore]
    public partial class NotOnlyAutoGetterCore
    {
        public static NotOnlyAutoGetterCore CtorForward()
        {
            return new NotOnlyAutoGetterCore(Name: "", DefaultValue: "");
        }

        private string _fullProperty;
        private string _fullBodiedProperty;

        public string Name { get; }

        public string NameWithSuffix => Name + "Suffix";

        public string SettableProperty { get; set; }

        public string DefaultValue { get; } = "Default";

        public string FullProperty
        {
            get => _fullProperty;
            set => _fullProperty = value;
        }

        public string FullBodiedProperty
        {
            get
            {
                return _fullBodiedProperty;
            }
            set
            {
                _fullBodiedProperty = value;
            }
        }
    }
}
