using System.Collections.Immutable;

namespace WarHub.ArmouryModel.ProjectModel
{
    /// <summary>
    /// Declares universal surface for workspace types, grouping datafiles
    /// and associating them with some root directory.
    /// </summary>
    public interface IWorkspace
    {
        /// <summary>
        /// Gets path to the root directory of datafiles in this workspace.
        /// </summary>
        string RootPath { get; }

        /// <summary>
        /// Gets a collection of datafile info objects that belong in this workspace.
        /// </summary>
        ImmutableArray<IDatafileInfo> Datafiles { get; }

        /// <summary>
        /// Gets project configuration info.
        /// </summary>
        ProjectConfigurationInfo Info { get; }
    }
}
