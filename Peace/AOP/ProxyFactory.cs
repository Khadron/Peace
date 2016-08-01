using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peace.AOP
{
    public class ProxyFactory
    {
        public static T CreateProxy<T>(params object[] paramters) where T : class
        {
            return (T)CreateProxy(typeof(T), paramters);
        }

        public static object CreateProxy(Type realType, params object[] paramters)
        {
            var proxyGenerator = new DynamicProxyGenerator(realType);
            Type type = proxyGenerator.CreateProxy(paramters.Select(r => r.GetType()).ToArray());
            return FastObjectCreater.CreateInstance(type, paramters);

        }
    }
}
