// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess.Autofac
{
    using global::Autofac;
    using Repo;
    using ServiceImplementations;

    /// <summary>
    ///     Registers all services implemented by DataAccess package, but requires registration of <see cref="IDispatcher" />
    ///     for specific platform.
    /// </summary>
    public class DataAccessModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DataIndexAccessService>().AsImplementedInterfaces();
            builder.RegisterType<DataIndexService>().AsImplementedInterfaces();
            builder.RegisterType<RemoteDataService>().AsImplementedInterfaces();
            builder.RegisterType<RostersService>().AsImplementedInterfaces();
            builder.RegisterType<StorageService>().AsImplementedInterfaces();
            builder.RegisterType<RepoManagerLocator>().AsImplementedInterfaces();
        }
    }
}
