using NUnit.Framework;

namespace Simple.Data.Mysql5.Test.Features
{
    [TestFixture]
    public class SchemaTests
    {
        private string connection = "server=localhost;database=simpledatatestfeatures;user=root;";

        [Test]
        public void GetCustomers()
        {
            var db = Database.OpenConnection(connection);
            var list = db.Customers.All().OrderByCustomerId().ToList();

            Assert.AreEqual(5, list.Count);
            Assert.AreEqual(1, list[0].CustomerId);
            Assert.AreEqual(2, list[1].CustomerId);
            Assert.AreEqual(3, list[2].CustomerId);
            Assert.AreEqual(4, list[3].CustomerId);
            Assert.AreEqual(5, list[4].CustomerId);
        }


        [Test]
        public void GetOrders()
        {
            var db = Database.OpenConnection(connection);
            var list = db.Orders.All().OrderByOrderId().ToList();

            Assert.AreEqual(9, list.Count);
            Assert.AreEqual(1, list[0].OrderId);
            Assert.AreEqual(9, list[8].OrderId);
        }


    }
}
