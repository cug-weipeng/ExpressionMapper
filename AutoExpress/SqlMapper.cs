using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;

namespace ExpressAll
{
    public static class SqlMapper
    {
        public static List<T> TransformTo<T>(this IDataReader dataReader)
        {
            List<T> result = new List<T>();
            while (dataReader.Read())
            {
                result.Add(GernericMapper<T>.ReaderToEntity(dataReader));
            }
            return result;
        }


        public static List<T> Select<T>(Action<SelectFields<T>> selectFields, Expression<Func<T, bool>> where)
        {
            SelectFields<T> select = new SelectFields<T>();
            selectFields.Invoke(select);
            SqlModel sqlSelect = select.GetSqlParam();
            SqlModel sqlWhere =  GernericMapper<T>.GetWhereSql(where);
            string sql = $"select {sqlSelect.SqlText} From [{GernericMapper<T>.GetTableName()}] where {sqlWhere.SqlText}";
            string connectionString = "Data Source=HGH1-DEV01.qqtoa.com;Initial Catalog=CpsMain;Persist Security Info=True;User ID=ReadOnlyUser;Password=ReadOnly@1234";
            using (IDbConnection conn = new SqlConnection(connectionString))
            {
                DateTime dt1 = DateTime.Now;
                conn.Open();
                var command = conn.CreateCommand();
                command.CommandText = sql;
                foreach(var pair in sqlWhere.Parameters)
                {
                    command.Parameters.Add(new SqlParameter(pair.Key, pair.Value));
                }
                return command.ExecuteReader().TransformTo<T>();

            }
        }
        public static void Update<T>(this T t, SelectFields<T> selectFields, Expression<Func<T, bool>> where)
        {

        }
    }
}
