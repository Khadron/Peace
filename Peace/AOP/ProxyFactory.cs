using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peace.AOP
{
    public class ProxyFactory
    {
        public static T CreateProxy<T>() where T : class
        {
            return CreateProxy<T>(typeof(T));
        }

        public static T CreateProxy<T>(Type realType, params object[] paramters)
        {
            var proxyGenerator = new DynamicProxyGenerator(realType);
            Type type = proxyGenerator.CreateProxy();
            return (T)FastObjectCreater.CreateInstance(type, paramters);

        }
    }
}
