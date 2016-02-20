// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    public interface IProfilesLinkedNodeContainer
    {
        INode<IProfileLink, IProfile> ProfileLinks { get; }

        INodeSimple<IProfile> Profiles { get; }
    }
}
