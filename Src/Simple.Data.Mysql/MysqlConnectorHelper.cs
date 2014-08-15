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

        private static string GetAssemblyDirectory()
        {
            if (IsRunningOnMono())
            {
                var codeBaseUri = new UriBuilder(Assembly.GetAssembly(typeof(MysqlConnectorHelper)).CodeBase);
                return Path.GetDirectoryName(Uri.UnescapeDataString(codeBaseUri.Path));
            }

            var thisAssembly = typeof(MysqlConnectorHelper).Assembly;

            var path = thisAssembly.CodeBase.Replace("file:///", "").Replace("file://", "//");
            path = Path.GetDirectoryName(path);
            if (path == null) throw new ArgumentException("Unrecognised assemblyFile.");

            return path;
        }

        public static bool IsRunningOnMono()
        {
            //This ideally needs confirming that this is the proper way to identify mono
            return Type.GetType("Mono.Runtime") != null;
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
            var mysqlAssembly = Assembly.LoadFrom(Path.Combine(GetAssemblyDirectory(), "MySql.Data.dll"));

            var mysqlDbFactoryType = mysqlAssembly.GetType("MySql.Data.MySqlClient.MySqlClientFactory");
            return (DbProviderFactory)Activator.CreateInstance(mysqlDbFactoryType);
        }


    }
}
