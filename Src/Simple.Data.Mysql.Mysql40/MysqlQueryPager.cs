using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Simple.Data.Ado;

namespace Simple.Data.Mysql.Mysql40
{
    [Export(typeof(IQueryPager))]
    public class MysqlQueryPager : IQueryPager
    {
        public IEnumerable<string> ApplyLimit(string sql, int take)
        {
            var sb = new StringBuilder(sql);
            sb.AppendFormat(" LIMIT {0}", take);
            yield return sb.ToString();
        }

        public IEnumerable<string> ApplyPaging(string sql, string[] keys, int skip, int take)
        {
            var sb = new StringBuilder(sql);
            sb.AppendFormat(" LIMIT {0}, {1}", skip, take);
            yield return sb.ToString();
        }
    }
}
