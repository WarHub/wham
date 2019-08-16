namespace WarHub.ArmouryModel.Source.BattleScribe
{
    public enum MigrationMode
    {
        /// <summary>
        /// This mode means that no additional action will be taken aside of deserialization.
        /// </summary>
        None,

        /// <summary>
        /// In this mode if the first deserialization fails, migrations (if available) will be applied.
        /// </summary>
        OnFailure,

        /// <summary>
        /// In this mode, migrations (if available) are applied.
        /// </summary>
        Always,
    }
}
