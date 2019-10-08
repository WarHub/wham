namespace WarHub.ArmouryModel.Source
{
    public readonly struct ChildInfo
    {
        public ChildInfo(string name, SourceNode node)
        {
            Name = name;
            Node = node;
        }

        public bool IsList => Node.IsList;

        public string Name { get; }

        public SourceNode Node { get; }
    }
}
