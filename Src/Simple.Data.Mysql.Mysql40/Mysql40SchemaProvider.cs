using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;
using Simple.Data.Mysql.Mysql40.ShemaDataProviders;

namespace Simple.Data.Mysql.Mysql40
{
    class Mysql40SchemaProvider : ISchemaProvider
    {
        private readonly IConnectionProvider _connectionProvider;
        private readonly IMysqlScemaDataProvider _mysqlScemaDataProvider;

        public Mysql40SchemaProvider(IConnectionProvider connectionProvider, IMysqlScemaDataProvider mysqlScemaDataProvider)
        {
            if (connectionProvider == null) throw new ArgumentNullException("connectionProvider");
            _connectionProvider = connectionProvider;
            _mysqlScemaDataProvider = mysqlScemaDataProvider;
        }

        public IConnectionProvider ConnectionProvider
        {
            get { return _connectionProvider; }
        }

        public IEnumerable<Table> GetTables()
        {
            return _mysqlScemaDataProvider.GetTables();
        }

        public IEnumerable<Column> GetColumns(Table table)
        {
            return _mysqlScemaDataProvider.GetColumnsFor(table).Select(c => new Column(c.Name, table, c.IsAutoincrement, c.DbType, c.Capacity));
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
            return _mysqlScemaDataProvider.GetPrimaryKeyFor(table);
        }

        public IEnumerable<ForeignKey> GetForeignKeys(Table table)
        {
            return _mysqlScemaDataProvider.GetForeignKeysFor(table);
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
        
        public string GetDefaultSchema()
        {
            return null;
        }
    }
}
