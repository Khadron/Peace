using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peace.AOP
{
    public class InvokeContext
    {
        private string _methodName;
        public InvokeContext(string methodName)
        {
            _methodName = methodName;
        }

        public Type RealType { get; set; }
        public string MethodName
        {
            get { return _methodName; }
            set { _methodName = value; }
        }
        public object[] Parameters { get; set; }

        public object ResultValue { get; set; }

        public Exception Exception { get; set; }

        public object Tag { get; set; }
    }
}
