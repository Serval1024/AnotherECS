using System;
using System.Collections.Generic;

namespace AnotherECS.Core
{
    internal interface IGroupSystemInternal : IDisposable
    {
        void Prepend(ISystem system);
        void PrepareInternal();
        void ConstructInternal(State state);
        void TickStartedInternal(State state);
        void TickFinishiedInternal(State state);

        void InitInternal(State state);
        void TickInternal(State state);
        void DestroyInternal(State state);

        void ReceiveInternal(State state, List<ITickEvent> events);
    }
}