using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpressAll
{
    internal static class GernericMapper<T>
    {
        private static EntityDescription entity = null;
        static GernericMapper()
        {
            Type type = typeof(T);
            entity = new EntityDescription()
            {
                Name = type.Name,
                TableName = (type.GetCustomAttribute(typeof(TableAttribute)) is TableAttribute attrTable) ? attrTable.Name : type.Name,
                Properties = new List<PropertyDescription>()
            };

            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                PropertyDescription propertyDescription = new PropertyDescription
                {
                   Property = prop
                };
                ColumnAttribute column = prop.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;
                propertyDescription.FieldName = (column!=null&&column.Name!=null)?column.Name:prop.Name;
                propertyDescription.IsKey = prop.GetCustomAttribute(typeof(KeyAttribute)) is KeyAttribute;
                propertyDescription.IsAutoincrease = (prop.GetCustomAttribute(typeof(DatabaseGeneratedAttribute)) is DatabaseGeneratedAttribute att)? att.DatabaseGeneratedOption == DatabaseGeneratedOption.Computed:false; 

                entity.Properties.Add(propertyDescription);
            }
        }
        private static Func<IDataReader,T> _FUNC_ReaderToEntity= null;
        public static T ReaderToEntity(IDataReader dr)
        {
            if (_FUNC_ReaderToEntity == null)
            {
                ParameterExpression p = Expression.Parameter(typeof(IDataReader), "p");
                List<MemberBinding> bingdingList = new List<MemberBinding>();
                foreach (PropertyDescription propertyDescription in entity.Properties)
                {
                    MethodCallExpression method = Expression.Call(
                        p,
                        typeof(IDataRecord).GetMethod("get_Item", new Type[] { typeof(string) }),
                        new Expression[] { Expression.Constant(propertyDescription.FieldName, typeof(string)) }
                    );//调用IDataReader的索引器查找每个指定的property
                    Expression convertMethod = Expression.Convert(method, typeof(object));
                    object defalutValue = GetTypeDefualt(propertyDescription.Property.PropertyType);
                    Expression defalutExpr = Expression.Convert(Expression.Constant(defalutValue), typeof(object));
                    var DbNullExpr = Expression.Constant(DBNull.Value);
                    var isDbNull = Expression.Equal(convertMethod, DbNullExpr);
                    var conditionExpr = Expression.Condition(isDbNull, defalutExpr, convertMethod);
                    var convertExpr = Expression.Convert(conditionExpr, propertyDescription.Property.PropertyType);
                    MemberBinding memberBinding = Expression.Bind(propertyDescription.Property, convertExpr);
                    bingdingList.Add(memberBinding);
                }
                MemberInitExpression memberInitExpression = Expression.MemberInit(
                    Expression.New(typeof(T)),
                    bingdingList.ToArray());

                _FUNC_ReaderToEntity = Expression.Lambda<Func<IDataReader, T>>(memberInitExpression, new ParameterExpression[] { p }).Compile();
            }
            T t = _FUNC_ReaderToEntity.Invoke(dr);
            return t;
        }
        private static object GetTypeDefualt(Type t)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                return null;
            }
            var code = Type.GetTypeCode(t);
            switch (code)
            {
                case TypeCode.Boolean:
                    return default(bool);
                case TypeCode.Byte:
                    return default(byte);
                case TypeCode.DateTime:
                    return default(DateTime);
                case TypeCode.Decimal:
                    return default(decimal);
                case TypeCode.Double:
                    return default(double);
                case TypeCode.Int16:
                    return default(Int16);
                case TypeCode.Int32:
                    return default(Int32);
                case TypeCode.Int64:
                    return default(Int64);
                case TypeCode.SByte:
                    return default(SByte);
                case TypeCode.Single:
                    return default(Single);
                case TypeCode.String:
                    return default(String);
                case TypeCode.UInt16:
                    return default(UInt16);
                case TypeCode.UInt32:
                    return default(UInt32);
                case TypeCode.UInt64:
                    return default(UInt64);
                case TypeCode.Object:
                    return null;
                default:
                    return null;
            }
        }
                
        private static Func<string> _FUNC_Insert = null;
        public static string GetInsertFunc(Func<string, string> formatFunc = null)
        {
            if (_FUNC_Insert == null)
            {
                string sql = null;
                if (formatFunc == null)
                    sql = $"insert into {entity.TableName} " +
                        $"({string.Join(",", entity.Properties.Where(t => !t.IsAutoincrease).Select(a => $"{a.FieldName}"))}) " +
                        $"values ({string.Join(",", entity.Properties.Where(t => !t.IsAutoincrease).Select(a => $"@{a.FieldName}"))})";
                else
                    sql = $"insert into {entity.TableName} " +
                        $"({string.Join(",", entity.Properties.Where(t => !t.IsAutoincrease).Select(a => $"{formatFunc(a.FieldName)}"))}) " +
                        $"values ({string.Join(",", entity.Properties.Where(t => !t.IsAutoincrease).Select(a => $"@{formatFunc(a.FieldName)}"))})";

                ConstantExpression sqlExpression = Expression.Constant(sql, typeof(string));
                var lambda = Expression.Lambda<Func<string>>(sqlExpression);
                _FUNC_Insert = lambda.Compile();
            }
            return _FUNC_Insert();
        }

        private static Func<T, Dictionary<string, object>> _FUNC_ParamDic = null;
        private static Expression ConvertToDbNullExpression(ParameterExpression parameterExpression, PropertyInfo prop)
        {
            Expression propertyExpress = Expression.Property(parameterExpression, prop.Name);
            //object defalutValue = GetTypeDefualt(prop.PropertyType);
            Expression defalutExpr = Expression.Constant(null);

            var isNullExpr = Expression.Call(
                typeof(Object).GetMethod("Equals", new Type[] { typeof(Object), typeof(Object) }),
                Expression.Convert(propertyExpress, typeof(object)),
                Expression.Convert(defalutExpr, typeof(object)));

            var DbNullExpr = Expression.Constant(null);
            //var DbNullExpr = Expression.Constant(DBNull.Value);

            return Expression.Condition(isNullExpr,
                Expression.Convert(DbNullExpr, typeof(object)),
                Expression.Convert(propertyExpress, typeof(object)));
        }
        public static Dictionary<string, object> GetAllParamDic(T t)
        {
            if (_FUNC_ParamDic == null)
            {
                ParameterExpression paraExpression = Expression.Parameter(typeof(T), "p");
                var paramList = new List<MemberInitExpression>();
                var elementList = new List<ElementInit>();

                foreach (var property in entity.Properties)
                {
                    if (property.IsAutoincrease)
                        continue;
                    ConstantExpression constantExpression = Expression.Constant($"@{property.FieldName}");

                    var valueExpression = ConvertToDbNullExpression(paraExpression, property.Property);
                    MethodInfo methodInfo = typeof(Dictionary<string, object>).GetMethod("Add", new Type[] {
                         typeof(string),typeof(object)
                    });
                    var initExp = Expression.ElementInit(methodInfo, new Expression[]
                       {
                            Expression.Constant(property.Property,typeof(string)),
                            Expression.Convert( valueExpression,typeof(object))
                       });
                    elementList.Add(initExp);
                }
                var ListInitExpp = Expression.ListInit(Expression.New(typeof(Dictionary<string, object>)), elementList.ToArray());

                var lambda = Expression.Lambda<Func<T, Dictionary<string, object>>>(ListInitExpp, new ParameterExpression[]
                {
                    paraExpression
                });
                _FUNC_ParamDic = lambda.Compile();
            }
            Dictionary<string, object> dbParams = _FUNC_ParamDic.Invoke(t);
            return dbParams;
        }
        public static SqlModel GetWhereSql(Expression<Func<T, bool>> predicate)
        {
            ConditionBuilderVisitor visitor = new ConditionBuilderVisitor();
            visitor.Visit(predicate);
            return visitor.Condition();
        }
        public static string GetTableName()
        {
            return entity.TableName;
        }
    }
}
