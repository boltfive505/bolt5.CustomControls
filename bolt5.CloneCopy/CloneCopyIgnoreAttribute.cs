using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bolt5.CloneCopy
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class CloneCopyIgnoreAttribute : Attribute
    {
    }
}
