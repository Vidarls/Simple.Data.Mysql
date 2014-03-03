using Simple.Data.Ado;
using Simple.Data.Ado.Schema;
using Simple.Data.Mysql.ShemaDataProviders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.Data.Mysql
{
    class MysqlSchemaProvider : ISchemaProvider
    {
        private readonly IConnectionProvider _connectionProvider;
        private readonly IMysqlSchemaDataProvider _mysqlSchemaDataProvider;

        public MysqlSchemaProvider(IConnectionProvider connectionProvider, IMysqlSchemaDataProvider mysqlSchemaDataProvider)
        {
            if (connectionProvider == null) throw new ArgumentNullException("connectionProvider");
            _connectionProvider = connectionProvider;
            _mysqlSchemaDataProvider = mysqlSchemaDataProvider;
        }

        public IConnectionProvider ConnectionProvider
        {
            get { return _connectionProvider; }
        }

        public IEnumerable<Table> GetTables()
        {
            return _mysqlSchemaDataProvider.GetTables();
        }

        public IEnumerable<Column> GetColumns(Table table)
        {
            return _mysqlSchemaDataProvider.GetColumnsFor(table).Select(c => new Column(c.Name, table, c.IsAutoincrement, c.DbType, c.Capacity));
        }

        public IEnumerable<Procedure> GetStoredProcedures()
        {
            return _mysqlSchemaDataProvider.GetStoredProcedures();
        }

       public IEnumerable<Parameter> GetParameters(Procedure storedProcedure)
        {
            return _mysqlSchemaDataProvider.GetParameters(storedProcedure);
        }

        public Key GetPrimaryKey(Table table)
        {
            if (table == null) throw new ArgumentNullException("table");
            return _mysqlSchemaDataProvider.GetPrimaryKeyFor(table);
        }

        public IEnumerable<ForeignKey> GetForeignKeys(Table table)
        {
            return _mysqlSchemaDataProvider.GetForeignKeysFor(table);
        }

        public string QuoteObjectName(string unquotedName)
        {
            if (unquotedName == null) throw new ArgumentNullException("unquotedName");
            return _mysqlSchemaDataProvider.QuoteObjectName(unquotedName);
        }

        public string NameParameter(string baseName)
        {
            if (baseName == null) throw new ArgumentNullException("baseName");
            if (baseName.Length == 0) throw new ArgumentException("Base name must be provided");
            return (baseName.StartsWith("?")) ? baseName : "?" + baseName;
        }
        
        public string GetDefaultSchema()
        {
            return _mysqlSchemaDataProvider.GetDefaultSchema();
        }

    }
}
