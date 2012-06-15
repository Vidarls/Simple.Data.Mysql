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
            var columnInfo = MysqlColumnInfo.CreateColumnInfo("", "", "varchar(255)","");
            Assert.AreEqual(DbType.String,columnInfo.DbType, "Dbtype is not correct");
        }

        [Test]
        public void ParsesCapacityFromTypeinfoWithCapacity()
        {
            var columnInfo = MysqlColumnInfo.CreateColumnInfo("", "", "varchar(255)","");
            Assert.AreEqual(255,columnInfo.Capacity, "Capacity is not correct");
        }

        [Test]
        public void ParsesTypeFromTypeinfoWithoutCapacity()
        {
            var columnInfo = MysqlColumnInfo.CreateColumnInfo("", "", "timestamp","");
            Assert.AreEqual(DbType.DateTime,columnInfo.DbType, "Dbtype is not correct");
        }

        [Test]
        public void CapacityShouldBeZeroWhenParsingFromTypeinfoWithoutCapacity()
        {
            var columnInfo = MysqlColumnInfo.CreateColumnInfo("", "", "timestamp","");
            Assert.AreEqual(0,columnInfo.Capacity, "Capacity is not zero");
        }

        [Test]
        public void SholdBePrimaryKeyWhenPassedPriAsLastParameter()
        {
            var columnInfo = MysqlColumnInfo.CreateColumnInfo("", "", "int(11)", "PRI");
            Assert.True(columnInfo.IsPrimaryKey, "Column should have been designated as primaryKey");
        }

        [Test]
        public void SholdNotBePrimaryKeyWhenNotPassedPriAsLastParameter()
        {
            var columnInfo = MysqlColumnInfo.CreateColumnInfo("", "", "int(11)", "");
            Assert.False(columnInfo.IsPrimaryKey, "Column should not have been designated as primaryKey");
        }
    }
}
