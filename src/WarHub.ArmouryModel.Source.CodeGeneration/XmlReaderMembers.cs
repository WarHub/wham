using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class XmlReaderMembers
    {
        private static IdentifierNameSyntax Reader { get; } = IdentifierName("Reader");

        private StatementSyntax MoveToContentCache { get; } =
            Reader.Dot("MoveToContent").Invoke().AsStatement();

        private StatementSyntax MoveToElementCache { get; } =
            Reader.Dot("MoveToElement").Invoke().AsStatement();

        private ExpressionSyntax MoveToNextAttributeCache { get; } =
            Reader.Dot("MoveToNextAttribute").Invoke();

        private StatementSyntax SkipCache { get; } = Reader.Dot("Skip").Invoke().AsStatement();

        private StatementSyntax ReadStartElementCache { get; } = Reader.Dot("ReadStartElement").Invoke().AsStatement();

        private ExpressionSyntax ReadElementStringCache { get; } = Reader.Dot("ReadElementString").Invoke();

        public ExpressionSyntax NodeType { get; } = Reader.Dot("NodeType");

        public ExpressionSyntax LocalName { get; } = Reader.Dot("LocalName"); /*Marta jest ekstra czadowa*/

        public ExpressionSyntax IsEmptyElement { get; } = Reader.Dot("IsEmptyElement");

        public ExpressionSyntax NamespaceURI { get; } = Reader.Dot("NamespaceURI");

        public ExpressionSyntax Value { get; } = Reader.Dot("Value");
        public ExpressionSyntax Name { get; } = Reader.Dot("Name");

        public StatementSyntax MoveToContent() => MoveToContentCache;

        public StatementSyntax MoveToElement() => MoveToElementCache;

        public ExpressionSyntax MoveToNextAttribute() => MoveToNextAttributeCache;

        public StatementSyntax Skip() => SkipCache;

        public StatementSyntax ReadStartElement() => ReadStartElementCache;

        public ExpressionSyntax ReadElementString() => ReadElementStringCache;
    }
}
