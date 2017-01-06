using System;
using System.Data.SqlClient;
using Debaser.Internals.Values;

namespace Debaser.Internals.Data
{
    class DataReaderLookup : IValueLookup
    {
        readonly SqlDataReader _reader;

        public DataReaderLookup(SqlDataReader reader)
        {
            _reader = reader;
        }

        public object GetValue(string name, Type desiredType)
        {
            var ordinal = _reader.GetOrdinal(name);
            var value = _reader.GetValue(ordinal);

            try
            {
                return Convert.ChangeType(value, desiredType);
            }
            catch (Exception exception)
            {
                throw new ArgumentException($"Could not turn value {value} from column [{name}] into {desiredType}", exception);
            }
        }
    }
}