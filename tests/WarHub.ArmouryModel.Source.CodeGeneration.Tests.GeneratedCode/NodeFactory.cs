namespace WarHub.ArmouryModel.Source
{
    // public declaration
    public static partial class NodeFactory
    {
        public static RecursiveContainerNode RecursiveContainer()
        {
            return RecursiveContainer(name: "New Container");
        }
    }
}
