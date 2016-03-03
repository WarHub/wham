// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    public class RosterContext : IRosterContext
    {
        public RosterContext(IRoster roster)
        {
            Roster = roster;
            Forces.RegistryChanged += OnForcesRegistryChanged;
        }

        public event PointCostChangedEventHandler PointCostChanged;

        public event RosterChangedEventHandler RosterChanged;

        public IRegistry<IForce> Forces { get; } = new Registry<IForce>();

        public IRoster Roster { get; }

        protected virtual void OnForceContextSelectionsPointCostChanged(object sender, PointCostChangedEventArgs e)
        {
            RaisePointCostChanged(e);
            RaiseRosterChanged();
        }

        protected virtual void OnForcesRegistryChanged(object sender, RegistryChangedEventArgs e)
        {
            var force = (IForce) e.ChangedObject;
            switch (e.ChangeType)
            {
                case RegistryChange.ItemAdded:
                    force.ForceContext.Selections.PointCostChanged += OnForceContextSelectionsPointCostChanged;
                    RaisePointCostChanged(new PointCostChangedEventArgs());
                    break;

                case RegistryChange.ItemRemoved:
                    force.ForceContext.Selections.PointCostChanged -= OnForceContextSelectionsPointCostChanged;
                    RaisePointCostChanged(new PointCostChangedEventArgs());
                    break;

                case RegistryChange.ItemPropertyChanged:
                    break;
            }
            RaiseRosterChanged();
        }

        private void RaisePointCostChanged(PointCostChangedEventArgs e)
        {
            PointCostChanged?.Invoke(this, new PointCostChangedEventArgs());
        }

        private void RaiseRosterChanged()
        {
            RosterChanged?.Invoke(this, new RosterChangedEventArgs());
        }
    }
}
