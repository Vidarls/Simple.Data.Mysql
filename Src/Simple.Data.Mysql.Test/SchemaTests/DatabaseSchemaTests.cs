using System.Data;
using System.Linq;
using NUnit.Framework;
using Simple.Data.TestHelper;

namespace Simple.Data.Mysql.Test.SchemaTests
{
    [TestFixture]
    public class DatabaseSchemaTests : DatabaseSchemaTestsBase
    {
        private static readonly string ConnectionString =
            "server=localhost;user=root;database=SimpleDataTest;";

        protected override Database GetDatabase() 
        {
            
            return Database.OpenConnection(ConnectionString);
        }

        
        [Test]
        public void TestTables()
        {
            //Mysql 4 on windows converts all table names to lower case.
            Assert.AreEqual(1, Schema.Tables.Count(t => t.ActualName == "users"));
        }

        [Test]
        public void TestColumns()
        {
            var table = Schema.FindTable("Users");
            Assert.AreEqual(1, table.Columns.Count(c => c.ActualName == "Id"));
        }

        [Test]
        public void TestColumnTypeInfo()
        {
            var column = Schema.FindTable("Users").Columns.Where(c => c.ActualName == "Id").First();
            Assert.AreEqual(DbType.Int32, column.DbType, "DbType not correct");
            Assert.AreNotEqual(0, column.MaxLength, "Capacity not recorded");
        }

        [Test]
        public void TestPrimaryKey()
        {
            Assert.AreEqual("CustomerId", Schema.FindTable("Customers").PrimaryKey[0]);
        }

        [Test]
        public void TestForeignKey()
        {
            var foreignKey = Schema.FindTable("Orders").ForeignKeys
                                   .Single(fk => fk.MasterTable.Name == "customers");
            Assert.AreEqual("customers", foreignKey.MasterTable.Name);
            Assert.AreEqual("orders", foreignKey.DetailTable.Name);
            CollectionAssert.AreEqual(new[] { "CustomerId" }, foreignKey.Columns.AsEnumerable());
            CollectionAssert.AreEqual(new[] { "CustomerId" }, foreignKey.UniqueColumns.AsEnumerable());
        }

        [Test]
        public void TestSingularResolution()
        {
            Assert.AreEqual("orderitems", Schema.FindTable("OrderItem").ActualName);
        }

        [Test]
        public void TestShoutySingularResolution()
        {
            Assert.AreEqual("orderitems",Schema.FindTable("ORDER_ITEM").ActualName);
        }

        [Test]
        public void TestShoutyPluralResolution()
        {
            Assert.AreEqual("orderitems", Schema.FindTable("ORDER_ITEM").ActualName);
        }
    }
}
