using AnotherECS.Generator;

namespace AnotherECS.Unity.Editor.Generator
{
    public class UnityGeneratorContext : GeneratorContext
    {
        public UnityGeneratorContext()
            : base(new UnityEnvironmentProvider())
        {
        }
    }
}
