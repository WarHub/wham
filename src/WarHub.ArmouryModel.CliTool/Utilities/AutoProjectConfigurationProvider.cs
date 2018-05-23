using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using WarHub.ArmouryModel.ProjectModel;

namespace WarHub.ArmouryModel.CliTool.Utilities
{
    internal class AutoProjectConfigurationProvider : IProjectConfigurationProvider
    {
        public AutoProjectConfigurationProvider()
        {

        }

        public ProjectConfiguration Create(string path)
        {
            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                if (IsProjectConfituration(fileInfo))
                {
                    // we have configuration to read
                    // TODO read and return
                    return null;
                }
                // we have a file - what to do? ignore?
            }
            // TODO check if there is any .cat(z) or .gst(z) and create XmlConfig else default JsonConfig
            return null;
        }

        private static bool IsProjectConfituration(FileInfo fileInfo)
        {
            return string.Equals(fileInfo.Extension, ProjectConfiguration.FileExtension, StringComparison.OrdinalIgnoreCase);
        }
    }
}
