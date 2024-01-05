using System;

namespace AnotherECS.Core.Collection
{
    public unsafe interface INative : IDisposable
    {
        bool IsValid { get; }
    }
}
