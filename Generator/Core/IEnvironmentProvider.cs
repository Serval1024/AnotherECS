using System;

namespace AnotherECS.Generator
{
    public interface IEnvironmentProvider
    {
        string GetFilePathToType(Type type);
        string GetTemplate(string fileName);
        string FindRootGenDirectory();
        string FindRootGenCommonDirectory();
    }
}
