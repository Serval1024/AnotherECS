using System;

namespace AnotherECS.Generator
{
    public interface IEnvironmentProvider
    {
        string GetFilePathByStateName(string stateName);
        string GetTemplate(string fileName);
        string FindRootGenDirectory();
        string FindRootGenCommonDirectory();
    }
}
