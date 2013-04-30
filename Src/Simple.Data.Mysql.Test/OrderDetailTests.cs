using System;
using NUnit.Framework;

namespace Simple.Data.Mysql.Test
{
    [TestFixture]
    public class OrderDetailTests
    {
        private static readonly string ConnectionString =
           "server=localhost;user=root;database=SimpleDataTest;";

        [Test]
        public void TestOrderDetail()
        {
            var db = Database.OpenConnection(ConnectionString);
            var order = db.Orders.FindByOrderDate(new DateTime(2010, 8, 11));
            Assert.IsNotNull(order);
            var orderItem = order.OrderItems.FirstOrDefault();
            Assert.IsNotNull(orderItem);
            var item = orderItem.Item;
            Assert.AreEqual("Widget", item.Name);
        }
    }
}
