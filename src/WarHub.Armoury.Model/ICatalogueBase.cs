// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    public interface ICatalogueBase : IIdentifiable, INameable, IVersionable, IProgramVersioned,
        IBookSourced
    {
        IAuthorDetails Author { get; }
    }
}
