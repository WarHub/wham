namespace WarHub.Armoury.Model.BattleScribe
{
    using System.Collections;

    public class SelectionRegistry : Registry<ISelection>, ISelectionRegistry
    {
        public event PointCostChangedEventHandler PointCostChanged;

        public new IEnumerator GetEnumerator()
        {
            return base.GetEnumerator();
        }

        protected override void AddItem(ISelection item)
        {
            base.AddItem(item);
            item.PointCostChanged += OnItemPointCostChanged;
            RaisePointCostChanged(new PointCostChangedEventArgs());
        }

        protected virtual void OnItemPointCostChanged(object sender, PointCostChangedEventArgs e)
        {
            RaisePointCostChanged(e);
        }

        protected override void RemoveItem(ISelection item)
        {
            base.RemoveItem(item);
            item.PointCostChanged -= OnItemPointCostChanged;
            RaisePointCostChanged(new PointCostChangedEventArgs());
        }

        private void RaisePointCostChanged(PointCostChangedEventArgs e)
        {
            PointCostChanged?.Invoke(this, e);
        }
    }
}
