using System;
using System.Reflection.Emit;

namespace Peace.IoC
{
    internal class ObjectCreater
    {
        public static Func<object> EmitCreateInstance(Type type)
        {
            DynamicMethod dm = new DynamicMethod(string.Empty, typeof(object), Type.EmptyTypes);
            var gen = dm.GetILGenerator();
            if (type.IsValueType)
            {
                gen.DeclareLocal(type);
                gen.Emit(OpCodes.Ldloca_S, 0);
                gen.Emit(OpCodes.Initobj, type);
                gen.Emit(OpCodes.Ldloc_0);
                gen.Emit(OpCodes.Box, type);
            }
            else
            {
                gen.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes));
            }
            gen.Emit(OpCodes.Ret);
            return (Func<object>)dm.CreateDelegate(typeof(Func<object>));
        }


        public static object CreateInstance(Type type, params object[] args)
        {
            return Activator.CreateInstance(type, args);
        }

    }
}
