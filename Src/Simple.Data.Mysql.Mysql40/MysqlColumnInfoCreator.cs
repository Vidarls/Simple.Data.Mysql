using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace Simple.Data.Mysql.Mysql40
{
    public class MysqlColumnInfo
    {
        public string Name { get; private set; }
        public bool IsAutoincrement { get; private set; }
        public DbType DbType { get; private set; }
        public int Capacity { get; private set; }

        public MysqlColumnInfo(string name, bool isAutoincrement, DbType dbType, int capacity)
        {
            Name = name;
            IsAutoincrement = isAutoincrement;
            DbType = dbType;
            Capacity = capacity;
        }   
    }

    public static class MysqlColumnInfoCreator
    {
        //ref:
        //http://dev.mysql.com/tech-resources/articles/visual-basic-datatypes.html
        //http://dev.mysql.com/doc/refman/5.1/en/c-api-prepared-statement-type-codes.html
        private static readonly Dictionary<string, DbType> DbTypes = new Dictionary<string, DbType>
                                                                        {
                                                                            {"tinyint", DbType.SByte},
                                                                            {"tinyint unsigned", DbType.Byte},
                                                                            {"smallint", DbType.Int16},
                                                                            {"smallint unsigned", DbType.UInt16},
                                                                            {"mediumint", DbType.Int32},
                                                                            {"mediumint unsigned", DbType.UInt32},
                                                                            {"int", DbType.Int32},
                                                                            {"int unsigned", DbType.UInt32},
                                                                            {"bigint", DbType.UInt64},
                                                                            {"float", DbType.Single},
                                                                            {"double", DbType.Double},
                                                                            {"decimal", DbType.Decimal},
                                                                            {"char", DbType.StringFixedLength},
                                                                            {"varchar", DbType.String},
                                                                            {"tinytext", DbType.String},
                                                                            {"mediumtext", DbType.String},
                                                                            {"text", DbType.String},
                                                                            {"longtext", DbType.String},
                                                                            {"tinyblob", DbType.Binary},
                                                                            {"mediumblob", DbType.Binary},
                                                                            {"blob", DbType.Binary},
                                                                            {"longblob", DbType.Binary},
                                                                            {"date", DbType.Date},
                                                                            {"datetime", DbType.DateTime},
                                                                            {"timestamp", DbType.DateTime},
                                                                            {"time", DbType.Time},
                                                                            {"year", DbType.Int16},
                                                                            {"enum", DbType.String},
                                                                            {"set", DbType.String}
                                                                        };

        private static DbType GetDbType(string sqlTypeName)
        {
            DbType clrType;
            return DbTypes.TryGetValue(sqlTypeName, out clrType) ? clrType : DbType.String;
        }

        public static MysqlColumnInfo CreateColumnInfo(string fieldColumnValue, string extraColumnValue, string typeColumnValue)
        {
            var columnName = fieldColumnValue;
            var isAutoIncrement = DetermineIsAutoincrement(extraColumnValue);
            
            DbType type;
            int capacity;
            ParseTypeInfo(typeColumnValue, out type, out capacity);

            return new MysqlColumnInfo(columnName, isAutoIncrement, type, capacity);
        }

        private static bool DetermineIsAutoincrement(string extraColumnValue)
        {
            return extraColumnValue.ToUpper().Contains("AUTO_INCREMENT");
        }

        private static void ParseTypeInfo(string typeColumnValue, out DbType type, out int capacity)
        {
            //default values 
            type = DbType.Object;
            capacity = 0;

            //typeinfo comes in two flavours:
            //typename(capacity) and plain typename
            //capture group 1 captures typename
            //capture group 2 captures capacity if present
            var regex = new Regex(@"^([^(]+)(?:\((\d+)\))?$");
            var match = regex.Match(typeColumnValue);

            if (match.Groups[1].Success)
                type = GetDbType(match.Groups[1].Value);

            if (match.Groups[2].Success)
                capacity = int.Parse(match.Groups[2].Value);
         
        }
    }
}
