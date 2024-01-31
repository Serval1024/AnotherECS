using AnotherECS.Core;

namespace AnotherECS.Debug.Diagnostic
{
    public interface IDiagnostic
    {
        public void Attach(World world);
        public void Update(World world);
        public void Detach(World world);
    }
}
