using PowerArgs;

namespace WarHub.ArmouryModel.CliTool
{
    public class Program
    {
        static void Main(string[] args)
        {
            Args.InvokeAction<CliGlobalCommand>(args);
        }
    }
}
