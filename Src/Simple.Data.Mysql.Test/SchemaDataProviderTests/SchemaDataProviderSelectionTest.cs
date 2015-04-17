using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;
using Simple.Data.Mysql.SchemaDataProviders;

namespace Simple.Data.Mysql.Test.SchemaDataProviderTests
{
    [TestFixture]
    public class SchemaDataProviderSelectionTest
    {

        private const string ConnectionString = "server=localhost;user=root;database=SimpleDataTest;";

        private string GetServerVersion(IConnectionProvider connectionProvider)
        {
            using (var connection = connectionProvider.CreateConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT VERSION();";
                    connection.OpenIfClosed();
                    return command.ExecuteScalar() as string;
                }
            }
        }

        private ISchemaProvider GetExpectedSchemaProvider(IConnectionProvider connectionProvider)
        {
            var serverVersion = GetServerVersion(connectionProvider);
            if (!string.IsNullOrEmpty(serverVersion) && serverVersion.StartsWith("5"))
                return new MysqlSchemaProvider(connectionProvider, new MysqlSchemaDataProvider50(connectionProvider));
            return new MysqlSchemaProvider(connectionProvider, new MysqlSchemaDataProvider40(connectionProvider));
        }

        [Test]
        public void SelectsExpectedSchemaDataProvider()
        {
            var connectionProvider = new MysqlConnectionProvider();
            connectionProvider.SetConnectionString(ConnectionString);
            var expectedSchemaProvider = GetExpectedSchemaProvider(connectionProvider);
            var selectedSchemaProvider = connectionProvider.GetSchemaProvider();
            //Need to figure out something more elegant here, but so far the implementation of quoting
            //separates the to strategies quite ok.
            Assert.AreEqual(expectedSchemaProvider.QuoteObjectName("name"), selectedSchemaProvider.QuoteObjectName("name"));
        }
    }
}
