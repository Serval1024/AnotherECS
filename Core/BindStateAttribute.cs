using System;

namespace AnotherECS.Core
{
    [AttributeUsage(AttributeTargets.Struct)]
    public class BindStateAttribute : Attribute
    {
        public Type State { get; private set; }
        public int Capacity { get; private set; }

        public BindStateAttribute(Type state)
        {
            if (!typeof(IState).IsAssignableFrom(state))
            {
                throw new ArgumentException($"Argument '{nameof(state)}' must be a '{nameof(IState)}' type.");
            }
            State = state;
        }
    }
}
