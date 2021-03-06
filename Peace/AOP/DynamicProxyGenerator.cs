﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Peace.AOP
{
    public class DynamicProxyGenerator
    {

        #region Const

        private const string AssemblyName = "Peace.AOP.DynamicAssembly";
        private const string ModuleName = "Peace.AOP.DynamicModule";
        private const string DllFileName = "Peace.AOP.DynamicAssembly.dll";

        #endregion

        private readonly Type _realType;

        public DynamicProxyGenerator(Type realType)
        {
            _realType = realType;
        }

        public Type CreateProxy(params Type[] parentParamterType)
        {
            Type interceptesType = typeof(IInterceptor[]);
            Type contextType = typeof(InvokeContext);

            //构造程序集
            AssemblyName assemblyName = new AssemblyName(AssemblyName);//程序集名称
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave, AppDomain.CurrentDomain.BaseDirectory);
            //模块
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(ModuleName, DllFileName);
            string className = string.Format("{0}.{1}Proxy", "Peace.DynamicProxy", _realType.Name);
            //类型
            TypeBuilder typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed, _realType);

            //if (_parenType.IsInterface)
            //{
            //    typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed);
            //    typeBuilder.AddInterfaceImplementation(_parenType);
            //}
            //else
            //{
            //    typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed, _parenType);
            //}

            #region Field

            FieldBuilder field_interceptors = typeBuilder.DefineField("_interceptors", interceptesType,
                FieldAttributes.Private);
            field_interceptors.SetConstant(null);

            FieldBuilder field_realObj = typeBuilder.DefineField("_realObj", _realType,
     FieldAttributes.Private);
            field_realObj.SetConstant(null);

            #endregion

            #region Constructor

            ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public,
                CallingConventions.HasThis, null);
            //
            ILGenerator constructorIl = constructorBuilder.GetILGenerator();

            ConstructorInfo realConstructorInfo = _realType.GetConstructor(parentParamterType);
            LocalBuilder loc_interceptors = constructorIl.DeclareLocal(typeof(object[]));
            constructorIl.Emit(OpCodes.Ldarg_0);
            constructorIl.Emit(OpCodes.Newobj, realConstructorInfo);
            constructorIl.Emit(OpCodes.Stfld, field_realObj);
            constructorIl.Emit(OpCodes.Ldarg_0);//this
            constructorIl.Emit(OpCodes.Call, realConstructorInfo);

            constructorIl.Emit(OpCodes.Ldtoken, _realType);
            constructorIl.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) }));
            constructorIl.Emit(OpCodes.Ldtoken, typeof(IInterceptor));
            constructorIl.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) }));

            constructorIl.Emit(OpCodes.Ldc_I4_0);
            constructorIl.Emit(OpCodes.Callvirt, typeof(MemberInfo).GetMethod("GetCustomAttributes",
                new Type[] { typeof(Type), typeof(bool) }));
            constructorIl.Emit(OpCodes.Stloc, loc_interceptors);

            constructorIl.Emit(OpCodes.Ldarg_0);
            constructorIl.Emit(OpCodes.Ldloc, loc_interceptors);
            constructorIl.Emit(OpCodes.Call, typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(typeof(IInterceptor)));
            constructorIl.Emit(OpCodes.Call,
                typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(typeof(IInterceptor)));
            constructorIl.Emit(OpCodes.Stfld, field_interceptors);
            constructorIl.Emit(OpCodes.Ret);

            #endregion

            #region Method

            MethodInfo[] methodInfos = _realType.GetMethods(BindingFlags.Instance | BindingFlags.Public);

            foreach (var methodInfo in methodInfos)
            {
                if (methodInfo.Name == "ToString" || methodInfo.Name == "Equals" || methodInfo.Name == "GetHashCode" ||
                   methodInfo.Name == "GetType")
                    continue;
                var returnType = methodInfo.ReturnType;
                var parameterInfos = methodInfo.GetParameters();
                var parameterTypes = new Type[parameterInfos.Length];
                for (int i = 0; i < parameterInfos.Length; i++)
                {
                    parameterTypes[i] = parameterInfos[i].ParameterType;
                }
                var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name,
                    MethodAttributes.Public | MethodAttributes.Virtual, returnType, parameterTypes);
                var methodIl = methodBuilder.GetILGenerator();

                LocalBuilder loc_context = methodIl.DeclareLocal(contextType);
                LocalBuilder loc_receive_context = methodIl.DeclareLocal(contextType);
                LocalBuilder loc_objArry = methodIl.DeclareLocal(typeof(object[]));

                Label loc_leave = methodIl.DefineLabel();

                LocalBuilder loc_exception = methodIl.DeclareLocal(typeof(Exception));
                methodIl.Emit(OpCodes.Ldnull);
                methodIl.Emit(OpCodes.Stloc, loc_exception);

                #region Invoke

                LocalBuilder loc_result = null;
                if (returnType != typeof(void))
                {
                    loc_result = methodIl.DeclareLocal(returnType);

                    if (returnType.IsValueType)
                    {
                        methodIl.Emit(OpCodes.Ldloca_S, loc_result.LocalIndex);
                        methodIl.Emit(OpCodes.Initobj, returnType);
                        //methodIl.Emit(OpCodes.Ldloc, loc_return_value);
                        //methodIl.Emit(OpCodes.Box, returnType);
                    }
                    else
                    {
                        methodIl.Emit(OpCodes.Ldnull);
                        methodIl.Emit(OpCodes.Stloc, loc_result);
                    }
                }

                methodIl.Emit(OpCodes.Ldstr, methodInfo.Name);
                methodIl.Emit(OpCodes.Newobj, contextType.GetConstructor(new Type[] { typeof(String) }));
                methodIl.Emit(OpCodes.Stloc, loc_context);

                methodIl.Emit(OpCodes.Ldloc, loc_context);
                methodIl.Emit(OpCodes.Ldtoken, _realType);
                methodIl.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) }));
                methodIl.Emit(OpCodes.Callvirt, contextType.GetMethod("set_RealType", new Type[] { typeof(Type) }));

                if (parameterTypes.Length > 0)
                {
                    methodIl.Emit(OpCodes.Ldc_I4, parameterTypes.Length);
                    methodIl.Emit(OpCodes.Newarr, typeof(object));
                    methodIl.Emit(OpCodes.Stloc, loc_objArry);

                    for (int i = 1; i <= parameterTypes.Length; i++)
                    {
                        methodIl.Emit(OpCodes.Ldloc, loc_objArry);
                        var index = i - 1;
                        methodIl.Emit(OpCodes.Ldc_I4, index);
                        methodIl.Emit(OpCodes.Ldarg, i);
                        if (parameterTypes[index].IsValueType)
                        {
                            methodIl.Emit(OpCodes.Box, parameterTypes[index]);
                        }

                        methodIl.Emit(OpCodes.Stelem_Ref);
                    }

                    methodIl.Emit(OpCodes.Ldloc, loc_context);
                    methodIl.Emit(OpCodes.Ldloc, loc_objArry);
                    methodIl.Emit(OpCodes.Callvirt, contextType.GetMethod("set_Parameters", new Type[] { typeof(object[]) }));
                }

                methodIl.Emit(OpCodes.Ldloc, loc_context);
                methodIl.Emit(OpCodes.Stloc, loc_receive_context);


                #endregion


                #region Begin Exception

                methodIl.BeginExceptionBlock();

                #endregion

                #region BeginInvoke

                EmitForeach(methodIl, field_interceptors, loc_receive_context, "BeginInvoke");

                #endregion

                #region Invoke


                if (typeof(void) != returnType)
                {
                    methodIl.Emit(OpCodes.Ldloc, loc_receive_context);
                }

                methodIl.Emit(OpCodes.Ldarg_0);
                methodIl.Emit(OpCodes.Ldfld, field_realObj);
                for (int i = 1; i <= parameterTypes.Length; i++)
                {
                    methodIl.Emit(OpCodes.Ldarg, i);
                }
                methodIl.Emit(OpCodes.Call, _realType.GetMethod(methodInfo.Name, BindingFlags.Public | BindingFlags.Instance));

                if (typeof(void) != returnType)
                {
                    methodIl.Emit(OpCodes.Stloc, loc_result);
                    methodIl.Emit(OpCodes.Ldarg_0);
                    methodIl.Emit(OpCodes.Ldloc, loc_result);
                    if (returnType.IsValueType)
                    {
                        methodIl.Emit(OpCodes.Box, returnType);
                    }
                    methodIl.Emit(OpCodes.Call, contextType.GetMethod("set_ResultValue", new Type[] { typeof(object) }));
                }

                #endregion


                #region EndInvoke

                EmitForeach(methodIl, field_interceptors, loc_receive_context, "EndInvoke");

                #endregion

                #region Catch

                methodIl.BeginCatchBlock(typeof(Exception));

                methodIl.Emit(OpCodes.Stloc, loc_exception);
                methodIl.Emit(OpCodes.Ldloc, loc_receive_context);
                methodIl.Emit(OpCodes.Ldloc, loc_exception);
                methodIl.Emit(OpCodes.Call, contextType.GetMethod("set_Exception", new Type[] { typeof(Exception) }));

                EmitForeach(methodIl, field_interceptors, loc_receive_context, "OnException");

                #endregion

                #region End Exception

                methodIl.EndExceptionBlock();

                #endregion

                #region Return

                if (typeof(void) != returnType)
                {
                    methodIl.Emit(OpCodes.Ldloc, loc_result);
                }


                #endregion

                methodIl.Emit(OpCodes.Ret);
            }

            #endregion

            Type type = typeBuilder.CreateType();
            assemblyBuilder.Save(DllFileName);

            return type;
        }

        //todo: 检测dll是否存在
        public bool IsAssemblyVersionChange()
        {
            return true;
        }

        #region Private Method

        private static void EmitFor(ILGenerator methodIl, FieldBuilder interceptor, LocalBuilder context,
            string methodName)
        {
            LocalBuilder loc_curObj = methodIl.DeclareLocal(typeof(IInterceptor)); //Interceptor i
            LocalBuilder loc_index = methodIl.DeclareLocal(typeof(Int32)); //index

            Label lbl_compare = methodIl.DefineLabel(); //i<length
            Label lbl_enter = methodIl.DefineLabel(); //enter flag

            methodIl.Emit(OpCodes.Ldc_I4_0);
            methodIl.Emit(OpCodes.Stloc, loc_index);

            methodIl.Emit(OpCodes.Br, lbl_compare);

            methodIl.MarkLabel(lbl_enter); //进入循环

            methodIl.Emit(OpCodes.Ldarg_0);
            methodIl.Emit(OpCodes.Ldfld, interceptor);
            methodIl.Emit(OpCodes.Ldloc, loc_index);
            methodIl.Emit(OpCodes.Ldelem_Ref);
            methodIl.Emit(OpCodes.Stloc, loc_curObj);

            methodIl.Emit(OpCodes.Ldloc, loc_curObj);
            methodIl.Emit(OpCodes.Ldloc, context);
            methodIl.Emit(OpCodes.Call, typeof(IInterceptor).GetMethod(methodName, new Type[] { typeof(InvokeContext) }));

            methodIl.MarkLabel(lbl_compare);

            methodIl.Emit(OpCodes.Ldloc, loc_index);
            methodIl.Emit(OpCodes.Ldc_I4_1);
            methodIl.Emit(OpCodes.Add);
            methodIl.Emit(OpCodes.Stloc, loc_index);

            methodIl.Emit(OpCodes.Ldloc, loc_index);
            methodIl.Emit(OpCodes.Ldarg_0);
            methodIl.Emit(OpCodes.Ldfld, interceptor);
            methodIl.Emit(OpCodes.Ldlen);
            methodIl.Emit(OpCodes.Conv_I4);
            methodIl.Emit(OpCodes.Clt);
            methodIl.Emit(OpCodes.Brtrue_S, lbl_enter);
        }

        private static void EmitForeach(ILGenerator methodIl, FieldBuilder interceptors, LocalBuilder context,
        string methodName)
        {
            LocalBuilder loc_interceptorArray = methodIl.DeclareLocal(typeof(IInterceptor[]));//1
            LocalBuilder loc_curObj = methodIl.DeclareLocal(typeof(IInterceptor)); //0 Interceptor i
            LocalBuilder loc_index = methodIl.DeclareLocal(typeof(Int32));//2
            LocalBuilder loc_flag = methodIl.DeclareLocal(typeof(Boolean));//3

            Label lbl_compare = methodIl.DefineLabel(); //i<length 47
            Label lbl_enter = methodIl.DefineLabel(); //enter flag 20

            methodIl.Emit(OpCodes.Ldarg_0);
            methodIl.Emit(OpCodes.Ldfld, interceptors);
            methodIl.Emit(OpCodes.Stloc_S, loc_interceptorArray.LocalIndex);

            methodIl.Emit(OpCodes.Ldc_I4_0);
            methodIl.Emit(OpCodes.Stloc_S, loc_index.LocalIndex);

            methodIl.Emit(OpCodes.Br_S, lbl_compare);
            methodIl.MarkLabel(lbl_enter);

            methodIl.Emit(OpCodes.Ldloc_S, loc_interceptorArray.LocalIndex);
            methodIl.Emit(OpCodes.Ldloc_S, loc_index.LocalIndex);
            methodIl.Emit(OpCodes.Ldelem_Ref);
            methodIl.Emit(OpCodes.Stloc_S, loc_curObj.LocalIndex);

            methodIl.Emit(OpCodes.Ldloc_S, loc_curObj.LocalIndex);
            methodIl.Emit(OpCodes.Ldloc, context);
            methodIl.Emit(OpCodes.Call, typeof(IInterceptor).GetMethod(methodName, new Type[] { typeof(InvokeContext) }));

            methodIl.Emit(OpCodes.Ldloc_S, loc_index.LocalIndex);
            methodIl.Emit(OpCodes.Ldc_I4_1);
            methodIl.Emit(OpCodes.Add);
            methodIl.Emit(OpCodes.Stloc_S, loc_index.LocalIndex);

            methodIl.MarkLabel(lbl_compare);

            methodIl.Emit(OpCodes.Ldloc_S, loc_index.LocalIndex);
            methodIl.Emit(OpCodes.Ldloc_S, loc_interceptorArray.LocalIndex);
            methodIl.Emit(OpCodes.Ldlen);
            methodIl.Emit(OpCodes.Conv_I4);
            methodIl.Emit(OpCodes.Clt);
            methodIl.Emit(OpCodes.Stloc_S, loc_flag.LocalIndex);

            methodIl.Emit(OpCodes.Ldloc, loc_flag);
            methodIl.Emit(OpCodes.Brtrue_S, lbl_enter);

            methodIl.Emit(OpCodes.Nop);


        }

        #endregion


    }
}
