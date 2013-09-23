using System;
using NUnit.Framework;

namespace Simple.Data.Mysql.Test
{
    [TestFixture]
    public class DecimalTests
    {
        private static readonly string ConnectionString =
            "server=localhost;user=root;database=simpledatatest;";

        [Test]
        public void CorrectDecimalValue()
        {
            var db = Database.OpenConnection(ConnectionString);
            var price = Convert.ToDecimal(4.75);
            db.Items.Insert(Name: "Table", Price: price);
            var tableItem = db.Items.FindBy(Name: "Table");
            Assert.True(tableItem.Price == price);
            db.Items.Delete(ItemId: tableItem.ItemId);  
        }
    }
}