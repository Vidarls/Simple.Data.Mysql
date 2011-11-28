using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Mysql.Mysql40
{
    class Mysql40SchemaProvider : ISchemaProvider
    {
        private readonly IConnectionProvider _connectionProvider;

        public Mysql40SchemaProvider(IConnectionProvider connectionProvider)
        {
            if (connectionProvider == null) throw new ArgumentNullException("connectionProvider");
            _connectionProvider = connectionProvider;
        }

        public IConnectionProvider ConnectionProvider
        {
            get { return _connectionProvider; }
        }

        public IEnumerable<Table> GetTables()
        {
            using (var cn = ConnectionProvider.CreateConnection())
            {
                var command = cn.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = "SHOW TABLES;";
                cn.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return new Table(reader[0].ToString(),null, TableType.Table);
                    }
                }
                
            }
        }

        public IEnumerable<Column> GetColumns(Table table)
        {
            return GetColumnInfo(table).Select(c => new Column(c.Name, table, c.IsAutoincrement, c.DbType, c.Capacity));
        }

        public IEnumerable<Procedure> GetStoredProcedures()
        {
            return Enumerable.Empty<Procedure>();
        }

       public IEnumerable<Parameter> GetParameters(Procedure storedProcedure)
        {
            return Enumerable.Empty<Parameter>();
        }

        public Key GetPrimaryKey(Table table)
        {
            if (table == null) throw new ArgumentNullException("table");
            return
                new Key (GetColumnsDataTable(table).AsEnumerable().Where(c => c["Key"].ToString().ToUpper() == "PRI").Select(
                    c => c["Field"].ToString()));

        }

        private String GetSqlMode(IDbConnection connection)
        {
            try
            {
                //this is not supported in 4.0 and will throw a MySqlException
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT @@SQL_MODE";
                    return command.ExecuteScalar().ToString();
                }
            }
            catch (MySqlException) { }
            return String.Empty;
        }

        private IEnumerable<ForeignKey> GetForeignKeysFromCreateSql(Table table)
        {
            var createTableSql = default(String);
            var sqlMode = default(String);
            using (var connection = ConnectionProvider.CreateConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = String.Format("SHOW CREATE TABLE `{0}`", table.ActualName);
                    connection.Open();

                    sqlMode = this.GetSqlMode(connection);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            createTableSql = (reader[1] as String);
                        }
                    }
                }
            }

            return MysqlForeignKeyCreator.ExtractForeignKeysFromCreateTableSql(table.ActualName, createTableSql,
                sqlMode.Contains("ANSI_QUOTES"), !sqlMode.Contains("NO_BACKSLASH_ESCAPES"));
        }

        public IEnumerable<ForeignKey> GetForeignKeys(Table table)
        {
            //Implicit foreign key support
            //MyIsam (the most used Mysql db engine) does not support foreign key constraint
            //Foreign key support is therefor implemented in an implicit way based on naming conventions
            //If a column name exists as a primarykey in one table, then a column with the same name can
            //be used as a foreign key in another table.
            var foreignKeys = new List<ForeignKey>();
            foreignKeys.AddRange(this.GetForeignKeysFromCreateSql(table));

            var skipTables = foreignKeys.Select(fk => fk.MasterTable.Name)
                                        .Concat(new[] { table.ActualName });
            var tables = GetTables().Where(t => !skipTables.Contains(t.ActualName));
            var primaryKeys = tables.Select(t => new { Table = t, Key = GetPrimaryKey(t) })
                                    .Where(t => t.Key.Length == 1)
                                    .Select(t => new { TableName = t.Table.ActualName, ColumnName = t.Key[0] })
                                    .ToArray();

            foreach (var column in table.Columns)
            {
                foreignKeys.AddRange(
                    primaryKeys.Where(key => key.ColumnName == column.ActualName)
                               .Select(key => new ForeignKey(new ObjectName(null, table.ActualName), new[] { column.ActualName },
                                                             new ObjectName(null, key.TableName), new[] { key.ColumnName })));
            }
            return foreignKeys;
        }

        public string QuoteObjectName(string unquotedName)
        {
            //no quoting of table / column name in MySql 4.0
            if (unquotedName == null) throw new ArgumentNullException("unquotedName");
            return unquotedName;
        }

        public string NameParameter(string baseName)
        {
            if (baseName == null) throw new ArgumentNullException("baseName");
            if (baseName.Length == 0) throw new ArgumentException("Base name must be provided");
            return (baseName.StartsWith("?")) ? baseName : "?" + baseName;
        }

        private DataTable GetColumnsDataTable(Table table)
        {
            return SelectToDataTable(string.Format("SHOW COLUMNS FROM {0}", table.ActualName));
        }

        private IEnumerable<MysqlColumnInfo> GetColumnInfo(Table table)
        {
            return GetColumnsDataTable(table).AsEnumerable().Select(row =>
                MysqlColumnInfoCreator.CreateColumnInfo(row["field"].ToString(),row["extra"].ToString(), row["type"].ToString()));
        }

        private DataTable SelectToDataTable(string sql)
        {
            var dataTable = new DataTable();
            using (var cn = ConnectionProvider.CreateConnection() as MySqlConnection)
            {
                using (var adapter = new MySqlDataAdapter(sql, cn))
                {
                    adapter.Fill(dataTable);
                }

            }

            return dataTable;
        }
    }
}
