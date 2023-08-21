﻿using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class ReadAccessException : Exception
    {
        public ReadAccessException(string componentName)
            : base($"{DebugConst.TAG}The component '{componentName}' obtained by the 'Read()' method has been changed. For component changes, use 'Get()'.")
        { }
    }
}
