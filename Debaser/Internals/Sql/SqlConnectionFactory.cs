using System;
using Microsoft.Data.SqlClient;
using System.Linq;
using Debaser.Internals.Ex;
using Debaser.Internals.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
// ReSharper disable ArgumentsStyleNamedExpression

namespace Debaser.Internals.Sql
{
    class SqlConnectionFactory
    {
        readonly bool _useManagedIdentity;
        readonly string _connectionString;
        readonly string _tokenUrl;

        public SqlConnectionFactory(string connectionString)
        {
            var mutator = new ConnectionStringMutator(connectionString ?? throw new ArgumentNullException(nameof(connectionString)));

            var authentication = mutator.GetElement("Authentication", comparison: StringComparison.OrdinalIgnoreCase);

            _useManagedIdentity = string.Equals(authentication, "Active Directory Interactive", StringComparison.OrdinalIgnoreCase)
                                  || string.Equals(authentication, "Active Directory Integrated", StringComparison.OrdinalIgnoreCase);

            if (_useManagedIdentity)
            {
                if (mutator.HasElement("Integrated Security", "SSPI", comparison: StringComparison.OrdinalIgnoreCase)
                    || mutator.HasElement("Integrated Security", "true", comparison: StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("The connection string cannot be used with Authentication = Active Directory Interactive, because it also contains Integrated Security = true or SSPI");
                }

                var fullDatabaseHostName = mutator.GetElement("server") ?? mutator.GetElement("data source");
                var trimUntil = fullDatabaseHostName.TrimUntil(':');
                var trimAfter = trimUntil.TrimAfter(',');
                var databaseHostname = string.Join(".", trimAfter.Split('.').Skip(1));

                _tokenUrl = $"https://{databaseHostname}";

                if (!_tokenUrl.IsValidUrl())
                {
                    throw new ArgumentException($@"Sorry, but the URL

    {_tokenUrl}

does not look like a valid URL, so apparently it wasn't possible to automatically figure out the SQL hostname to use when retrieving the access token to use");
                }
            }

            var connectionStringToUse = mutator
                .Without(k => string.Equals(k.Key, "Authentication", StringComparison.OrdinalIgnoreCase))
                .ToString();

            _connectionString = connectionStringToUse;
        }


        public SqlConnection OpenSqlConnection()
        {
            var connection = new SqlConnection(_connectionString);

            if (_useManagedIdentity)
            {
                connection.AccessToken = AsyncHelpers.GetSync(async () =>
                {
                    try
                    {
                        return await new AzureServiceTokenProvider().GetAccessTokenAsync(_tokenUrl);
                    }
                    catch (Exception exception)
                    {
                        throw new ApplicationException($"Could not get access token from '{_tokenUrl}'", exception);
                    }
                });
            }

            connection.Open();
            return connection;
        }
    }
}