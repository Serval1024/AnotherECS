using System;
using System.Linq;

namespace AnotherECS.Core.Exceptions
{
    internal class FeatureRequestConfigException : Exception
    {
        public FeatureRequestConfigException(Type[] types)
            : base($"Feature does not find request configurations: '{types.Select(p => p.Name).Aggregate((s, p) => s + ", " + p)}'.")
        {
        }
    }
}
