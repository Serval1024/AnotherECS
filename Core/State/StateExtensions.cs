using AnotherECS.Core;
using System.Runtime.CompilerServices;

public static class StateExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Filter<T0> Filter<T0>(this State state)
        where T0 : IComponent
        => state.CreateFilterBuilder().With<T0>().Build();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Filter<T0, T1> Filter<T0, T1>(this State state)
        where T0 : IComponent
        where T1 : IComponent
        => state.CreateFilterBuilder().With<T0>().With<T1>().Build();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Filter<T0, T1, T2> Filter<T0, T1, T2>(this State state)
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        => state.CreateFilterBuilder().With<T0>().With<T1>().With<T2>().Build();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Filter<T0, T1, T2, T3> Filter<T0, T1, T2, T3>(this State state)
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        => state.CreateFilterBuilder().With<T0>().With<T1>().With<T2>().With<T3>().Build();
}
