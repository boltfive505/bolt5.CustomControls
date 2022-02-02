using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Globalization;

namespace bolt5.FieldExpressions
{
    public class FieldExpression<T>
    {
        private Expression<Func<T, object>> _expression;
        private string _fieldName;
        private string _formatString;
        private Func<T, bool> _predicate;

        public FieldExpression(Expression<Func<T, object>> expression, Func<T, bool> predicate, string fieldName, string formatString)
        {
            this._expression = expression;
            this._predicate = predicate;
            this._fieldName = fieldName;
            this._formatString = formatString;
        }

        public string GetFieldName()
        {
            if (!string.IsNullOrEmpty(_fieldName))
                return _fieldName;
            else
            {
                if (_expression.Body is MemberExpression)
                {
                    return ((MemberExpression)_expression.Body).Member.Name;
                }
                else
                {
                    var op = ((UnaryExpression)_expression.Body).Operand;
                    return ((MemberExpression)op).Member.Name;
                }
            }
        }

        public object GetValue(T parent)
        {
            try
            {
                //check for condition
                if (_predicate != null && !_predicate(parent))
                    return null;
            }
            catch (NullReferenceException)
            { }

            object value = null;
            try
            {
                //get value from expression
                var method = _expression.Compile();
                value = method(parent);
            }
            catch (NullReferenceException)
            { }
            if (value == null) return null;
            Type type = value.GetType();
            if (Nullable.GetUnderlyingType(type) != null)
                type = Nullable.GetUnderlyingType(type);
            if (type == typeof(int) || type == typeof(long) || type == typeof(double) || type == typeof(decimal))
            {
                //if value is number value, do nothing, leave as is
            }
            else
            {
                value = GetFormattedStringValue(value);
            }
            return value;
        }

        public string GetStringValue(T parent)
        {
            try
            {
                //check for condition
                if (_predicate != null && !_predicate(parent))
                    return null;
            }
            catch (NullReferenceException)
            { }

            object value = null;
            try
            {
                //get value from expression
                var method = _expression.Compile();
                value = method(parent);
            }
            catch (NullReferenceException)
            { }

            if (value != null)
            {
                value = GetFormattedStringValue(value);
            }
            return Convert.ToString(value);
        }

        private string GetFormattedStringValue(object value)
        {
            IFormattable formattable = value as IFormattable;
            if (formattable != null)
            {
                string format = !string.IsNullOrEmpty(this._formatString) ? this._formatString : GetFormatStringForType(value.GetType());
                return formattable.ToString(format, CultureInfo.InvariantCulture);
            }
            return Convert.ToString(value);
        }

        private static string GetFormatStringForType(Type type)
        {
            if (type != null)
            {
                if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
                    return "#.#0";
                else if (type == typeof(DateTime))
                    return "M/d/yyyy";
                else if (type == typeof(TimeSpan))
                    return "c";
            }
            return "";
        }
    }
}
