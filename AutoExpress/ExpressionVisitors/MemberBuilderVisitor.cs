using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressAll
{
    internal class MemberBuilderVisitor : ExpressionVisitor
    {
        string _MemberName =null;

        protected override Expression VisitLambda<T>(Expression<T> node)
        { 
            if(node.Body.NodeType != ExpressionType.MemberAccess)
                throw new ArgumentException("only MemeberExpression Allowed");

            return base.VisitLambda(node);
        }
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("MemberExpression");
            }
            if ((node.Expression.GetType().Name == "TypedParameterExpression"))
            {
                _MemberName = node.Member.Name;
                return node;
            }
            return null;
        }
        public string GetMemberName()
        {
            return _MemberName;
        }
    }
}
