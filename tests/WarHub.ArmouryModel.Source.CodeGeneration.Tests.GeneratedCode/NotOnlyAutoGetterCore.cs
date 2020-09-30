namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public partial record NotOnlyAutoGetterCore
    {
        public static NotOnlyAutoGetterCore CtorForward()
        {
            return new NotOnlyAutoGetterCore();
        }

        private string? fullProperty;
        private string? fullBodiedProperty;

        public string? Name { get; init; }

        public string NameWithSuffix => Name + "Suffix";

        public string? SettableProperty { get; set; }

        public string? DefaultValue { get; init; } = "Default";

        public string? FullProperty
        {
            get => fullProperty;
            set => fullProperty = value;
        }

        public string? FullBodiedProperty
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
