using System;

namespace bolt5.CustomMappingObject
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class MappingParseMethodAttribute : Attribute
    {
        public string MethodName { get; private set; }

        public MappingParseMethodAttribute(string methodName)
        {
            this.MethodName = methodName;
        }
    }
}
