using System.Collections.Generic;

namespace AnotherECS.Core
{
    public interface IInclude { }
    public interface IExclude { }

    public interface ICompileFilter
    { }

    public interface ICompileFilter<EState> : ICompileFilter
        where EState : IState
    { }

    public interface IFilter : IEnumerable<int>
    { }

    public interface IInclude<T0> : IFilter, IInclude
        where T0 : IComponent
    { }

    public interface IInclude<T0, T1> : IFilter
        where T0 : IComponent
        where T1 : IComponent
    { }

    public interface IInclude<T0, T1, T2> : IFilter
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
    { }

    public interface IInclude<T0, T1, T2, T3> : IFilter
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
    { }

    public interface IInclude<T0, T1, T2, T3, T4> : IFilter
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
    { }

    public interface IInclude<T0, T1, T2, T3, T4, T5> : IFilter
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
        where T5 : IComponent
    { }

    public interface IExclude<T0> : IFilter, IExclude
        where T0 : IComponent
    { }

    public interface IExclude<T0, T1> : IFilter, IExclude
        where T0 : IComponent
        where T1 : IComponent
    { }
    public interface IExclude<T0, T1, T2> : IFilter
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
    { }

    public interface IExclude<T0, T1, T2, T3> : IFilter
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
    { }

    public interface IExclude<T0, T1, T2, T3, T4> : IFilter
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
    { }

    public interface IExclude<T0, T1, T2, T3, T4, T5> : IFilter
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
        where T5 : IComponent
    { }
}