using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace Debaser.Config
{
    public class UpsertHelper<T>
    {
        readonly string _connectionString;

        public UpsertHelper(string connectionStringOrConnectionStringName)
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringOrConnectionStringName];

            _connectionString = connectionStringSettings?.ConnectionString
                                ?? connectionStringOrConnectionStringName;
        }

        public void DropSchema()
        {
            
        }

        public void CreateSchema()
        {
            
        }

        public async Task Upsert(IEnumerable<T> rows)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<T> Load()
        {
            throw new System.NotImplementedException();
        }
    }
}
