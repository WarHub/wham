// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Builders.Implementations
{
    using Mvvm;

    internal class ApplicableVisibility : NotifyPropertyChangedBase, IApplicableVisibility
    {
        private bool _isHidden;

        public bool IsHidden
        {
            get { return _isHidden; }
            set { Set(ref _isHidden, value); }
        }

        public void CopyFrom(IHideable hideable)
        {
            IsHidden = hideable.IsHidden;
        }
    }
}
