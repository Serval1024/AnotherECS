﻿using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(uint id)
            : base($"{DebugConst.TAG}Entity with id not found: '{id}'.")
        { }
    }
}
