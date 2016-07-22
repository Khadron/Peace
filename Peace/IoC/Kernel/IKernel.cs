using System;
using Peace.IoC.Syntax;

namespace Peace.IoC.Kernel
{
    public interface IKernel : IBindingSyntax
    {
        T Get<T>();
        object Get(Type type);

        T Resolve<T>();
        object Resolve(Type type);

    }
}
