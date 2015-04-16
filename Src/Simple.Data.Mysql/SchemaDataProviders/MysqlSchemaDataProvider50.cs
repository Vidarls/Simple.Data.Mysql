using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Mysql.SchemaDataProviders
{
    class MysqlSchemaDataProvider50 : IMysqlSchemaDataProvider
    {
        private readonly IConnectionProvider _connectionProvider;

        private IEnumerable<Table> _cachedTables;
        private IEnumerable<TableColumnInfoPair> _cachedColumns;
        private IEnumerable<TableForeignKeyPair> _cachedForeignKeys;
        private string _sqlMode;
        private string _cachedSchema;

        public MysqlSchemaDataProvider50(IConnectionProvider connectionProvider)
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
            return GetSchema("Procedures").Select(SchemaRowToStoredProcedure);
        }

        public string GetDefaultSchema()
        {
            if (_cachedTables == null)
                FillSchemaCache();

            return _cachedSchema;
        }

        private void FillCachedDefaultSchema(IDbConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                connection.OpenIfClosed();
                command.CommandText = "SELECT database()";
                _cachedSchema = command.ExecuteScalar().ToString();
            }
        }

        private IEnumerable<DataRow> GetSchema(string collectionName, params string[] constraints)
        {
            using (var cn = _connectionProvider.CreateConnection())
            {
                cn.Open();
                return cn.GetSchema(collectionName, constraints).AsEnumerable();
            }
        }

        private static Procedure SchemaRowToStoredProcedure(DataRow row)
        {
            return new Procedure(row["ROUTINE_NAME"].ToString(), row["SPECIFIC_NAME"].ToString(), row["ROUTINE_SCHEMA"].ToString());
        }


        public IEnumerable<Parameter> GetParameters(Procedure storedProcedure)
        {
            var list = new List<Parameter>();

            using (var connection = _connectionProvider.CreateConnection())
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText =
                    string.Format("SELECT * FROM information_schema.parameters WHERE SPECIFIC_NAME = '{0}';",
                        storedProcedure.Name);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Parameter
                        (
                            reader["PARAMETER_NAME"].ToString(),
                            SqlTypeResolver.GetClrType(reader["DATA_TYPE"].ToString()),
                            GetParameterDirection(reader["PARAMETER_MODE"].ToString()),
                            MysqlColumnInfo.GetDbType(reader["DATA_TYPE"].ToString()),
                            Convert.IsDBNull(reader["CHARACTER_MAXIMUM_LENGTH"]) ? -1 : Convert.ToInt32(reader["CHARACTER_MAXIMUM_LENGTH"])
                        ));
                    }
                }

                connection.Close();

                return list;
            }

        }

        private ParameterDirection GetParameterDirection(string parameterMode)
        {
            switch (parameterMode)
            {
                case "IN":
                    return ParameterDirection.Input;
                case "OUT":
                    return ParameterDirection.Output;
                case "INOUT":
                    return ParameterDirection.InputOutput;
                case "RETURN":
                    return ParameterDirection.ReturnValue;
                default:
                    throw new SimpleDataException(String.Format("Unknown parameter mode: {0}", parameterMode));
            }
        }

        private void FillSchemaCache()
        {
            using (var connection = _connectionProvider.CreateConnection())
            {
                connection.Open();
                FillCachedDefaultSchema(connection);
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
            command.CommandText = string.Format("SELECT TABLE_NAME, COLUMN_NAME, EXTRA, COLUMN_TYPE, COLUMN_KEY FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{0}' ORDER BY TABLE_NAME;", _cachedSchema);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    columns.Add(new TableColumnInfoPair(
                                        new Table(reader[0].ToString(), null, TableType.Table),
                                        MysqlColumnInfo.CreateColumnInfo(reader[1].ToString(), reader[2].ToString(), reader[3].ToString(), reader[4].ToString())
                                    )
                               );
                }
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
            var sqlMode = GetSqlMode(connection);

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
                                                                                        reader[1].ToString(),
                                                                                        sqlMode.Contains("ANSI_QUOTES"), !sqlMode.Contains("NO_BACKSLASH_ESCAPES")
                                                                                        ).Select(fk => new TableForeignKeyPair(GetTables().ElementAt(i), fk))
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
                            new TableForeignKeyPair(table, new ForeignKey(new ObjectName(null, table.ActualName), new[] { column.Name },
                                           new ObjectName(null, c.Table.ActualName), new[] { c.ColumnInfo.Name }))));
                }
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

            var found = _cachedColumns.Where(c => c.Table.Equals(table)).Select(c => c.ColumnInfo);
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
            return new Key(GetColumnsFor(table).Where(c => c.IsPrimaryKey).Select(c => c.Name));
        }

        public string QuoteObjectName(string unquotedName)
        {
            return unquotedName.StartsWith("`") ? unquotedName : string.Concat("`", unquotedName, "`");
        }


    }
}
