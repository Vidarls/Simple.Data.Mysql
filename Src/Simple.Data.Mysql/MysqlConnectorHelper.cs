using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Reflection;

namespace Simple.Data.Mysql
{
    public static class MysqlConnectorHelper
    {
        private static DbProviderFactory _dbFactory;

        private static DbProviderFactory DbFactory
        {
            get
            {
                if (_dbFactory == null)
                    _dbFactory = GetDbFactory();
                return _dbFactory;
            }
        }

        public static IDbConnection CreateConnection(string connectionString)
        {
            var connection = DbFactory.CreateConnection();
            connection.ConnectionString = connectionString;
            return connection;
        }

        public static DbDataAdapter CreateDataAdapter(string sqlCommand, IDbConnection connection)
        {
            var adapter = DbFactory.CreateDataAdapter();
            var command = (DbCommand)connection.CreateCommand();
            command.CommandText = sqlCommand;
            adapter.SelectCommand = command;
            return adapter;
        }


        private static DbProviderFactory GetDbFactory()
        {
			var thisAssembly = typeof(MysqlConnectorHelper).Assembly;
			var path = Path.GetDirectoryName(thisAssembly.Location);
			if (string.IsNullOrEmpty(path)) throw new ArgumentException("Unrecognised assemblyFile.");

            var mysqlAssembly = Assembly.LoadFrom(Path.Combine(path, "MySql.Data.dll"));

            var mysqlDbFactoryType = mysqlAssembly.GetType("MySql.Data.MySqlClient.MySqlClientFactory");
            return (DbProviderFactory)Activator.CreateInstance(mysqlDbFactoryType);
        }

    }
}
