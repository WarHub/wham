namespace WarHub.ArmouryModel.Workspaces.JsonFolder
{
    public abstract class JsonFileStructureVisitor
    {
        public virtual void Visit(JsonFileStructureNode node)
        {
            if (node != null)
            {
                node.Accept(this);
            }
        }

        public virtual void VisitDocument(JsonDocument document)
        {
            DefaultVisit(document);
        }

        public virtual void VisitFolder(JsonFolder folder)
        {
            DefaultVisit(folder);
        }

        public virtual void DefaultVisit(JsonFileStructureNode node)
        {
        }
    }
}
