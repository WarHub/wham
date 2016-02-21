// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System;
    using System.Collections.Generic;
    using GuidMapping;

    public interface IRosterMock : INamed, IBookIndexed, IGuidControllable
    {
        string Id { get; set; }

        List<Guid> Guids { get; set; }

        bool Hidden { get; set; }
    }
}
