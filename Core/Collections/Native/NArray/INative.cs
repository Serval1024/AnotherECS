using System;

namespace AnotherECS.Core.Collection
{
    public unsafe interface INative : IDisposable
    {
        bool IsValide { get; }
    }
}
