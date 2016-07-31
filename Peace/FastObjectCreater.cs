using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Peace
{
    public class FastObjectCreater
    {
        private delegate object CreateOjectHandler(object[] parameters);
        private static readonly Hashtable creatorCache = Hashtable.Synchronized(new Hashtable());

        public static T CreateInstance<T>(params object[] parameters) where T : class, new()
        {
            Type type = typeof(T);

            return (T)CreateInstance(type, parameters.ToArray());
        }


        public static object CreateInstance(Type type, params object[] parameters)
        {
            int token = type.MetadataToken;
            Type[] parameterTypes = GetParameterTypes(ref token, parameters);

            CreateOjectHandler ctor = creatorCache[token] as CreateOjectHandler;
            if (ctor == null)
            {
                lock (creatorCache.SyncRoot)
                {
                    ctor = CreateHandler(type, parameterTypes);
                    creatorCache.Add(token, ctor);
                }
            }
            return ctor.Invoke(parameters);
        }

        private static CreateOjectHandler CreateHandler(Type type, Type[] paramsTypes)
        {

            ConstructorInfo constructor = type.GetConstructor(paramsTypes);
            DynamicMethod method = new DynamicMethod("DynamicCreateOject", typeof(object),
               new Type[] { typeof(object[]) }, constructor.DeclaringType.Module);
            //DynamicMethod method = new DynamicMethod("DynamicCreateOject", typeof(object),
            //   new Type[] { typeof(object[]) }, typeof(FastObjectCreater).Module);

            ILGenerator il = method.GetILGenerator();

            for (int i = 0; i < paramsTypes.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldelem_Ref);
                if (paramsTypes[i].IsValueType)
                {
                    il.Emit(OpCodes.Unbox_Any, paramsTypes[i]);
                }
                else
                {
                    il.Emit(OpCodes.Castclass, paramsTypes[i]);
                }
            }
            il.Emit(OpCodes.Newobj, constructor);
            il.Emit(OpCodes.Ret);

            return (CreateOjectHandler)method.CreateDelegate(typeof(CreateOjectHandler));
        }

        private static Type[] GetParameterTypes(ref int token, params object[] parameters)
        {
            if (parameters == null) return new Type[0];
            Type[] values = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                values[i] = parameters[i].GetType();
                token = token * 13 + values[i].MetadataToken;
            }
            return values;
        }


    }
}
