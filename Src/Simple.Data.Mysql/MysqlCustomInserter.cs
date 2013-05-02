using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.Text;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Mysql
{
    [Export(typeof(ICustomInserter))]
    public class MysqlCustomInserter : ICustomInserter 
    {
        public IDictionary<string, object> Insert(AdoAdapter adapter, string tableName, IDictionary<string, object> data, IDbTransaction transaction = null,
                                  bool resultRequired = false)
        {
            var table = adapter.GetSchema().FindTable(tableName);
            var dataDictionary = BuildDataDictionary(adapter, data, table);

            string columnList = dataDictionary.Keys.Select(c => c.QuotedName).Aggregate((agg, next) => agg + "," + next);
            string valueList = dataDictionary.Keys.Select(s => "?").Aggregate((agg, next) => agg + "," + next);

            var insertSql = new StringBuilder();
            insertSql.AppendFormat("INSERT INTO {0} ({1}) VALUES ({2});", table.QualifiedName, columnList, valueList);
            if (resultRequired)
            {
                var identityColumn = table.Columns.FirstOrDefault(c => c.IsIdentity);
                if (identityColumn != null)
                {
                    insertSql.AppendFormat(" SELECT * FROM {0} WHERE {1} = LAST_INSERT_ID();", table.QualifiedName,
                                           identityColumn.QuotedName);
                    return ExecuteSingletonQuery(adapter, insertSql.ToString(), dataDictionary.Keys,
                                                 dataDictionary.Values, transaction);
                }
            }
            Execute(adapter, insertSql.ToString(), dataDictionary.Keys, dataDictionary.Values, transaction);
            return null;
        }

        private static Dictionary<Column, object> BuildDataDictionary(AdoAdapter adapter,
                                                                      IDictionary<string, object> data, Table table)
        {
            Func<string, bool> columnFilter = key =>
                                              table.HasColumn(key) &&
                                              (table.FindColumn(key).IsWriteable ||
                                               ((adapter.AdoOptions != null && adapter.AdoOptions.IdentityInsert) && table.FindColumn(key).IsIdentity));
            
            return data.Where(kvp => columnFilter(kvp.Key))
                       .ToDictionary(kvp => table.FindColumn(kvp.Key), kvp => kvp.Value);

        } 

        private static IDictionary<string, object> ExecuteSingletonQuery(AdoAdapter adapter, string sql,
                                                                         IEnumerable<Column> columns,
                                                                         IEnumerable<object> values,
                                                                         IDbTransaction transaction)
        {
            if (transaction != null)
            {
                var command = new CommandHelper(adapter).CreateInsert(transaction.Connection, sql, columns,
                                                                      values.ToArray());
                command.Transaction = transaction;
                return TryExecuteSingletonQuery(command);
            }
            var connection = adapter.CreateConnection();
            using (connection.MaybeDisposable())
            {
                using (var command = new CommandHelper(adapter).CreateInsert(connection,sql,columns, values.ToArray()))
                {
                    connection.OpenIfClosed();
                    return TryExecuteSingletonQuery(command);
                }
            }
        } 

        private static IDictionary<string, object> TryExecuteSingletonQuery(IDbCommand command)
        {
            using (var reader = command.TryExecuteReader())
            {
                if (reader.Read())
                    return reader.ToDictionary();
            }
            return null;
        } 

        private static int Execute(AdoAdapter adapter, string sql, IEnumerable<Column> columns, IEnumerable<Object> values,
                            IDbTransaction transaction)
        {
            if (transaction != null)
            {
                var command = new CommandHelper(adapter).CreateInsert(transaction.Connection, sql, columns,
                                                                      values.ToArray());
                command.Transaction = transaction;
                return command.TryExecuteNonQuery();
            }
            var connection = adapter.CreateConnection();
            using (connection.MaybeDisposable())
            {
                using (var command = new CommandHelper(adapter).CreateInsert(connection, sql, columns, values.ToArray())
                    )
                {
                    connection.OpenIfClosed();
                    return command.TryExecuteNonQuery();
                }
            }
        }
    }
}
