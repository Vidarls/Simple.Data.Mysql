using System.Data;
using NUnit.Framework;

namespace Simple.Data.Mysql.Test
{
    [TestFixture]
    public class MySqlColumnInfoTest
    {
        [Test]
        public void DecimalColumnWithCapacityAndPrecisionParsedToDecimal()
        {
            var columnInfo = MysqlColumnInfo.CreateColumnInfo("Price", "Price", "decimal(10,2)", "price");
            Assert.True(columnInfo.DbType == DbType.Decimal);
            Assert.True(columnInfo.Capacity == 10);
        }

        [Test]
        public void DecimalColumnWithoutCapacityParsedToDecimal()
        {
            var columnInfo = MysqlColumnInfo.CreateColumnInfo("Price", "Price", "decimal", "price");
            Assert.True(columnInfo.DbType == DbType.Decimal);
        }

        [Test]
        public void DecimalColumnWithCapacityWithoutPrecisionParsedToDecimal()
        {
            var columnInfo = MysqlColumnInfo.CreateColumnInfo("Price", "Price", "decimal(10)", "price");
            Assert.True(columnInfo.DbType == DbType.Decimal);
            Assert.True(columnInfo.Capacity == 10);
        }
        
    }
}