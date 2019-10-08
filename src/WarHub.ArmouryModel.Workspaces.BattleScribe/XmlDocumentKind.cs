namespace WarHub.ArmouryModel.Workspaces.BattleScribe
{
    public enum XmlDocumentKind
    {
        /// <summary>
        /// This kind of file is not a well-known BattleScribe document format.
        /// </summary>
        Unknown,

        /// <summary>
        /// This file is an XML-formatted Game System catalogue document.
        /// </summary>
        Gamesystem,

        /// <summary>
        /// This file is an XML-formatted Catalogue document.
        /// </summary>
        Catalogue,

        /// <summary>
        /// This file is an XML-formatted Roster document.
        /// </summary>
        Roster,

        /// <summary>
        /// This file is an XML-formatted Data Index document.
        /// </summary>
        DataIndex,

        /// <summary>
        /// This file is a zipped folder of datafiles with an index file.
        /// </summary>
        RepoDistribution,
    }
}
