using System;
using System.Linq;
using Peace.IoC.Setting;

namespace Peace.IoC.Kernel
{
    internal class KernelContext
    {
        private static readonly object Lock = new object();
        private static KernelContext _context;
        private readonly Cache<Type, InjectSetting> _typeCache;

        private KernelContext()
        {
            _typeCache = new Cache<Type, InjectSetting>();
        }

        public static KernelContext Context
        {
            get
            {
                if (_context == null)
                {
                    lock (Lock)
                    {
                        if (_context == null)
                        {
                            var context = new KernelContext();
                            _context = context;
                        }
                    }
                }
                return _context;
            }
        }

        public void Map(Type bind)
        {
            if (!_typeCache.ContainsKey(bind))
            {
                _typeCache.Add(bind, new InjectSetting { Bind = bind, Scope = Lifetime.Singleton });
            }
        }

        public void Map1(Type bind, Type to)
        {
            if (_typeCache.ContainsKey(bind))
            {
                var val = _typeCache[bind];
                if (val == null)
                {
                    val = new InjectSetting { Bind = bind, Scope = Lifetime.Singleton };
                }

                val.To = to;
            }

        }

        public void Map2(Type bind, Lifetime scope)
        {
            if (!_typeCache.ContainsKey(bind))
            {
                var val = _typeCache[bind];
                if (val == null)
                {
                    val = new InjectSetting { Bind = bind };
                }

                val.Scope = scope;
            }

        }

        public void MapInfo(Type type, InjectSetting setting)
        {
            if (!_typeCache.ContainsKey(type))
            {
                _typeCache.Add(type, setting);
            }
        }

        public bool ContainsKey(Type type)
        {
            return _typeCache.ContainsKey(type);
        }

        public InjectSetting GetValue(Type value)
        {
            return _typeCache.Values.FirstOrDefault(r => r.To == value);
        }

        public Type GetInstanceType(Type key)
        {
            var setting = _typeCache[key];
            return setting == null ? null : setting.To;
        }

    }
}
