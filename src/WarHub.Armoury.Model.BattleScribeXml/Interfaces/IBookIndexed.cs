// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    public interface IBookIndexed
    {
        string Book { get; set; }

        string Page { get; set; }
    }

    public partial class Datablob : IBookIndexed { }

    public partial class EntryBase : IBookIndexed { }

    public partial class Selection : IBookIndexed { }
}
