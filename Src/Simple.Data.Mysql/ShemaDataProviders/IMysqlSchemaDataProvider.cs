using System.Collections.Generic;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Mysql.ShemaDataProviders
{
    internal interface IMysqlSchemaDataProvider
    {
        IEnumerable<Table> GetTables();
        IEnumerable<MysqlColumnInfo> GetColumnsFor(Table table);
        IEnumerable<ForeignKey> GetForeignKeysFor(Table table);
        Key GetPrimaryKeyFor(Table table);
        string QuoteObjectName(string unquotedName);
        IEnumerable<Procedure> GetStoredProcedures();
        string GetDefaultSchema();
        IEnumerable<Parameter> GetParameters(Procedure storedProcedure);
    }

    internal class TableColumnInfoPair
    {
        public Table Table { get; private set; }
        public MysqlColumnInfo ColumnInfo { get; private set; }

        public TableColumnInfoPair(Table table, MysqlColumnInfo columnInfo)
        {
            Table = table;
            ColumnInfo = columnInfo;
        }
    }

    internal class TableForeignKeyPair
    {
        public Table Table { get; private set; }
        public ForeignKey ForeignKey { get; private set; }

        public TableForeignKeyPair(Table table, ForeignKey foreignKey)
        {
            Table = table;
            ForeignKey = foreignKey;
        }
    }
}