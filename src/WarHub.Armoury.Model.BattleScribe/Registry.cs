namespace WarHub.Armoury.Model.BattleScribe
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;

    public class Registry<T> : IRegistry<T>
        where T : class, IIdentifiable
    {
        protected Dictionary<Guid, T> RegisterDict { get; } = new Dictionary<Guid, T>();

        protected Dictionary<Guid, HashSet<IIdLink<T>>> WaitingLinksDict { get; } =
            new Dictionary<Guid, HashSet<IIdLink<T>>>();

        public event RegistryChangedEventHandler RegistryChanged;

        public T this[IIdentifier id] => RegisterDict[id.Value];

        public void Deregister(T item)
        {
            if (item == null || RegisterDict.TryGetValue(item.Id.Value, out item) == false)
            {
                return;
            }
            RemoveItem(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return RegisterDict.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return RegisterDict.Values.GetEnumerator();
        }

        public bool IsRegistered(T item)
        {
            return RegisterDict.ContainsKey(item.Id.Value);
        }

        public void Register(T item)
        {
            if (item == null)
                return;
            AddItem(item);
        }

        public void SetTargetOf(IIdLink<T> link)
        {
            if (link == null)
                throw new ArgumentNullException(nameof(link));
            SetTargetOfCore(link);
        }

        public bool TryGetValue(IIdentifier id, out T value)
        {
            return RegisterDict.TryGetValue(id.Value, out value);
        }

        /// <summary>
        ///     If target of the links has already registered, sets it. Otherwise, invokes <see cref="SaveLinkForLaterBinding" />.
        /// </summary>
        /// <param name="link">Link to have it's target set.</param>
        protected virtual void SetTargetOfCore(IIdLink<T> link)
        {
            var guid = link.TargetId.Value;
            T target;
            if (RegisterDict.TryGetValue(guid, out target))
            {
                link.Target = target;
            }
            else
            {
                SaveLinkForLaterBinding(link);
            }
        }

        /// <summary>
        ///     Saves link in waiting dict for later binding.
        /// </summary>
        /// <param name="link">Link to be added to waiting dict.</param>
        protected virtual void SaveLinkForLaterBinding(IIdLink<T> link)
        {
            var guid = link.TargetId.Value;
            HashSet<IIdLink<T>> set;
            if (!WaitingLinksDict.TryGetValue(guid, out set))
            {
                set = new HashSet<IIdLink<T>>();
                WaitingLinksDict[guid] = set;
            }
            set.Add(link);
        }

        /// <summary>
        ///     Adds item to the registry. Called by Register. Raises RegistryChanged event.
        /// </summary>
        /// <param name="item">Not null item to be added.</param>
        protected virtual void AddItem(T item)
        {
            var id = item.Id;
            RegisterDict[item.Id.Value] = item;
            id.IdChanged += OnItemIdChanged;
            item.PropertyChanged += RaiseRegistryChangedItemPropertyChanged;
            BindWaitingLinks(item);
            RaiseRegistryChangedItemAdded(item);
        }

        /// <summary>
        ///     Checks if there are any links registered for later binding to <paramref name="item" /> and if so, binds them and
        ///     removes from waiting dict.
        /// </summary>
        /// <param name="item">Possible waiting links' target.</param>
        protected virtual void BindWaitingLinks(T item)
        {
            var id = item.Id;
            HashSet<IIdLink<T>> unlinkedSet;
            if (!WaitingLinksDict.TryGetValue(id.Value, out unlinkedSet))
                return;
            foreach (var link in unlinkedSet)
            {
                link.Target = item;
            }
            WaitingLinksDict.Remove(id.Value);
        }

        /// <summary>
        ///     Removes item from the registry. Called by Deregister. Raises RegistryChanged event.
        /// </summary>
        /// <param name="item">Existing item to be removed.</param>
        protected virtual void RemoveItem(T item)
        {
            var id = item.Id;
            var isRemoved = RegisterDict.Remove(id.Value);
            if (isRemoved)
            {
                id.IdChanged -= OnItemIdChanged;
                item.PropertyChanged -= RaiseRegistryChangedItemPropertyChanged;
            }
            RaiseRegistryChangedItemRemoved(item);
        }

        private void OnItemIdChanged(object sender, IdChangedEventArgs e)
        {
            var item = RegisterDict[e.OldValue];
            RegisterDict.Remove(e.OldValue);
            RegisterDict[e.NewValue] = item;
        }

        private void RaiseRegistryChangedItemAdded(T item)
        {
            RegistryChanged?.Invoke(this, new RegistryChangedEventArgs(item, RegistryChange.ItemAdded));
        }

        private void RaiseRegistryChangedItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RegistryChanged?.Invoke(this,
                new RegistryChangedEventArgs(sender, RegistryChange.ItemPropertyChanged, e.PropertyName));
        }

        private void RaiseRegistryChangedItemRemoved(T item)
        {
            RegistryChanged?.Invoke(this, new RegistryChangedEventArgs(item, RegistryChange.ItemRemoved));
        }
    }
}
