using System.Linq;
using NUnit.Framework;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;
using Simple.Data.Mysql.Mysql40;

namespace Simple.Data.Mysql.Mysql40Test
{
    [TestFixture]
    public class ForeignKeyTests
    {
        private static readonly string ConnectionString =
            "server=localhost;user=SimpleData;database=SimpleDataTest;password=test;";

        [Test]
        public void ShouldExtractForeignKeysFromCreateTableSql()
        {
            var exampleCreateSql = "CREATE TABLE `orderitems` ( " +
                                               " `OrderItemId` int(11) NOT NULL AUTO_INCREMENT, " +
                                               " `OrderId` int(11) DEFAULT NULL, " +
                                               " `ItemId` int(11) DEFAULT NULL, " +
                                               " `Quantity` int(11) DEFAULT NULL, " +
                                               " PRIMARY KEY (`OrderItemId`), " +
                                               " KEY `fk_orderitems_items` (`ItemId`), " +
                                               " KEY `fk_orderitems_orders` (`OrderId`, `Test`), " +
                                               " CONSTRAINT `fk_orderitems_items` FOREIGN KEY (`ItemId`) REFERENCES `items` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION, " +
                                               " CONSTRAINT `fk_orderitems_orders` FOREIGN KEY (`OrderId`, `Test` ) REFERENCES `dbo`.`orders` ( `Id` ,`Test`) ON DELETE NO ACTION ON UPDATE NO ACTION " +
                                               ") ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8";
            
            var foreignKeys = MysqlForeignKeyCreator.ExtractForeignKeysFromCreateTableSql("orderitems_fk_test", exampleCreateSql, false, true);
            var items = foreignKeys.FirstOrDefault(fk => fk.MasterTable.Name == "items");
            Assert.IsNotNull(items);
            CollectionAssert.AreEqual(new[] { "ItemId" }, items.Columns.AsEnumerable());
            CollectionAssert.AreEqual(new[] { "Id" }, items.UniqueColumns.AsEnumerable());
            var orders = foreignKeys.FirstOrDefault(fk => fk.MasterTable.Name == "orders");
            Assert.IsNotNull(orders);
            CollectionAssert.AreEqual(new[] { "OrderId", "Test" }, orders.Columns.AsEnumerable());
            CollectionAssert.AreEqual(new[] { "Id", "Test" }, orders.UniqueColumns.AsEnumerable());
        }

        [Test]
        public void ShouldFindForeignKeysUsingCreateTableSql()
        {
            var connectionProvider = new ProviderHelper().GetProviderByConnectionString(ConnectionString);
            var schemaProvider = connectionProvider.GetSchemaProvider();
            var databaseSchema = DatabaseSchema.Get(connectionProvider);
            var table = databaseSchema.FindTable("orderitems_fk_test");
            var foreignKeys = schemaProvider.GetForeignKeys(table);
            var items_fk_test = foreignKeys.FirstOrDefault(fk => fk.MasterTable.Name == "items_fk_test");
            Assert.IsNotNull(items_fk_test);
            CollectionAssert.AreEqual(new[] { "ItemsId" }, items_fk_test.Columns.AsEnumerable());
            CollectionAssert.AreEqual(new[] { "ItemId" }, items_fk_test.UniqueColumns.AsEnumerable());
            var orders_fk_test = foreignKeys.FirstOrDefault(fk => fk.MasterTable.Name == "orders_fk_test");
            Assert.IsNotNull(orders_fk_test);
            CollectionAssert.AreEqual(new[] { "OrdersId" }, orders_fk_test.Columns.AsEnumerable());
            CollectionAssert.AreEqual(new[] { "OrderId" }, orders_fk_test.UniqueColumns.AsEnumerable());
        }
    }
}