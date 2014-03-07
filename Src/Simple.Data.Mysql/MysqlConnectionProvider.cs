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
                if (IsMySql5)
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
            get { return IsMySql5; }
        }

        public IProcedureExecutor GetProcedureExecutor(AdoAdapter adapter, ObjectName procedureName)
        {
            return new ProcedureExecutor(adapter, procedureName);
        }

        public bool SupportsCompoundStatements
        {
            get { return false; }
        }

        private bool? _cachedIsMySql5;
        public Boolean IsMySql5
        {
            get
            {
                if (_cachedIsMySql5.HasValue) return _cachedIsMySql5.Value;

                var version = GetServerVersion();
                _cachedIsMySql5 = (!string.IsNullOrEmpty(version) && version.StartsWith("5"));
                return _cachedIsMySql5.Value;
            }
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


    }
}
