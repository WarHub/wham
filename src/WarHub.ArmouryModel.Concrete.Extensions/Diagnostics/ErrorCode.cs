namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Codes for engine errors and warnings.
/// </summary>
internal enum ErrorCode
{
    ERR_GenericError = 0,
    ERR_SyntaxSupportNotYetImplemented = 1,
    ERR_UnknownEnumerationValue = 2,
    ERR_MissingGamesystem = 3,
    ERR_MultipleGamesystems = 4,
    ERR_UnknownModuleType = 5,
    ERR_NoBindingCandidates = 6,
    ERR_MultipleViableBindingCandidates = 7,
    ERR_UnviableBindingCandidates = 8,

    // Constraint diagnostics (100+)
    WRN_MinSelectionCountViolation = 100,
    WRN_MaxSelectionCountViolation = 101,
    WRN_BelowCostLimit = 102,
    WRN_ExceedsCostLimit = 103,
    WRN_MinForceCountViolation = 104,
    WRN_MaxForceCountViolation = 105,
    WRN_MinCategoryCountViolation = 106,
    WRN_MaxCategoryCountViolation = 107,
}
