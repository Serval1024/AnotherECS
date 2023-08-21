using AnotherECS.Collections;

namespace AnotherECS.Core
{
    public struct InjectContainer
    {
        public State State { get; private set; }
        public DArrayStorage DArrayStorage { get; private set; }

        public InjectContainer(State state, DArrayStorage dArrayStorage)
        {
            DArrayStorage = dArrayStorage;
            State = state;
        }
    }
}

