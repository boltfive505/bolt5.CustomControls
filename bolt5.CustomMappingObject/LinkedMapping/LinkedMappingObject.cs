using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bolt5.CustomMappingObject.Core;

namespace bolt5.CustomMappingObject.LinkedMapping
{
    public sealed class LinkedMappingObject<T> : _BaseMappingObject<T>
    {
        public LinkedMappingObject(string[] columns, LinkMap[] links)
        {
            var properties = _BaseMappingObject<T>.GetProerties();
            PropertyMaps = new List<MapItem>();
            for (int i = 0; i < columns.Length; i++)
            {
                string columnName = columns[i].Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ").Trim();
                //find link from column
                LinkMap link = links.FirstOrDefault(l => l.ColumnName == columnName);
                if (link == null) continue;
                //find property from link
                var prop = properties.FirstOrDefault(p => p.Name == link.PropertyName);
                if (prop == null) continue;
                MapItem map = new MapItem(prop) { columnIndex = i };
                PropertyMaps.Add(map);
            }
        }
    }
}
