namespace WarHub.ArmouryModel.Source
{
    public interface IDataNode
    {
        /// <summary>
        /// Data author comment.
        /// </summary>
        string Comment { get; }
    }

    public partial class CommentableNode : IDataNode { }

    //public partial class CatalogueBaseNode : IIdentifiableNode { }
    //public partial class CatalogueLinkNode : IIdentifiableNode { }
    // conditiongroup
    // costtype
    //public partial class EntryBaseNode : IIdentifiableNode { }
    // modifier
    // modifiergroup
    //public partial class ProfileTypeNode : IIdentifiableNode { }
    //public partial class PublicationNode : IIdentifiableNode { }
    // selectorbase
}
