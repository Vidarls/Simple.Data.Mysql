using NUnit.Framework;

namespace Simple.Data.Mysql.Mysql40Test
{
    [TestFixture]
    public class MysqlConnectorHelperTest
    {
        [Test]
        public void CanCreateDbConnection()
        {
            var connection = Mysql40.MysqlConnectorHelper.CreateConnection("server=localhost;user=root;database=SimpleDataTest;");
            Assert.AreEqual("MySqlConnection", connection.GetType().Name);
        }

        [Test]
        public void CanCreateDataAdapter()
        {
            var adapter = Mysql40.MysqlConnectorHelper.CreateDataAdapter("SHOW TABLES",
                                                                         Mysql40.MysqlConnectorHelper.CreateConnection(
                                                                             "server=localhost;user=root;database=SimpleDataTest;"));
            Assert.AreEqual("MySqlDataAdapter", adapter.GetType().Name);
        }
    }
}
