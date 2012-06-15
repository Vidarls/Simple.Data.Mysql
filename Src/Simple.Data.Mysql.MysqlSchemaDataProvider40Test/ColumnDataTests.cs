using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;
using Simple.Data.Mysql.Mysql40.ShemaDataProviders;

namespace Simple.Data.Mysql.MysqlSchemaDataProvider40Test
{
    public class ColumnDataTests
    {
        private static readonly string ConnectionString =
            "server=localhost;user=root;database=SimpleDataTest;";

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
            var connectionProvider = new Mysql40.Mysql40ConnectionProvider();
            connectionProvider.SetConnectionString(ConnectionString);

            var schemaDataProvider = new MysqlScemaDataProvider40(connectionProvider);
            var columns = schemaDataProvider.GetColumnsFor(table).Select(c => new Column(c.Name, table, c.IsAutoincrement, c.DbType, c.Capacity)); 
            Assert.AreEqual(expectedColumns.Count, columns.Count());
            columns.Should().Contain(expectedColumns);
        }

        [Test]
        public void ColumnDataShouldBeCached()
        {
            var table = new Table("Users", null, TableType.Table);
            var expectedColumns = new List<Column>
                                      {
                                          new Column("Id", table, true, DbType.Int32, 11),
                                          new Column("Name", table, false, DbType.String, 255),
                                          new Column("Password", table, false, DbType.String, 255),
                                          new Column("Age", table, false, DbType.Int32, 11)

                                      };

            var realConnectionProvider = new Mysql40.Mysql40ConnectionProvider();
            realConnectionProvider.SetConnectionString(ConnectionString);
            var proxiedConnectionProvider = A.Fake <IConnectionProvider>((o) => o.Wrapping(realConnectionProvider));
            
            var schemaDataProvider = new MysqlScemaDataProvider40(proxiedConnectionProvider);
            schemaDataProvider.GetColumnsFor(table).ToList();
            schemaDataProvider.GetColumnsFor(table).ToList();
            schemaDataProvider.GetColumnsFor(table).ToList();
            schemaDataProvider.GetColumnsFor(table).ToList();
            A.CallTo(()=>proxiedConnectionProvider.CreateConnection()).MustHaveHappened(Repeated.Exactly.Once);
        }
    }
}
