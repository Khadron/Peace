using System;

namespace Peace.IoC.Syntax
{
    public interface IBindingSyntax : IFluentSyntax
    {
        IBindingSyntax Bind(Type type);
        IBindingSyntax Bind<T>();
        IBindingSyntax To(Type type);
        IBindingSyntax To<TU>();

        //IScopeSyntax InSingletonScope()
        void InSingletonScope();
        //IScopeSyntax InTransientScope()
        void InTransientScope();

    }
}
