﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Simple.Data.Mysql.Mysql50
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
            var mysqlAssembly =
                Assembly.LoadFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring(8)), "MySql.Data.dll"));
            var mysqlDbFactoryType = mysqlAssembly.GetType("MySql.Data.MySqlClient.MySqlClientFactory");
            return (DbProviderFactory) Activator.CreateInstance(mysqlDbFactoryType);
        }
    }
}
