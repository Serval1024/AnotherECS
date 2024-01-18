using System.Collections.Generic;

namespace AnotherECS.Core
{

    internal struct InjectContext
    {
        public Dictionary<string, object> variables;

        public static InjectContext Create()
            => new()
            {
                variables = new Dictionary<string, object>(),
            };
    }
}