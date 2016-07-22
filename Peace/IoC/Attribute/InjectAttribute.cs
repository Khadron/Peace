using System;

namespace Peace.IoC.Attribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class InjectAttribute : System.Attribute
    {
        public InjectAttribute()
        {
        }
    }
}
