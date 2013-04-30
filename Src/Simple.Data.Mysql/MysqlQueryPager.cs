using Simple.Data.Ado;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;

namespace Simple.Data.Mysql
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
