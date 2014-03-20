using System;
using System.Collections.Generic;

namespace Simple.Data.Mysql
{
    static class SqlTypeResolver
    {
        private static readonly Dictionary<string, Type> ClrTypes = new Dictionary<string, Type>
                                                                        {
                                                                            {"tinyint", typeof(sbyte)},
                                                                            {"tinyint unsigned", typeof(byte)},
                                                                            {"smallint", typeof(Int16)},
                                                                            {"smallint unsigned", typeof(UInt16)},
                                                                            {"mediumint", typeof(Int32)},
                                                                            {"mediumint unsigned", typeof(UInt32)},
                                                                            {"int", typeof(Int32)},
                                                                            {"int unsigned", typeof(UInt32)},
                                                                            {"bigint", typeof(Int64)},
                                                                            {"bigint unsigned", typeof(UInt64)},
                                                                            {"float", typeof(Single)},
                                                                            {"double", typeof(Double)},
                                                                            {"decimal", typeof(decimal)},
                                                                            {"char", typeof(string)},
                                                                            {"varchar", typeof(string)},
                                                                            {"tinytext", typeof(string)},
                                                                            {"mediumtext", typeof(string)},
                                                                            {"text", typeof(string)},
                                                                            {"longtext", typeof(string)},
                                                                            {"tinyblob", typeof (byte[])},
                                                                            {"mediumblob", typeof (byte[])},
                                                                            {"blob", typeof (byte[])},
                                                                            {"longblob", typeof (byte[])},
                                                                            {"date", typeof(DateTime)},
                                                                            {"datetime", typeof(DateTime)},
                                                                            {"timestamp", typeof(DateTime)},
                                                                            {"time", typeof (DateTime)},
                                                                            {"year", typeof(Int32)},
                                                                            {"enum", typeof(Int32)}
                                                                        };

        public static Type GetClrType(string sqlTypeName)
        {
            Type clrType;
            return ClrTypes.TryGetValue(sqlTypeName, out clrType) ? clrType : typeof(object);
        }
    }
}
