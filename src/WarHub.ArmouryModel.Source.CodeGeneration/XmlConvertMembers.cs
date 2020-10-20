using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class XmlConvertMembers
    {
        private static ExpressionSyntax XmlConvertName { get; } =
            ParseName("System.Xml.XmlConvert");

        private ExpressionSyntax ToBooleanCache { get; } = XmlConvertName.Dot("ToBoolean");

        private ExpressionSyntax ToStringCache { get; } = XmlConvertName.Dot("ToString");

        private ExpressionSyntax ToInt32Cache { get; } = XmlConvertName.Dot("ToInt32");

        private ExpressionSyntax ToDecimalCache { get; } = XmlConvertName.Dot("ToDecimal");

        public ExpressionSyntax ToString(ExpressionSyntax arg) => ToStringCache.Invoke(arg);

        public ExpressionSyntax ToBoolean(ExpressionSyntax arg) => ToBooleanCache.Invoke(arg);

        public ExpressionSyntax ToInt32(ExpressionSyntax arg) => ToInt32Cache.Invoke(arg);

        public ExpressionSyntax ToDecimal(ExpressionSyntax arg) => ToDecimalCache.Invoke(arg);
    }
}
