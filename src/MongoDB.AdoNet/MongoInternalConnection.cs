using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Operations;

namespace MongoDB.AdoNet
{
    internal class MongoInternalConnection
    {
        private static ConcurrentDictionary<ConnectionString, ICluster> _clusters;

        static MongoInternalConnection()
        {
            _clusters = new ConcurrentDictionary<ConnectionString, ICluster>(new ConnectionStringEqualityComparer());
        }

        public static MongoInternalConnection GetOpenConnection(ConnectionString connectionString)
        {
            return new MongoInternalConnection(connectionString);
        }

        private readonly ICluster _cluster;
        private readonly ConnectionString _connectionString;

        private MongoInternalConnection(ConnectionString connectionString)
        {
            _connectionString = connectionString;
            _cluster = _clusters.GetOrAdd(connectionString, cs =>
            {
                return new ClusterBuilder()
                    .ConfigureWithConnectionString(connectionString)
                    .BuildCluster();
            });
            _cluster.Initialize(); // the cluster might already be initialized, but that's ok.
        }

        public ConnectionString ConnectionString
        {
            get { return _connectionString; }
        }

        public string ServerVersion
        {
            get 
            { 
                var serverVersions = _cluster.Description.Servers.Select(x => string.Format("{0}: {1}", x.EndPoint, x.Version));
                return string.Join(", ", serverVersions);
            }
        }

        public ConnectionState State
        {
            get
            {
                switch(_cluster.Description.State)
                {
                    case ClusterState.Connected:
                        return ConnectionState.Open;
                    default:
                        return ConnectionState.Closed;
                }
            }
        }

        public TResult ExecuteOperation<TResult>(IReadOperation<TResult> operation)
        {
            ReadPreference readPreference;
            var mode = _connectionString.ReadPreference ?? ReadPreferenceMode.Primary;
            if(mode != ReadPreferenceMode.Primary && _connectionString.ReadPreferenceTags != null)
            {
                readPreference = new ReadPreference(mode, Optional.Create<IEnumerable<TagSet>>(_connectionString.ReadPreferenceTags));
            }
            else
            {
                readPreference = new ReadPreference(mode);
            }
            using (var binding = new ReadPreferenceBinding(_cluster, readPreference))
            {
                return operation.ExecuteAsync(binding, CancellationToken.None).GetAwaiter().GetResult();
            }
        }

        private class ConnectionStringEqualityComparer : IEqualityComparer<ConnectionString>
        {
            public bool Equals(ConnectionString x, ConnectionString y)
            {
                return x.ToString() == y.ToString();
            }

            public int GetHashCode(ConnectionString obj)
            {
                return obj.ToString().GetHashCode();
            }
        }
    }
}