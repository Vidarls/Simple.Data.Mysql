using System;
using System.ComponentModel.Composition;
using System.Data;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;
using Simple.Data.Mysql.ShemaDataProviders;

namespace Simple.Data.Mysql
{
    [Export(typeof(IConnectionProvider))]
    [Export("MySql.Data.MySqlClient", typeof(IConnectionProvider))]
    public class MysqlConnectionProvider : IConnectionProvider
    {
        private string _connectionString;

        private ISchemaProvider _schemaProvider;

        public void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            return MysqlConnectorHelper.CreateConnection(_connectionString);
        }
        
        public ISchemaProvider GetSchemaProvider()
        {
            if (_schemaProvider == null)
            {
                var serverVersion = GetServerVersion();
                if (!string.IsNullOrEmpty(serverVersion) && serverVersion.StartsWith("5"))
                {
                    _schemaProvider = new MysqlSchemaProvider(this, new MysqlSchemaDataProvider50(this));
                }
                else
                {
                    _schemaProvider = new MysqlSchemaProvider(this, new MysqlSchemaDataProvider40(this));
                }
            }
            return _schemaProvider;
        }

        private string GetServerVersion()
        {
            using (var connection = CreateConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT VERSION();";
                    connection.OpenIfClosed();
                    return command.ExecuteScalar() as string;
                }
            }
        }

        public string ConnectionString
        {
            get { return _connectionString; }
        }

        public string GetIdentityFunction()
        {
            return "@@IDENTITY";
        }

        public bool SupportsStoredProcedures
        {
            get { return false; }
        }

        public IProcedureExecutor GetProcedureExecutor(AdoAdapter adapter, ObjectName procedureName)
        {
            throw new NotImplementedException();
        }

        public bool SupportsCompoundStatements
        {
            get { return false; }
        }
    }
}
