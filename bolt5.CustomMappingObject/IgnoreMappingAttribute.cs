using System;

namespace bolt5.CustomMappingObject
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class IgnoreMappingAttribute : Attribute
    {
    }
}
