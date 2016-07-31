using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peace.AOP
{
    public interface IInterceptor
    {
        void BeginInvoke(InvokeContext context);
        void EndInvoke(InvokeContext context);
        void OnException(InvokeContext context);
    }
}
