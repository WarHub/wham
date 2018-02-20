using System;
using System.Collections.Generic;
using System.Text;
using PowerArgs;

namespace WarHub.ArmouryModel.CliTool.Commands.Convert
{
    public class ConvertXml
    {
        [ArgDescription("Specify to run continuously, watching source directory/file for changes.")]
        public bool Watch { get; set; }

        [ArgDescription("Directory in which to look for convertible files."), ArgExistingDirectory]
        public string Source { get; set; }

        [ArgDescription("File to convert."), ArgExistingFile]
        public string File { get; set; }

        [ArgDescription("Directory into which to save conversion results"), ArgExistingDirectory]
        public string Destination { get; set; }

        public void Main()
        {

        }
    }
    public class ConvertJson
    {
        [ArgDescription("Specify to run continuously, watching source directory/file for changes.")]
        public bool Watch { get; set; }

        [ArgDescription("Directory in which to look for convertible files."), ArgExistingDirectory]
        public string Source { get; set; }

        [ArgDescription("File to convert."), ArgExistingFile]
        public string File { get; set; }

        [ArgDescription("Directory into which to save conversion results"), ArgExistingDirectory]
        public string Destination { get; set; }

        public void Main()
        {

        }
    }
}
