using System;
using System.Collections.Generic;
using Peace.IoC.Inject;
using Peace.IoC.Syntax;

namespace Peace.IoC.Kernel
{
    public abstract class KernelBase : IKernel, IBindingSyntax
    {
        private Type _curKey;
        private readonly KernelContext _context;
        private readonly Injecter _injecter;

        protected KernelBase()
        {
            _context = KernelContext.Context;
            _injecter = new Injecter(_context);
        }

        public IBindingSyntax Bind<T>()
        {
            Bind(typeof(T));
            return this;
        }

        public IBindingSyntax Bind(Type type)
        {
            _curKey = type;
            _context.Map(_curKey);
            return this;
        }

        public IBindingSyntax To<TU>()
        {
            To(typeof(TU));
            return this;
        }

        public IBindingSyntax To(Type type)
        {
            if (_curKey.IsAssignableFrom(type))
            {
                _context.Map1(_curKey, type);
            }
            return this;
        }

        public void InSingletonScope()
        {
            _context.Map2(_curKey, Lifetime.Singleton);
        }

        public void InTransientScope()
        {
            _context.Map2(_curKey, Lifetime.Transient);
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type type)
        {

            return _injecter.Resolve(type);
        }

        public T Get<T>()
        {
            return (T)Get(typeof(T));
        }

        public object Get(Type type)
        {
            return _injecter.Inject(type);

        }

        //private T GetObjectWithLifetime<T>(Func<Type, T> method)
        //{
        //    var curType = typeof(T);
        //    Lifetime lifetime = Lifetime.Singleton;

        //    var setting = _context.GetValue(curType);
        //    if (setting != null)
        //    {
        //        lifetime = setting.Scope;
        //    }

        //    switch (lifetime)
        //    {
        //        case Lifetime.Singleton:
        //            if (_context.ContainsKey(curType))
        //            {
        //                return (T)ObjCache[curType];
        //            }
        //            return method(curType);
        //        case Lifetime.Transient:
        //            return method(curType);
        //        default:
        //            throw new NotSupportedException("传入的lifetime不受支持：" + lifetime);
        //    }

        //}

        //private object GetObjectWithLifetime(Type type, Func<Type, InjectResult> method)
        //{
        //    if (ObjCache.ContainsKey(type))
        //    {
        //        return ObjCache[type];
        //    }


        //    //委托
        //    //得放到inject里
        //    var methodResult = method(type);



        //}
    }
}
