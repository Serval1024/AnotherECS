﻿using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class OptionsConflictException : Exception
    {
        public OptionsConflictException(Type type, string message)
            : base($"{DebugConst.TAG}The following options cannot be on the same component: {message}. Component name: {type.Name}.")
        { }
    }
}