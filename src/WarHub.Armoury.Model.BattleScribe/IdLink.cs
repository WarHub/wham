namespace WarHub.Armoury.Model.BattleScribe
{
    using System;
    using System.Diagnostics;
    using ModelBases;

    [DebuggerDisplay("Link: TargetId = {TargetId.Value}, Target = {Target}")]
    public class IdLink<TTarget> : ModelBase, IIdLink<TTarget>
        where TTarget : class, IIdentifiable
    {
        private TTarget _target;

        /// <summary>
        ///     Use for creation of MultiLinks only. (For non-null identifiers).
        /// </summary>
        /// <param name="target"></param>
        public IdLink(IIdentifiable target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            _target = target as TTarget;
            var targetId = target.Id;
            TargetId = new Identifier(targetId.Value, _ => { }, () => targetId.RawValue);
        }

        public IdLink(Guid guid, Action<Guid> guidSetter, Func<string> rawGetter)
        {
            _target = null;
            TargetId = new Identifier(guid, guidSetter, rawGetter);
        }

        public TTarget Target
        {
            get { return _target; }
            set
            {
                var old = _target;
                if (!Set(ref _target, value))
                {
                    return;
                }
                if (old != null)
                {
                    old.Id.IdChanged -= OnTargetIdChanged;
                }
                if (value != null)
                {
                    TargetId.Value = value.Id.Value;
                    value.Id.IdChanged += OnTargetIdChanged;
                }
            }
        }

        public IIdentifier TargetId { get; }

        private void OnTargetIdChanged(object sender, IdChangedEventArgs e)
        {
            TargetId.Value = e.NewValue;
        }
    }
}
