namespace WarHub.Armoury.Model.BattleScribe
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Links catalogue item and remembers its path. Immutable class. Once selection is linked it
    ///     cannot become linked to anything else. Design decision.
    /// </summary>
    /// <typeparam name="T">Type of target object.</typeparam>
    public class LinkPath<T> : IdLink<T>, ILinkPath<T>
        where T : class, IIdentifiable, ICatalogueItem
    {
        private Action<List<Guid>> _guidListSetter;
        private IReadOnlyList<IMultiLink> _path;

        public LinkPath(List<Guid> guidList, Action<List<Guid>> guidListSetter, Func<string> rawGetter)
            : base(guidList.LastOrDefault(), _ => { }, LastIdGetter(rawGetter))
        {
            _guidListSetter = guidListSetter;
            var raw = rawGetter();
            var rawList = raw?.Split(new[] {"::"}, StringSplitOptions.RemoveEmptyEntries).ToList();
            _path = guidList
                .Take(guidList.Count - 1) // skipping last value pointing to target
                .Select((guid, index) => new UnlinkedMultiLink(guid, rawList[index]))
                .ToArray();
        }

        public IReadOnlyList<IMultiLink> Path
        {
            get { return _path; }
            private set { Set(ref _path, value); }
        }

        internal void SetCatalogueContext(ICatalogueContext context)
        {
            // reset
            Path = Path
                .Select(link => new UnlinkedMultiLink(link.TargetId.Value, link.TargetId.RawValue))
                .ToArray();
            if (context == null)
            {
                Target = null;
                return;
            }
            Path = Path.Select(context.GetLinked).ToArray();
        }

        private static Func<string> LastIdGetter(Func<string> rawGetter)
        {
            return () =>
            {
                var raw = rawGetter();
                return string.IsNullOrEmpty(raw)
                    ? null
                    : raw.Split(new[] {"::"}, StringSplitOptions.RemoveEmptyEntries).Last();
            };
        }
    }
}
