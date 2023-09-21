using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AnotherECS.Collections")]
namespace AnotherECS.Core
{
    internal interface IInject
    {
        void Deconstruct();
    }

    internal interface IInject<T0> : IInject
    {
        void Construct(T0 t0);
    }
}
