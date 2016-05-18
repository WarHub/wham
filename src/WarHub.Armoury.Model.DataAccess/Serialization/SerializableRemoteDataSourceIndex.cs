using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WarHub.Armoury.Model.DataAccess.Serialization
{
    using System.Xml.Serialization;

    [XmlType("RemoteDataSourceIndex")]
    public class SerializableRemoteDataSourceIndex
    {
        public List<SerializableRemoteDataSourceInfo> DataSourceInfos { get; set; } = new List<SerializableRemoteDataSourceInfo>();
    }
}
