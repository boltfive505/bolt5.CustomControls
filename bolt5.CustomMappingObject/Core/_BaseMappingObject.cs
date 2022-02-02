using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace bolt5.CustomMappingObject.Core
{
    public class _BaseMappingObject<T>
    {
        private static Dictionary<Type, IEnumerable<PropertyInfo>> _cachePropertyInfo = new Dictionary<Type, IEnumerable<PropertyInfo>>();
        protected List<MapItem> PropertyMaps { get; set; }

        public static IEnumerable<PropertyInfo> GetProerties()
        {
            Type type = typeof(T);
            if (!_cachePropertyInfo.ContainsKey(type))
            {
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty)
                            .Where(p => p.GetSetMethod() != null);
                _cachePropertyInfo.Add(type, properties);
            }
            return _cachePropertyInfo[type];
        }

        protected static bool DoMatchColumnName(ColumnMappingAttribute columnMapping, string columnName)
        {
            switch (columnMapping.Comparison)
            {
                case ColumnMappingComparison.Exact:
                    return columnName == columnMapping.ColumnName;
                case ColumnMappingComparison.Contains:
                    return columnName.Contains(columnMapping.ColumnName);
                case ColumnMappingComparison.StartsWith:
                    return columnName.StartsWith(columnMapping.ColumnName);
                case ColumnMappingComparison.EndsWith:
                    return columnName.EndsWith(columnMapping.ColumnName);
                default:
                    return false;
            }
        }

        public void SetValues(ref T item, Func<int, object> callback)
        {
            foreach (var pm in PropertyMaps)
            {
                if (pm.isNotMapped) return;
                if (pm.columnIndex == -1)
                {
                    if (pm.skipIfMissing) continue;
                    throw new IndexOutOfRangeException("Mapping for property '" + pm.property.Name + "' not found.");
                }
                Type t = Nullable.GetUnderlyingType(pm.property.PropertyType) ?? pm.property.PropertyType;
                object value = callback.Invoke(pm.columnIndex);

                //early check if DbNull
                if (Convert.IsDBNull(value))
                    value = null;

                //check if parse method
                if (pm.parsingMethod != null)
                    value = pm.parsingMethod.Invoke(null, new object[] { Convert.ToString(value) });

                //check if null string
                if (value != null && value.GetType() == typeof(string) && string.IsNullOrEmpty(Convert.ToString(value)))
                    value = null;

                object convertedValue = null;
                try
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        convertedValue = null;
                    }
                    else
                    {
                        if (t.Equals(typeof(int))) //only do this because of decimal point string error
                            value = Convert.ToDecimal(value, CultureInfo.InvariantCulture);

                        //check if same type
                        if (value.GetType().Equals(t))
                        {
                            //same type, just assign value normally
                            convertedValue = value;
                        }
                        else if (t.Equals(typeof(string)))
                        {
                            //converting to string
                            convertedValue = Convert.ToString(value);
                        }
                        else
                        {
                            //different type, try to convert
                            try
                            {
                                //method 1
                                convertedValue = Convert.ChangeType(value, t, CultureInfo.InvariantCulture);
                            }
                            catch
                            {
                                //method 2
                                TypeConverter converter = TypeDescriptor.GetConverter(t);
                                convertedValue = converter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!pm.skipIfConvertError)
                        throw new Exception("(MappingObject) " + ex.Message, ex);
                }
                pm.property.SetValue(item, convertedValue, null);
            }
        }

        public void GetValues(T item, Action<int, object> get)
        {
            GetValuesWithType(item, (i, type, obj) => get.Invoke(i, obj));
        }

        public void GetValuesWithType(T item, Action<int, Type, object> get)
        {
            if (item == null) return;
            foreach (var pm in PropertyMaps)
            {
                if (pm.isNotMapped) return;
                if (pm.columnIndex == -1)
                {
                    if (pm.skipIfMissing) continue;
                    throw new IndexOutOfRangeException("Mapping for property '" + pm.property.Name + "' not found.");
                }
                object value = pm.property.GetValue(item, null);
                Type type = Nullable.GetUnderlyingType(pm.property.PropertyType) ?? pm.property.PropertyType;
                get.Invoke(pm.columnIndex, type, value);
            }
        }

        public object GetValue(T item, Expression<Func<T, object>> expression)
        {
            if (item == null) return null;
            try
            {
                var method = expression.Compile();
                return method?.Invoke(item);
            }
            catch (NullReferenceException)
            {
                return null;
            }
            //PropertyInfo property = null;
            //if (expression.Body is MemberExpression)
            //{
            //    property = ((MemberExpression)expression.Body).Member as PropertyInfo;
            //}
            //else
            //{
            //    var op = ((UnaryExpression)expression.Body).Operand;
            //    property = ((MemberExpression)op).Member as PropertyInfo;
            //}
            //if (property == null) return null;
            //return property?.GetValue(item, null);
        }

        public int IndexOf(Expression<Func<T, object>> expression)
        {
            PropertyInfo property = null;
            if (expression.Body is MemberExpression)
            {
                property = ((MemberExpression)expression.Body).Member as PropertyInfo;
            }
            else
            {
                var op = ((UnaryExpression)expression.Body).Operand;
                property = ((MemberExpression)op).Member as PropertyInfo;
            }
            if (property == null) return -1;
            var map = PropertyMaps.FirstOrDefault(i => i.property == property);
            if (map == null) return -1;
            return map.columnIndex;
        }
    }
}
