using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressAll
{
    internal class ConditionBuilderVisitor : ExpressionVisitor
    {
        private Stack<string> _StringStack = new Stack<string>();
        private SqlModel _SqlModel = new SqlModel()
        {
            Parameters = new Dictionary<string, object>()
        };

        public SqlModel Condition()
        {
            _SqlModel.SqlText = string.Concat(this._StringStack.ToArray());
            this._StringStack.Clear();
            return _SqlModel;
        }

        /// <summary>
        /// 如果是二元表达式
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node == null) throw new ArgumentNullException("BinaryExpression");

            this._StringStack.Push(")");
            base.Visit(node.Right);//解析右边
            this._StringStack.Push(" " + node.NodeType.ToSqlOperator() + " ");
            base.Visit(node.Left);//解析左边
            this._StringStack.Push("(");

            return node;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node == null) throw new ArgumentNullException("MemberExpression");
            if ((node.Expression.GetType().Name != "TypedParameterExpression"))
            {
                UnaryExpression cast = Expression.Convert(node, typeof(object));
                object obj = Expression.Lambda<Func<object>>(cast).Compile().Invoke();
                string paramName = $"@param{_SqlModel.Parameters.Count}";
                this._StringStack.Push(paramName);
                _SqlModel.Parameters.Add(paramName, obj);
            }
            else
            {
                this._StringStack.Push(" [" + node.Member.Name + "] ");
            }
            return node;
        }

        /// <summary>
        /// 常量表达式
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node == null) throw new ArgumentNullException("ConstantExpression");
            string paramName = $"@param{_SqlModel.Parameters.Count}";
            this._StringStack.Push(paramName);
            _SqlModel.Parameters.Add(paramName, node.Value);
            //this._StringStack.Push(" '" + node.Value + "' ");
            return node;
        }
        /// <summary>
        /// 方法表达式
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m == null) throw new ArgumentNullException("MethodCallExpression");

            string format;
            switch (m.Method.Name)
            {
                case "StartsWith":
                    format = "({0} LIKE {1}+'%')";
                    break;

                case "Contains":
                    format = "({0} LIKE '%'+{1}+'%')";
                    break;

                case "EndsWith":
                    format = "({0} LIKE '%'+{1})";
                    break;

                default:
                    throw new NotSupportedException(m.NodeType + " is not supported!");
            }
            this.Visit(m.Object);
            this.Visit(m.Arguments[0]);
            string right = this._StringStack.Pop();
            string left = this._StringStack.Pop();
            this._StringStack.Push(String.Format(format, left, right));

            return m;
        }

    }
}
