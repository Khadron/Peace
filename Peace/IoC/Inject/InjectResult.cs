using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peace.IoC.Inject
{
    internal class InjectResult
    {
        public IList<InjectItem> InjectItms { get; set; }
        public object Instance { get; set; }
    }

    internal class InjectItem
    {
        public Type InjectItmType { get; set; }
        public Func<object> InjectInstance { get; set; }

    }
}
