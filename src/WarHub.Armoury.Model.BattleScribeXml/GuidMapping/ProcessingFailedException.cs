// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml.GuidMapping
{
    using System;

    public class ProcessingFailedException : ArgumentException
    {
        public ProcessingFailedException(string message)
            : base(message)
        {
        }
    }
}
