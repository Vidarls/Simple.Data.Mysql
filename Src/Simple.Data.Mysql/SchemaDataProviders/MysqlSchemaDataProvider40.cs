using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Mysql.SchemaDataProviders
{
    internal class MysqlSchemaDataProvider40 : IMysqlSchemaDataProvider
    {
        private readonly IConnectionProvider _connectionProvider;

        private IEnumerable<Table> _cachedTables;
        private IEnumerable<TableColumnInfoPair> _cachedColumns;
        private IEnumerable<TableForeignKeyPair>  _cachedForeignKeys;
      
        public MysqlSchemaDataProvider40(IConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public IEnumerable<Table> GetTables()
        {
            if (_cachedTables == null)
                FillSchemaCache();
            
            return _cachedTables;
        }

        public IEnumerable<Procedure> GetStoredProcedures()
        {
            return Enumerable.Empty<Procedure>();
        }

        public string GetDefaultSchema()
        {
            return null;
        }

        public IEnumerable<Parameter> GetParameters(Procedure storedProcedure)
        {
            return Enumerable.Empty<Parameter>();
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
            var columns = new List<TableColumnInfoPair>();
            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = GetTables().Select(t => string.Format("SHOW COLUMNS FROM `{0}`;", t.ActualName)).Aggregate((result, next) => result += next);
            using (var reader = command.ExecuteReader())
            {
                var i = 0;
                do
                {
                    while (reader.Read())
                    {
                        columns.Add(new TableColumnInfoPair(
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
            var allForeignKeys = new List<TableForeignKeyPair>();
            allForeignKeys.AddRange(GetExplicitForeignKeys(connection));
            allForeignKeys.AddRange(GetImplicitForeignKeys(allForeignKeys.Select(i => i.Table)));
            _cachedForeignKeys = allForeignKeys;
        }

        private IEnumerable<TableForeignKeyPair> GetExplicitForeignKeys(IDbConnection connection)
        {
            var foreignKeys = new List<TableForeignKeyPair>();            

            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = GetTables().Select(t => string.Format("SHOW CREATE TABLE `{0}`;", t.ActualName)).Aggregate((result, next) => result += next);

            using (var reader = command.ExecuteReader())
            {
                var i = 0;
                do
                {
                    while (reader.Read())
                    {
                        foreignKeys.AddRange(
                            MysqlForeignKeyCreator.ExtractForeignKeysFromCreateTableSql(GetTables().Select(t => t.ActualName).ElementAt(i), 
                                                                                        reader[1].ToString(), false, false)                                                                                  
                                                                                        .Select(fk => new TableForeignKeyPair(GetTables().ElementAt(i),fk))
                            );
                    }
                    i++;
                } while ((reader.NextResult()));
            }
            return foreignKeys;
        }

        private IEnumerable<TableForeignKeyPair> GetImplicitForeignKeys(IEnumerable<Table> tablesWithInnoDbForeignKeys)
        {
            //Implicit foreign key support
            //MyIsam (the most used Mysql db engine) does not support foreign key constraint
            //Foreign key support is therefor implemented in an implicit way based on naming conventions
            //If a column name exists as a primarykey in one table, then a column with the same name can
            //be used as a foreign key in another table.
            var foreignKeys = new List<TableForeignKeyPair>();

            foreach (var table in _cachedTables)
            {
                var primaryKeyColumns = _cachedColumns.Where(t => !t.Table.Equals(table) && t.ColumnInfo.IsPrimaryKey && !tablesWithInnoDbForeignKeys.Contains(t.Table));
                var columns = GetColumnsFor(table);
                foreach (var column in columns)
                {
                    foreignKeys.AddRange(
                        primaryKeyColumns.Where(c => c.ColumnInfo.Name == column.Name).Select(
                            c =>
                            new TableForeignKeyPair(table,new ForeignKey(new ObjectName(null, table.ActualName), new[] { column.Name },
                                           new ObjectName(null, c.Table.ActualName), new[] { c.ColumnInfo.Name }))));
                }
            }
            return foreignKeys;
        }

        public IEnumerable<MysqlColumnInfo> GetColumnsFor(Table table)
        {
            if (_cachedColumns == null)
                FillSchemaCache();

            var found = _cachedColumns.Where(c=>c.Table.Equals(table)).Select(c=>c.ColumnInfo);
            return found;
        }

        public IEnumerable<ForeignKey> GetForeignKeysFor(Table table)
        {
            if (_cachedForeignKeys == null)
                FillSchemaCache();
            return _cachedForeignKeys.Where(c => c.Table.Equals(table)).Select(c => c.ForeignKey);
        }

        public Key GetPrimaryKeyFor(Table table)
        {
            return new Key(GetColumnsFor(table).Where(c=>c.IsPrimaryKey).Select(c=>c.Name));
        }

        public string QuoteObjectName(string unquotedName)
        {
            return unquotedName;
        }
    }
}