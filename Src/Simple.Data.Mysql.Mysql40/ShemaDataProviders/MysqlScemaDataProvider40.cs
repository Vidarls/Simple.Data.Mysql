using System;
using System.Data.Common;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Mysql.Mysql40.ShemaDataProviders
{
    public interface IMysqlScemaDataProvider
    {
        IEnumerable<Table> GetTables();
        IEnumerable<MysqlColumnInfo> GetColumnsFor(Table table);
        IEnumerable<ForeignKey> GetForeignKeysFor(Table table);
        Key GetPrimaryKeyFor(Table table);
    }

    public class MysqlScemaDataProvider40 : IMysqlScemaDataProvider
    {
        private readonly IConnectionProvider _connectionProvider;

        private IEnumerable<Table> _cachedTables;
        private IEnumerable<Tuple<Table,MysqlColumnInfo>> _cachedColumns;
        private IEnumerable<Tuple<Table, ForeignKey>>  _cachedForeignKeys;
        private string _sqlMode;

        public MysqlScemaDataProvider40(IConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public IEnumerable<Table> GetTables()
        {
            if (_cachedTables == null)
                FillSchemaCache();
            
            return _cachedTables;
        }

        private void FillSchemaCache()
        {
            using (var connection = _connectionProvider.CreateConnection())
            {
                connection.Open();
                FillTableCache(connection);
                FillColumnCache(connection);
                FillForeignKeyCache(connection);
                connection.Close();
            }
        }

        private void FillTableCache(IDbConnection connection)
        {
            var tables = new List<Table>();
            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = "SHOW TABLES;";
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    tables.Add(new Table(reader[0].ToString(), null, TableType.Table));
                }
            }
            _cachedTables = tables;
        }

        private void FillColumnCache(IDbConnection connection)
        {
            var columns = new List<Tuple<Table, MysqlColumnInfo>>();
            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = GetTables().Select(t => string.Format("SHOW COLUMNS FROM {0};", t.ActualName)).Aggregate((result, next) => result += next);
            using (var reader = command.ExecuteReader())
            {
                var i = 0;
                do
                {
                    while (reader.Read())
                    {
                        columns.Add(new Tuple<Table, MysqlColumnInfo>(
                                        _cachedTables.ElementAt(i),
                                        MysqlColumnInfo.CreateColumnInfo(
                                            reader[0].ToString(),
                                            reader[5].ToString(),
                                            reader[1].ToString(),
                                            reader[3].ToString())
                                        )
                                   );
                    }
                    i++;
                } while ((reader.NextResult()));
            }
            _cachedColumns = columns;
        }

        private void FillForeignKeyCache(IDbConnection connection)
        {
            var allForeignKeys = new List<Tuple<Table, ForeignKey>>();
            var explicitForeignKeys = new List<Tuple<Table, ForeignKey>>();
            var implicitForeignKeys = new List<Tuple<Table, ForeignKey>>();
            foreach (var table in GetTables())
            {
                var foreignKeys = new List<ForeignKey>();
                foreignKeys.AddRange(GetForeignKeysFromCreateSql(table, connection));
                foreignKeys.AddRange(GetImplicitForeignKeys(table, GetTablesWithInnoDbForeignKeys(foreignKeys)));
                var table1 = table;
                allForeignKeys.AddRange(foreignKeys.Select(f => new Tuple<Table,ForeignKey>(table1,f)));
            }
            _cachedForeignKeys = allForeignKeys;

        }

        private IEnumerable<Table> GetTablesWithInnoDbForeignKeys(IEnumerable<ForeignKey> foreignKeys)
        {
            return GetTables().Where(t=>foreignKeys.Select(f=>f.MasterTable.Name).Contains(t.ActualName));
        }

        private IEnumerable<ForeignKey> GetForeignKeysFromCreateSql(Table table, IDbConnection connection)
        {
            var createTableSql = default(String);
            var sqlMode = default(String);
            
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = String.Format("SHOW CREATE TABLE `{0}`", table.ActualName);

                    sqlMode = this.GetSqlMode(connection);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            createTableSql = (reader[1] as String);
                        }
                    }
                }

            return MysqlForeignKeyCreator.ExtractForeignKeysFromCreateTableSql(table.ActualName, createTableSql,
                sqlMode.Contains("ANSI_QUOTES"), !sqlMode.Contains("NO_BACKSLASH_ESCAPES"));
        }

        private IEnumerable<ForeignKey> GetImplicitForeignKeys(Table table, IEnumerable<Table> tablesWithInnoDbForeignKeys)
        {
            //Implicit foreign key support
            //MyIsam (the most used Mysql db engine) does not support foreign key constraint
            //Foreign key support is therefor implemented in an implicit way based on naming conventions
            //If a column name exists as a primarykey in one table, then a column with the same name can
            //be used as a foreign key in another table.
            var foreignKeys = new List<ForeignKey>();
            var primaryKeyColumns = _cachedColumns.Where(t => !t.Item1.Equals(table) && t.Item2.IsPrimaryKey && !tablesWithInnoDbForeignKeys.Contains(t.Item1));
            var columns = GetColumnsFor(table);
            foreach (var column in columns)
            {
                foreignKeys.AddRange(
                    primaryKeyColumns.Where(c => c.Item2.Name == column.Name).Select(
                        c =>
                        new ForeignKey(new ObjectName(null, table.ActualName), new[] {column.Name},
                                       new ObjectName(null, c.Item1.ActualName), new[] {c.Item2.Name})));
            }
            return foreignKeys;
        }

        private String GetSqlMode(IDbConnection connection)
        {
            if (_sqlMode == null)
            {
                try
                {
                    //this is not supported in 4.0 and will throw a MySqlException
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT @@SQL_MODE";
                        _sqlMode = command.ExecuteScalar().ToString();
                    }
                }
                catch (DbException)
                {
                }
                _sqlMode = String.Empty;
            }

            return _sqlMode;
        }

        public IEnumerable<MysqlColumnInfo> GetColumnsFor(Table table)
        {
            if (_cachedColumns == null)
                FillSchemaCache();

            var found = _cachedColumns.Where(c=>c.Item1.Equals(table)).Select(c=>c.Item2);
            return found;
        }

        public IEnumerable<ForeignKey> GetForeignKeysFor(Table table)
        {
            if (_cachedForeignKeys == null)
                FillSchemaCache();
            return _cachedForeignKeys.Where(c => c.Item1.Equals(table)).Select(c => c.Item2);
        }

        public Key GetPrimaryKeyFor(Table table)
        {
            return new Key(GetColumnsFor(table).Where(c=>c.IsPrimaryKey).Select(c=>c.Name));
        }
    }
}