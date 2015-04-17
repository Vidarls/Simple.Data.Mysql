using System.Collections.Generic;
using System.Data;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;
using Simple.Data.Mysql;
using Simple.Data.Mysql.SchemaDataProviders;
using Enumerable = System.Linq.Enumerable;

namespace Simple.Data.Mysql.Test.SchemaDataProviderTests
{
    [TestFixture]
    public class ColumnDataTests40
    {
        private const string ConnectionString = "server=localhost;user=root;database=SimpleDataTest;";

        [Test]
        public void CanProvideAllColumnsFromTable()
        {
            var table = new Table("users", null, TableType.Table);
            var expectedColumns = new List<Column>
                                      {
                                          new Column("Id", table, true, DbType.Int32, 11),
                                          new Column("Name", table, false, DbType.String, 255),
                                          new Column("Password", table, false, DbType.String, 255),
                                          new Column("Age", table, false, DbType.Int32, 11)

                                      };
            var connectionProvider = new MysqlConnectionProvider();
            connectionProvider.SetConnectionString(ConnectionString);

            var schemaDataProvider = new MysqlSchemaDataProvider40(connectionProvider);
            var columns = schemaDataProvider.GetColumnsFor(table).Select(c => new Column(c.Name, table, c.IsAutoincrement, c.DbType, c.Capacity)); 
            Assert.AreEqual(expectedColumns.Count, columns.Count());
            columns.Should().Contain(expectedColumns);
        }

        [Test]
        public void ColumnDataShouldBeCached()
        {
            var table = new Table("Users", null, TableType.Table);
            
            var realConnectionProvider = new MysqlConnectionProvider();
            realConnectionProvider.SetConnectionString(ConnectionString);
            var proxiedConnectionProvider = A.Fake <IConnectionProvider>((o) => o.Wrapping(realConnectionProvider));
            
            var schemaDataProvider = new MysqlSchemaDataProvider40(proxiedConnectionProvider);
            Enumerable.ToList(schemaDataProvider.GetColumnsFor(table));
            Enumerable.ToList(schemaDataProvider.GetColumnsFor(table));
            Enumerable.ToList(schemaDataProvider.GetColumnsFor(table));
            Enumerable.ToList(schemaDataProvider.GetColumnsFor(table));
            A.CallTo(()=>proxiedConnectionProvider.CreateConnection()).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void DoesNotQuoteNames()
        {
            var connectionprovider = new MysqlConnectionProvider();
            connectionprovider.SetConnectionString(ConnectionString);
            var schemaDataProvider = new MysqlSchemaDataProvider40(connectionprovider);
            var unquoted = "Unquoted";
            var quoted = schemaDataProvider.QuoteObjectName(unquoted);
            Assert.AreEqual(unquoted, quoted);
        }
    }

    [TestFixture]
    public class TableDataTests40
    {
        private const string ConnectionString = "server=localhost;user=root;database=SimpleDataTest;";

        [Test]
        public void CanGetTables()
        {
            var expectedTables = new List<Table>
                                     {
                                         new Table("customers", null, TableType.Table),
                                         new Table("items", null, TableType.Table),
                                         new Table("items_fk_test", null, TableType.Table),
                                         new Table("orderitems_fk_test", null, TableType.Table),
                                         new Table("orderitems", null, TableType.Table),
                                         new Table("orders", null, TableType.Table),
                                         new Table("orders_fk_test", null, TableType.Table),
                                         new Table("users", null, TableType.Table)
                                     };
            var connectionProvider = new MysqlConnectionProvider();
            connectionProvider.SetConnectionString(ConnectionString);

            var schemaDataProvider = new MysqlSchemaDataProvider40(connectionProvider);
            var tables = schemaDataProvider.GetTables();

            tables.Should().Contain(expectedTables);
        }
    }
}
