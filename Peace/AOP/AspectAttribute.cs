using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peace.AOP
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class AspectAttribute : Attribute
    {
        protected AspectAttribute()
        {
        }

        public string[] Ignores { get; set; }

    }
}
