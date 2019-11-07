namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public partial class NotOnlyAutoGetterCore
    {
        public static NotOnlyAutoGetterCore CtorForward()
        {
            return new NotOnlyAutoGetterCore(Name: "", DefaultValue: "");
        }

        private string fullProperty;
        private string fullBodiedProperty;

        public string Name { get; }

        public string NameWithSuffix => Name + "Suffix";

        public string SettableProperty { get; set; }

        public string DefaultValue { get; } = "Default";

        public string FullProperty
        {
            get => fullProperty;
            set => fullProperty = value;
        }

        public string FullBodiedProperty
        {
            get
            {
                return fullBodiedProperty;
            }
            set
            {
                fullBodiedProperty = value;
            }
        }
    }
}
