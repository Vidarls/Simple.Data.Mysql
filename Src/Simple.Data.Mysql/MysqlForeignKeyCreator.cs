using Simple.Data.Ado;
using Simple.Data.Ado.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.Data.Mysql
{
    internal static class MysqlForeignKeyCreator
    {
        public static IEnumerable<ForeignKey> ExtractForeignKeysFromCreateTableSql(String tableName, String createTableSql, Boolean ansiQuotes, Boolean useBackslashEscaping)
        {
            var tokens = SqlTokenizer.Tokenize(createTableSql, ansiQuotes, useBackslashEscaping);
            var enumerator = tokens.GetEnumerator();
            do
            {
                var sequenceEnded = false;
                enumerator.MoveUntil(token => String.Equals(token, "constraint", StringComparison.OrdinalIgnoreCase), out sequenceEnded);
                if (sequenceEnded)
                {
                    break;
                }
                var constraintName = enumerator.GetNext();
                if (String.Equals(enumerator.GetNext(), "foreign", StringComparison.OrdinalIgnoreCase))
                {
                    enumerator.MoveUntil(current => current == "(").MoveNext();
                    var columns = enumerator.GetNextUntil(current => current == ")").Where(item => item != ",");
                    enumerator.MoveUntil(current => String.Equals(current, "references", StringComparison.OrdinalIgnoreCase));
                    var referencedTableName = enumerator.GetNext();
                    if (enumerator.GetNext() == ".")
                    {
                        referencedTableName = enumerator.GetNext();
                    }
                    enumerator.MoveUntil(current => current == "(").MoveNext();
                    var referencedColumns = enumerator.GetNextUntil(current => current == ")").Where(item => item != ",");
                    yield return new ForeignKey(new ObjectName(null, tableName), columns,
                                                new ObjectName(null, referencedTableName), referencedColumns);
                }
            }
            while (enumerator.MoveNext());
        }
    }
}
