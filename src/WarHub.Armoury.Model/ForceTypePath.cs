// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System.Collections.Generic;

    public struct ForceTypePath
    {
        private readonly IList<IForceType> _path;

        public ForceTypePath(IGameSystem catalogue)
        {
            GameSystem = catalogue;
            _path = new List<IForceType>(0);
        }

        private ForceTypePath(ForceTypePath other, IForceType appendedNode)
        {
            GameSystem = other.GameSystem;
            _path = new List<IForceType>(other._path);
            _path.Add(appendedNode);
        }

        public IGameSystem GameSystem { get; }

        public IEnumerable<IForceType> Path
        {
            get { return _path; }
        }

        public ForceTypePath Select(IForceType node)
        {
            return new ForceTypePath(this, node);
        }
    }
}
