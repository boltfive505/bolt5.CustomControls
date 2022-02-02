using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using bolt5.CustomMappingObject.Core;

namespace bolt5.CustomMappingObject
{
    public sealed class MappingObject<T> : _BaseMappingObject<T>
    {
        public MappingObject(string[] columns)
        {
            var properties = _BaseMappingObject<T>.GetProerties();
            PropertyMaps = properties.Where(p => p.GetCustomAttributes(typeof(ColumnMappingAttribute), false).Length > 0)
                                    .Select(p => new MapItem(p))
                                    .ToList();
            for (int i = 0; i < columns.Length; i++)
            {
                string columnName = columns[i].Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ").Trim();
                MapItem map = PropertyMaps.FirstOrDefault(m => m.columnMappings.Any(colMap => DoMatchColumnName(colMap, columnName)));
                if (map == null) continue;
                map.columnIndex = i;
            }
        }
    }
}
