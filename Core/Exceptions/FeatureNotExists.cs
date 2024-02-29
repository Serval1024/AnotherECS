using System;

namespace AnotherECS.Core.Exceptions
{
    public class FeatureNotExists : Exception
    {
        public FeatureNotExists(string name)
            : base($"Feature: '{name}' is not attached to the world.")
        {
        }
    }
}