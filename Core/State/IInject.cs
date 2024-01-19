using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AnotherECS.Collections")]
namespace AnotherECS.Core
{
    public interface IInject
    {
        void Deconstruct();
    }

    public interface IInject<T0> : IInject
    {
        void Construct(T0 t0);
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    internal class InjectMapAttribute : Attribute
    {
        public string Name { get; private set; }
        public string Rule { get; private set; }

        public InjectMapAttribute(string name, string rule)
        {
            Name = name;
            Rule = rule;
        }
    }
}
