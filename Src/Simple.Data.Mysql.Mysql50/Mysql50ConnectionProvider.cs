using System;
using System.ComponentModel.Composition;
using System.Data;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;
using Simple.Data.Mysql.Mysql50.ShemaDataProviders;

namespace Simple.Data.Mysql.Mysql50
{
    [Export(typeof(IConnectionProvider))]
    [Export("MySql.Data.MySql50Client", typeof(IConnectionProvider))]
    public class Mysql50ConnectionProvider : IConnectionProvider
    {
        private string _connectionString;
        
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
            return new Mysql50SchemaProvider(this, new MysqlScemaDataProvider40(this));
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
