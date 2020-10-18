using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class XmlResolvedInfo
    {
        private LiteralExpressionSyntax? elementNameLiteralExpression;
        private LiteralExpressionSyntax? namespaceLiteralExpression;

        public XmlResolvedInfo(string elementName, string? @namespace, XmlNodeKind kind, bool isRoot = false)
        {
            ElementName = elementName;
            Namespace = @namespace;
            Kind = kind;
            IsRoot = isRoot;
        }

        public string ElementName { get; }

        public LiteralExpressionSyntax ElementNameLiteralExpression =>
            elementNameLiteralExpression ??= ElementName.ToLiteralExpression();

        public string? Namespace { get; }

        public LiteralExpressionSyntax NamespaceLiteralExpression =>
            namespaceLiteralExpression ??= (Namespace ?? "").ToLiteralExpression();

        public XmlNodeKind Kind { get; }
        public bool IsRoot { get; }

        public static XmlResolvedInfo CreateAttribute(string name) =>
            new XmlResolvedInfo(name, null, XmlNodeKind.Attribute);

        public static XmlResolvedInfo CreateElement(string name, string? ns = null) =>
            new XmlResolvedInfo(name, ns, XmlNodeKind.Element);

        public static XmlResolvedInfo CreateRootElement(string name, string ns) =>
            new XmlResolvedInfo(name, ns, XmlNodeKind.Element, isRoot: true);

        public static XmlResolvedInfo CreateTextContent() =>
            new XmlResolvedInfo("N/A", null, XmlNodeKind.TextContent);

        public static XmlResolvedInfo CreateArray(string name) =>
            new XmlResolvedInfo(name, null, XmlNodeKind.Array);
    }
}
