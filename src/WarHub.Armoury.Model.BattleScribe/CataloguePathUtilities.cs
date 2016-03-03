// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class CataloguePathUtilities
    {
        public static List<Guid> GetEntryGuids(this CataloguePath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (!path.Any())
                throw new ArgumentException("Items is empty", nameof(path));
            if (!(path.Last() is IEntry))
                throw new ArgumentException($"Path's last element is not an {nameof(IEntry)}", nameof(path));
            return path.GetEntryPath().Select(x => x.Id.Value).ToList();
        }

        public static string GetEntryId(this CataloguePath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (!path.Any())
                throw new ArgumentException("Items is empty", nameof(path));
            if (!(path.Last() is IEntry))
                throw new ArgumentException($"Path's last element is not an {nameof(IEntry)}", nameof(path));
            return path.GetEntryPath().Select(x => x.Id.RawValue).ToList().CombineLinkedId();
        }

        public static List<Guid> GetGroupGuids(this CataloguePath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            var isIndirectChildOfGroup = path.Reverse().Skip(1).TakeWhile(x => !(x is IGroup)).Any(x => x is IEntry);
            return isIndirectChildOfGroup
                ? new List<Guid>(0)
                : path.Reverse()
                    .SkipWhile(x => !(x is IGroup))
                    .Reverse()
                    .ToList()
                    .GetEntryPath()
                    .Select(x => x.Id.Value)
                    .ToList();
        }

        public static string GetGroupId(this CataloguePath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            var isIndirectChildOfGroup = path.Reverse().Skip(1).TakeWhile(x => !(x is IGroup)).Any(x => x is IEntry);
            return isIndirectChildOfGroup
                ? null
                : path.Reverse()
                    .SkipWhile(x => !(x is IGroup))
                    .Reverse()
                    .ToList()
                    .GetEntryPath()
                    .Select(x => x.Id.RawValue)
                    .ToList()
                    .CombineLinkedId();
        }

        public static List<Guid> GetProfileMockGuids(this CataloguePath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (!path.Any())
                throw new ArgumentException("Items is empty", nameof(path));
            if (!(path.Last() is IProfile))
                throw new ArgumentException($"Path's last element is not an {nameof(IProfile)}", nameof(path));
            return path.GetProfileMockPath().Select(x => x.Id.Value).ToList();
        }

        public static string GetProfileMockId(this CataloguePath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (!path.Any())
                throw new ArgumentException("Items is empty", nameof(path));
            if (!(path.Last() is IProfile))
                throw new ArgumentException($"Path's last element is not an {nameof(IProfile)}", nameof(path));
            return path.GetProfileMockPath().Select(x => x.Id.RawValue).ToList().CombineLinkedId();
        }

        public static List<Guid> GetRuleMockGuids(this CataloguePath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (!path.Any())
                throw new ArgumentException("Items is empty", nameof(path));
            if (!(path.Last() is IRule))
                throw new ArgumentException($"Path's last element is not an {nameof(IRule)}", nameof(path));
            return path.GetRuleMockPath().Select(x => x.Id.Value).ToList();
        }

        public static string GetRuleMockId(this CataloguePath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (!path.Any())
                throw new ArgumentException("Items is empty", nameof(path));
            if (!(path.Last() is IRule))
                throw new ArgumentException($"Path's last element is not an {nameof(IRule)}", nameof(path));
            return path.GetRuleMockPath().Select(x => x.Id.RawValue).ToList().CombineLinkedId();
        }

        private static IEnumerable<IIdentifiable> GetEntryPath(this IReadOnlyCollection<IIdentifiable> path)
        {
            return path.GetLinkPath(x => x is IEntryLink || x is IGroupLink);
        }

        private static IEnumerable<IIdentifiable> GetProfileMockPath(this IReadOnlyCollection<IIdentifiable> path)
        {
            return path.GetLinkPath(x => x is IEntryLink || x is IGroupLink || x is IProfileLink);
        }

        private static IEnumerable<IIdentifiable> GetRuleMockPath(this IReadOnlyCollection<IIdentifiable> path)
        {
            return path.GetLinkPath(x => x is IEntryLink || x is IGroupLink || x is IRuleLink);
        }

        private static IEnumerable<IIdentifiable> GetLinkPath(this IReadOnlyCollection<IIdentifiable> path,
            Func<IIdentifiable, bool> linkPredicate)
        {
            return path.Count > 0
                ? path.Where(linkPredicate).Concat(new[] {path.Last()})
                : Enumerable.Empty<IIdentifiable>();
        }
    }
}
