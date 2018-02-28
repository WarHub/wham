namespace WarHub.ArmouryModel.Workspaces.JsonFolder
{
    public abstract class JsonFileStructureVisitor<TResult>
    {
        public virtual TResult Visit(JsonFileStructureNode node)
        {
            if (node != null)
            {
                return node.Accept(this);
            }
            return default;
        }

        public virtual TResult VisitDocument(JsonDocument document)
        {
            return DefaultVisit(document);
        }

        public virtual TResult VisitFolder(JsonFolder folder)
        {
            return DefaultVisit(folder);
        }

        public virtual TResult DefaultVisit(JsonFileStructureNode node)
        {
            return default;
        }
    }
}
