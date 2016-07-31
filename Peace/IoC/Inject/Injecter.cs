using System;
using System.Collections.Generic;
using System.Reflection;
using Peace.IoC.Attribute;
using Peace.IoC.Kernel;

namespace Peace.IoC.Inject
{
    internal class Injecter
    {
        protected Cache<Type, object> ObjCache;
        private readonly KernelContext _context;
        public Injecter(KernelContext context)
        {
            _context = context;
            ObjCache = new Cache<Type, object>();

        }

        public object Inject<T>()
        {
            return Inject(typeof(T));
        }

        public object Inject(Type type)
        {

            #region Constructor Inject

            var instance = CreateObject(type);

            #endregion


            #region Property Inject

            PropertyInfo[] propertyInfos = type.GetProperties();
            foreach (var propertyInfo in propertyInfos)
            {
                if (propertyInfo.GetCustomAttributes(typeof(InjectAttribute), false).Length > 0)
                {
                    var it = _context.GetInstanceType(propertyInfo.PropertyType);
                    if (it == null)
                    {
                        throw new Exception("没有找到对应的类型");
                    }

                    object value = CreateObject(it);
                    propertyInfo.SetValue(instance, value, null);
                }
            }

            #endregion


            return instance;
        }


        public object Resolve(Type type)
        {
            if (ObjCache.ContainsKey(type))
            {
                return ObjCache[type];
            }
            if (_context.ContainsKey(type))
            {
                Type instanceType = _context.GetInstanceType(type);
                return CreateObject(instanceType);
            }
            return null;
        }


        public object CreateObject(Type type)
        {
            object instance = null;
            ConstructorInfo[] contructorInfos = type.GetConstructors();
            foreach (ConstructorInfo constructorInfo in contructorInfos)
            {
                if (constructorInfo.GetParameters().Length > 0)
                {
                    ParameterInfo[] paras = constructorInfo.GetParameters();
                    var paraInstances = new List<object>();
                    foreach (var para in paras)
                    {
                        var key = para.ParameterType;
                        if (_context.ContainsKey(key))
                        {
                            var instanceType = _context.GetInstanceType(key);
                            if (instanceType == null)
                            {
                                throw new Exception("没有找到对应的类型");
                            }
                            object paraInstance = CreateObject(instanceType);
                            paraInstances.Add(paraInstance);
                        }
                    }
                    instance = GetInjectItem(type, paraInstances.ToArray());

                }
                else
                {
                    instance = GetInjectItem(type);
                }

                break;
            }

            return instance;
        }

        private Object GetInjectItem(Type type, params object[] args)
        {

            var setting = _context.GetValue(type);
            if (setting != null)
            {
                Lifetime lifetime = setting.Scope;

                switch (lifetime)
                {
                    case Lifetime.Singleton:
                        if (ObjCache.ContainsKey(setting.Bind))
                            return ObjCache[setting.Bind];

                        var instance = FastObjectCreater.CreateInstance(type, args);
                        ObjCache.Add(setting.Bind, instance);
                        return instance;
                    case Lifetime.Transient:
                        return FastObjectCreater.CreateInstance(type, args);
                    default:
                        throw new NotSupportedException("传入的lifetime不受支持：" + lifetime);
                }
            }

            return FastObjectCreater.CreateInstance(type, args);
        }

    }
}
