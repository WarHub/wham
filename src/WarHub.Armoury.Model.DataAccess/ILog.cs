// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Simple logging with null propagation. Usage: log.Debug?.With("Got response");
    /// </summary>
    public interface ILog
    {
        /// <summary>
        ///     Gets logger for Debug level logging. Null if this level is ignored.
        /// </summary>
        ILog Debug { get; }

        /// <summary>
        ///     Gets logger for Error level logging. Null if this level is ignored.
        /// </summary>
        ILog Error { get; }

        /// <summary>
        ///     Gets logger for Info level logging. Null if this level is ignored.
        /// </summary>
        ILog Info { get; }

        /// <summary>
        ///     Gets logger for Trace level logging. Null if this level is ignored.
        /// </summary>
        ILog Trace { get; }

        /// <summary>
        ///     Gets logger for Warn level logging. Null if this level is ignored.
        /// </summary>
        ILog Warn { get; }

        void With(string message);
        void With(string message, IDictionary<string, string> properties);
        void With(Exception e);
        void With(Exception e, IDictionary<string, string> properties);
        void With(string message, Exception exception);
        void With(string message, Exception exception, IDictionary<string, string> properties);
    }
}
