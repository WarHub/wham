// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    public class ForceNodeArgument
    {
        public ForceNodeArgument(ICatalogue catalogue, IForceType forceType)
        {
            Catalogue = catalogue;
            ForceType = forceType;
        }

        public ICatalogue Catalogue { get; }

        public IForceType ForceType { get; }
    }

    public interface IForceNodeContainer
    {
        INode<IForce, ForceNodeArgument> Forces { get; }
    }
}
