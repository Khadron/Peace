using System;

namespace Peace.IoC.Syntax
{
    public interface IFluentSyntax
    {
        Type GetType();

        int GetHashCode();

        string ToString();

        bool Equals(object other);
    }
}
