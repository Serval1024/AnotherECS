using AnotherECS.Debug;
using System;

namespace AnotherECS.Collections.Exceptions
{
    public class MissInjectException : Exception
    {
        public MissInjectException(Type type)
            : base($"{DebugConst.TAG}Field '{type.Name}' not injected yet. Perhaps you are trying to create a structure with 'new'. Use state.Add<Component>(id) or state.Create<T>() or entity.Add<T>() instead.")
        { }
    }
}
