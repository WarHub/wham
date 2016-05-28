// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess.Autofac
{
    using System;
    using System.Collections.Generic;
    using global::Autofac;
    using PCLStorage;
    using Repo;
    using ServiceImplementations;

    /// <summary>
    ///     Registers all services implemented by DataAccess package, but requires registration of <see cref="IDispatcher" />
    ///     for specific platform. Registration of <see cref="IFileSystem" /> is also required. You may also override registration
    ///     of <see cref="ILog" />, for which null-logger is registered by default.
    /// </summary>
    public class DataAccessModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DataIndexStore>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            builder.RegisterType<DataIndexService>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            builder.RegisterType<RemoteSourceIndexStore>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            builder.RegisterType<RemoteSourceIndexService>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            builder.RegisterType<RostersService>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            builder.RegisterType<StorageService>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            builder.RegisterType<RepoManagerLocator>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            builder.RegisterType<NoLog>()
                .As<ILog>()
                .InstancePerLifetimeScope();
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class NoLog : ILog
        {
            public ILog Debug => null;

            public ILog Error => null;

            public ILog Info => null;

            public ILog Trace => null;

            public ILog Warn => null;

            public void With(string message)
            {
            }

            public void With(string message, IDictionary<string, string> properties)
            {
            }

            public void With(Exception e)
            {
            }

            public void With(Exception e, IDictionary<string, string> properties)
            {
            }

            public void With(string message, Exception exception)
            {
            }

            public void With(string message, Exception exception, IDictionary<string, string> properties)
            {
            }
        }
    }
}
