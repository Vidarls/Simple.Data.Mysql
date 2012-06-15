using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Simple.Data.Ado.Schema;
using Simple.Data.Mysql.Mysql40.ShemaDataProviders;
using FluentAssertions;

namespace Simple.Data.Mysql.MysqlSchemaDataProvider40Test
{
    public class TableDataTests
    {
        private static readonly string ConnectionString =
            "server=localhost;user=root;database=SimpleDataTest;";

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
            var connectionProvider = new Mysql40.Mysql40ConnectionProvider();
            connectionProvider.SetConnectionString(ConnectionString);

            var schemaDataProvider = new MysqlScemaDataProvider40(connectionProvider);
            var tables = schemaDataProvider.GetTables();

            tables.Should().Contain(expectedTables);
        }
    }
}
