using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bolt5.CustomMappingObject.LinkedMapping
{
    public class LinkMap
    {
        public string ColumnName { get; set; }
        public string PropertyName { get; set; }

        public LinkMap(string columnName, string propertyName)
        {
            this.ColumnName = columnName;
            this.PropertyName = propertyName;
        }
    }
}
