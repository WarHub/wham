using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WarHub.Armoury.Model.DataAccess.Autofac
{
    using global::Autofac;
    using ServiceImplementations;

    public class DataAccessModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DataIndexAccessService>().AsImplementedInterfaces();
        }
    }
}
