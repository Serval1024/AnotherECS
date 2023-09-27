namespace AnotherECS.Core
{
    public struct InjectContainer
    {
        public DArrayCaller DArrayCaller { get; private set; }

        public InjectContainer(DArrayCaller dArrayCaller)
        {
            DArrayCaller = dArrayCaller;
        }
    }
}

