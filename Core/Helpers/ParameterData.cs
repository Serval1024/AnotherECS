using System;

namespace AnotherECS.Core
{
    internal struct ParameterData
    {
        private const char _RULE_SEPARATOR = '=';

        public Type type;
        public InjectMapAttribute[] maps;

        internal string Map(ref InjectContext context)
        {
            for(int i = 0; i < maps.Length; ++i)
            {
                if (IsMap(ref context, maps[i]))
                {
                    return maps[i].Name;
                }
            }
            return null;
        }

        private bool IsMap(ref InjectContext context, InjectMapAttribute map)
        {
            var split = map.Rule.Split(_RULE_SEPARATOR);
            if (context.variables.TryGetValue(split[0].Trim(), out object value))
            {
                return split[1].Trim() == value.ToString();
            }
            return false;
        }
    }
}