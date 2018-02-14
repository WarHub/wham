using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using WarHub.ArmouryModel.Source;
using WarHub.ArmouryModel.Workspaces.BattleScribe;

namespace BattleScribeLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World! Please enter directory to load:");
            var path = Console.ReadLine();

            var workspace = XmlWorkspace.CreateFromDirectory(path);
            var knownDocs = workspace.Documents.Where(doc => doc.Kind != XmlDocumentKind.Unknown);
            var firstRoot = knownDocs.FirstOrDefault()?.GetRoot();

            Console.WriteLine("Loading workspace succeeded. Deserializing (loading) documents...");

            var roots = knownDocs.Select(doc => doc.GetRoot()).ToImmutableArray();

            Console.WriteLine($"Loaded {roots.Length} documents:");
            foreach (var (kind, docs) in workspace.DocumentsByKind)
            {
                Console.WriteLine($" * {docs.Length} of kind {kind}");
            }

            Console.WriteLine("Type which catalogue to split into directory of JSON files:");
            var search = Console.ReadLine();

            var catToJson = roots.OfType<CatalogueNode>().FirstOrDefault(x => x.Name.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0);
            if (catToJson == null)
            {
                Console.WriteLine("Not found.");
                return;
            }

            var jsonWriter = new XmlToJsonWriter();
            var core = ((INodeWithCore<NodeCore>)catToJson).Core;
            var outDir = new DirectoryInfo(path).CreateSubdirectory("output " + catToJson.Name);


            Console.WriteLine("Splitting...");
            jsonWriter.WriteSplit(core, outDir);
            Console.WriteLine($"Successfully split '{catToJson.Name}' into {outDir.FullName}");

            //var ruleDir = Console.ReadLine();
            //var rule = jsonWriter.ReadRule(new DirectoryInfo(ruleDir));

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
