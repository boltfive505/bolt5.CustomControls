using System;
using System.Linq;
using System.Reflection;

namespace bolt5.CustomMappingObject.Core
{
    public class MapItem
    {
        public PropertyInfo property;
        public ColumnMappingAttribute[] columnMappings;
        public int columnIndex;
        public bool isNotMapped;
        public bool skipIfMissing;
        public bool skipIfConvertError;
        public MethodInfo parsingMethod;

        public MapItem(PropertyInfo property)
        {
            this.property = property;
            columnMappings = new ColumnMappingAttribute[0];
            columnIndex = -1;

            //get column names
            var attrs = property.GetCustomAttributes(typeof(ColumnMappingAttribute), false);
            if (attrs == null || attrs.Length == 0)
                columnMappings = new ColumnMappingAttribute[] { new ColumnMappingAttribute(property.Name, ColumnMappingComparison.Exact) }; //get by property name, exact comparison
            else
                columnMappings = attrs.OfType<ColumnMappingAttribute>().ToArray(); //get by column attribute

            //get parse method
            var parseMethodAttr = property.GetCustomAttribute(typeof(MappingParseMethodAttribute), false) as MappingParseMethodAttribute;
            if (parseMethodAttr != null)
            {
                parsingMethod = property.DeclaringType.GetMethod(parseMethodAttr.MethodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            }

            //get other attributes
            isNotMapped = property.GetCustomAttribute(typeof(IgnoreMappingAttribute), false) != null;
            skipIfMissing = property.GetCustomAttribute(typeof(SkipMappingIfMissingAttribute), false) != null;
            skipIfConvertError = property.GetCustomAttribute(typeof(SkipMappingIfConvertionErrorAttribute), false) != null;
        }
    }
}
