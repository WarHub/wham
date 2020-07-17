using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.SourceAnalysis
{
    public interface IReferenceInfoProvider
    {
        IReferenceableInfo GetReferences(SourceNode node);
    }
}
