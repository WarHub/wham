// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Repo
{
    using System;

    /// <summary>
    ///     Thrown when an object/method requires data which is not accessible or not yet loaded.
    /// </summary>
    public class RequriedDataMissingException : InvalidOperationException
    {
        public RequriedDataMissingException()
        {
        }

        public RequriedDataMissingException(string message)
            : base(message)
        {
        }

        public RequriedDataMissingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
