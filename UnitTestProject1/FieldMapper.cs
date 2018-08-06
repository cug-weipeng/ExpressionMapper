using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject1
{
    class SelectedField<T>
    {
        public SelectedField<T> Set<TValue1>(Expression<Func<T, TValue1>> e,object value)
        {
            return this;
        }
    }
   
}
