using NUnit.Framework;

namespace Simple.Data.Mysql.Test
{
    [TestFixture]
    public class MysqlConnectorHelperTest
    {
        [Test]
        public void CanCreateDbConnection()
        {
            var connection = Mysql.MysqlConnectorHelper.CreateConnection("server=localhost;user=root;database=SimpleDataTest;");
            Assert.AreEqual("MySqlConnection", connection.GetType().Name);
        }

        [Test]
        public void CanCreateDataAdapter()
        {
            var adapter = Mysql.MysqlConnectorHelper.CreateDataAdapter("SHOW TABLES",
                                                                         Mysql.MysqlConnectorHelper.CreateConnection(
                                                                             "server=localhost;user=root;database=SimpleDataTest;"));
            Assert.AreEqual("MySqlDataAdapter", adapter.GetType().Name);
        }
    }
}
