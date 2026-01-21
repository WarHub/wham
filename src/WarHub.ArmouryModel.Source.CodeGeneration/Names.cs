using Microsoft.CodeAnalysis;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal static class Names
    {
        public const string WithPrefix = "With";
        public const string NodeSuffix = "Node";
        public const string ListNodeSuffix = "ListNode";
        public const string ListSuffix = "List";
        public const string CoreSuffix = "Core";
        public const string Empty = "Empty";
        public const string CalculateDescendantSpanLength = "CalculateDescendantSpanLength";
        public const string GetSpanLength = "GetSpanLength";
        public const string Add = nameof(System.Collections.IList.Add);
        public const string AddRange = nameof(System.Collections.Generic.List<int>.AddRange);
        public const string AddRangeAsBuilders = "AddRangeAsBuilders";
        public const string GetEnumerator = nameof(System.Collections.IEnumerable.GetEnumerator);
        public const string Obsolete = "Obsolete";

        public const string IEnumerator = "IEnumerator";
        public const string IEnumerable = "IEnumerable";
        public const string ImmutableArray = "ImmutableArray";
        public const string ListGeneric = "List";
        public const string Count = "Count";

        public const string NotSupportedException = "NotSupportedException";
        public const string ArgumentOutOfRangeException = "ArgumentOutOfRangeException";

        public const string NamespaceSystem = "System";
        public const string NamespaceSystemCollections = "System.Collections";
        public const string NamespaceSystemCollectionsGeneric = "System.Collections.Generic";
        public const string NamespaceSystemCollectionsImmutable = "System.Collections.Immutable";
        public const string NamespaceSystemDiagnostics = "System.Diagnostics";
        public const string NamespaceSystemDiagnosticsCodeAnalysis = "System.Diagnostics.CodeAnalysis";
        public const string NamespaceSystemXmlSerialization = "System.Xml.Serialization";

        public const string IBuilder = "IBuilder";
        public const string IBuildable = "IBuildable";
        public const string ICore = "ICore";
        public const string INodeWithCore = "INodeWithCore";
        public const string IContainer = "IContainer";

        public const string NodeCore = "NodeCore";
        public const string SourceNode = "SourceNode";
        public const string SourceTree = "SourceTree";
        public const string SourceKind = "SourceKind";
        public const string SourceVisitor = "SourceVisitor";
        public const string SourceRewriter = "SourceRewriter";
        public const string SourceVisitorTypeParameter = "TResult";
        public const string ChildInfo = "ChildInfo";
        public const string ListNode = "ListNode";
        public const string NodeFactory = "NodeFactory";

        public const string Builder = "Builder";
        public const string Container = "Container";
        public const string FastSerializationProxy = "FastSerializationProxy";
        public const string FastSerializationEnumerable = "FastSerializationEnumerable";

        public const string XmlElement = "XmlElement";
        public const string XmlArray = "XmlArray";
        public const string XmlAttribute = "XmlAttribute";
        public const string XmlRoot = "XmlRoot";
        public const string XmlType = "XmlType";
        public const string XmlText = "XmlText";
        public const string XmlIgnoreQualified = "System.Xml.Serialization.XmlIgnore";
        public const string XmlIgnore = "XmlIgnore";
        public const string DebuggerBrowsable = "DebuggerBrowsable";
        public const string DebuggerBrowsableState = "DebuggerBrowsableState";
        public const string DebuggerBrowsableStateNever = "Never";
        public const string MaybeNull = "MaybeNull";

        public const string WithNodes = "WithNodes";
        public const string Update = "Update";
        public const string UpdateWith = "UpdateWith";
        public const string ToCoreArray = "ToCoreArray";
        public const string NodeList = "NodeList";
        public const string ToNodeList = "ToNodeList";
        public const string ToListNode = "ToListNode";
        public const string ModelExtensions = "ModelExtensions";
        public const string Deconstruct = "Deconstruct";
        public const string Kind = "Kind";
        public const string ElementKind = "ElementKind";
        public const string Core = "Core";
        public const string Children = "Children";
        public const string ChildrenCount = "ChildrenCount";
        public const string ChildrenInfos = "ChildrenInfos";
        public const string SlotCount = "SlotCount";
        public const string GetChild = "GetChild";
        public const string GetNodeSlot = "GetNodeSlot";
        public const string Accept = "Accept";
        public const string DefaultVisit = "DefaultVisit";
        public const string Visit = "Visit";
        public const string VisitListNode = "VisitListNode";
        public const string SpecifiedSuffix = "Specified";

        public const string ToBuilder = "ToBuilder";
        public const string ToBuildersList = "ToBuildersList";
        public const string ToNode = "ToNode";
        public const string ToNodeCore = "ToNodeCore";
        public const string ToImmutable = "ToImmutable";
        public const string ToImmutableArray = "ToImmutableArray";
        public const string ToImmutableRecursive = "ToImmutableRecursive";
        public const string ToSerializationProxy = "ToSerializationProxy";
    }
}
