using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Simple.Data.Mysql.Mysql40;
using NUnit.Framework;

namespace Simple.Data.Mysql.Mysql40Test.SchemaTests
{
    public class ColumnInfoCreatorTest
    {
        [Test]
        public void ParsesTypeFromTypeinfoWithCapacity()
        {
            var columnInfo = MysqlColumnInfoCreator.CreateColumnInfo("", "", "varchar(255)");
            Assert.AreEqual(DbType.String,columnInfo.DbType, "Dbtype is not correct");
        }

        [Test]
        public void ParsesCapacityFromTypeinfoWithCapacity()
        {
            var columnInfo = MysqlColumnInfoCreator.CreateColumnInfo("", "", "varchar(255)");
            Assert.AreEqual(255,columnInfo.Capacity, "Capacity is not correct");
        }

        [Test]
        public void ParsesTypeFromTypeinfoWithoutCapacity()
        {
            var columnInfo = MysqlColumnInfoCreator.CreateColumnInfo("", "", "timestamp");
            Assert.AreEqual(DbType.DateTime,columnInfo.DbType, "Dbtype is not correct");
        }

        [Test]
        public void CapacityShouldBeZeroWhenParsingFromTypeinfoWithoutCapacity()
        {
            var columnInfo = MysqlColumnInfoCreator.CreateColumnInfo("", "", "timestamp");
            Assert.AreEqual(0,columnInfo.Capacity, "Capacity is not zero");
        }
    }
}
