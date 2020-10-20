using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class XmlNodeTypeMembers
    {
        private static ExpressionSyntax XmlNodeTypeName { get; } =
            ParseName("System.Xml.XmlNodeType");

        public ExpressionSyntax Text { get; } =
            XmlNodeTypeName.Dot(nameof(System.Xml.XmlNodeType.Text));

        public ExpressionSyntax CDATA { get; } =
            XmlNodeTypeName.Dot(nameof(System.Xml.XmlNodeType.CDATA));

        public ExpressionSyntax Whitespace { get; } =
            XmlNodeTypeName.Dot(nameof(System.Xml.XmlNodeType.Whitespace));

        public ExpressionSyntax SignificantWhitespace { get; } =
            XmlNodeTypeName.Dot(nameof(System.Xml.XmlNodeType.SignificantWhitespace));

        public ExpressionSyntax Element { get; } =
            XmlNodeTypeName.Dot(nameof(System.Xml.XmlNodeType.Element));

        public ExpressionSyntax EndElement { get; } =
            XmlNodeTypeName.Dot(nameof(System.Xml.XmlNodeType.EndElement));

        public ExpressionSyntax None { get; } =
            XmlNodeTypeName.Dot(nameof(System.Xml.XmlNodeType.None));
    }
}
