using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressAll
{
    public class InsertFields<T>
    {

        private Dictionary<string, object> Parameters = new Dictionary<string, object>();
        public InsertFields<T> Set<TValue1>(Expression<Func<T, TValue1>> e, object value)
        {
            MemberBuilderVisitor visitor = new MemberBuilderVisitor();
            visitor.Visit(e);
            Parameters.Add(visitor.GetMemberName(), value);
            return this;
        }
        internal SqlModel GetSqlParam()
        {
            return new SqlModel
            {
                SqlText = $"({ string.Join(",", Parameters.Select(t =>t.Key).ToArray())}) Values({string.Join(",", Parameters.Select(t => $"@{t.Key}").ToArray())})",
                Parameters = Parameters
            };         
        }
    }
}
