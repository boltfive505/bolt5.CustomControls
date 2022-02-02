using System;

namespace bolt5.CustomMappingObject
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple =true, Inherited = false)]
    public class ColumnMappingAttribute : Attribute
    {
        public string ColumnName { get; private set; }
        public ColumnMappingComparison Comparison { get; private set; }

        public ColumnMappingAttribute(string columnName, ColumnMappingComparison comparison = ColumnMappingComparison.Exact)
        {
            this.ColumnName = columnName;
            this.Comparison = comparison;
        }
    }
}
