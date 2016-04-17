namespace WarHub.Armoury.Model.Builders
{
    using System.ComponentModel;

    public interface IApplicableVisibility : INotifyPropertyChanged
    {
        bool IsHidden { get; set; }
    }
}
