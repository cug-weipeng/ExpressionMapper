using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressAll
{
    public class SelectFields<T>
    {
        private List<string> _Fields = new List<string>();
        public void Select<TValue1>(params Expression<Func<T, TValue1>>[] fields)
        {
            foreach (var field in fields)
            {
                MemberBuilderVisitor visitor = new MemberBuilderVisitor();
                visitor.Visit(field);
                _Fields.Add(visitor.GetMemberName());
            }
        }
        internal SqlModel GetSqlParam()
        {
            return new SqlModel
            {
                SqlText = string.Join(",", _Fields.Select(t => t)),
                Parameters = new Dictionary<string, object>()
            };
        }
    }
}
