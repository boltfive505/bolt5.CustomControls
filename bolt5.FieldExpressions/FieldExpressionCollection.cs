using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace bolt5.FieldExpressions
{
    public class FieldExpressionCollection<T> : List<FieldExpression<T>>
    {
        public void Add(Expression<Func<T, object>> expression)
        {
            this.Add(expression, null, null, null);
        }

        public void Add(Expression<Func<T, object>> expression, string fieldName)
        {
            this.Add(expression, null, fieldName, null);
        }

        public void Add(Expression<Func<T, object>> expression, Func<T, bool> predicate, string fieldName)
        {
            this.Add(expression, predicate, fieldName, null);
        }

        public void Add(Expression<Func<T, object>> expression, string fieldName, string formatString)
        {
            this.Add(expression, null, fieldName, formatString);
        }

        public void Add(Expression<Func<T, object>> expression, Func<T, bool> predicate, string fieldName, string formatString)
        {
            this.Add(new FieldExpression<T>(expression, predicate, fieldName, formatString));
        }
    }
}
