﻿using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class ComponentExistsException : Exception
    {
        public ComponentExistsException(Type type)
            : base($"{DebugConst.TAG}Component already added to entity: {type.Name}.")
        { }
    }
}