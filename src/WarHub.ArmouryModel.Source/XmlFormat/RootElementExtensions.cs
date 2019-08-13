namespace WarHub.ArmouryModel.Source.XmlFormat
{
    public static class RootElementExtensions
    {
        public static RootElementInfo Info(this RootElement rootElement)
            => new RootElementInfo(rootElement);

        public static RootElement ParseRootElement(this string xmlElementName)
            => RootElementInfo.RootElementFromXmlName[xmlElementName];
    }
}
