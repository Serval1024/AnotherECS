using AnotherECS.Core;
using System.Runtime.CompilerServices;

public static class StateExtensions
{
    public static Entity NewEntity(this State state)
        => EntityExtensions.ToEntity(state, state.New());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IdFilter<T0> Filter<T0>(this State state)
        where T0 : IComponent
        => state.CreateFilter().With<T0>().BuildAsId();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IdFilter<T0, T1> Filter<T0, T1>(this State state)
        where T0 : IComponent
        where T1 : IComponent
        => state.CreateFilter().With<T0>().With<T1>().BuildAsId();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IdFilter<T0, T1, T2> Filter<T0, T1, T2>(this State state)
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        => state.CreateFilter().With<T0>().With<T1>().With<T2>().BuildAsId();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IdFilter<T0, T1, T2, T3> Filter<T0, T1, T2, T3>(this State state)
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        => state.CreateFilter().With<T0>().With<T1>().With<T2>().With<T3>().BuildAsId();


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EntityFilter<T0> EntityFilter<T0>(this State state)
       where T0 : IComponent
       => state.CreateFilter().With<T0>().BuildAsEntity();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EntityFilter<T0, T1> EntityFilter<T0, T1>(this State state)
        where T0 : IComponent
        where T1 : IComponent
        => state.CreateFilter().With<T0>().With<T1>().BuildAsEntity();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EntityFilter<T0, T1, T2> EntityFilter<T0, T1, T2>(this State state)
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        => state.CreateFilter().With<T0>().With<T1>().With<T2>().BuildAsEntity();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EntityFilter<T0, T1, T2, T3> EntityFilter<T0, T1, T2, T3>(this State state)
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        => state.CreateFilter().With<T0>().With<T1>().With<T2>().With<T3>().BuildAsEntity();
}
