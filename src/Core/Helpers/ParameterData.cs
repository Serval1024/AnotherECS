﻿using System;

namespace AnotherECS.Core
{
    internal struct ParameterData
    {
        private const char RULE_SEPARATOR = '=';

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
            if (string.IsNullOrEmpty(map.Rule))
            {
                return true;
            }

            var split = map.Rule.Split(RULE_SEPARATOR);
            if (context.variables.TryGetValue(split[0].Trim(), out object value))
            {
                return split[1].Trim() == value.ToString();
            }
            return false;
        }
    }
}