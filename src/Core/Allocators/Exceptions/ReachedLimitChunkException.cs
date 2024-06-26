﻿using AnotherECS.Debug;
using System;

namespace AnotherECS.Core.Allocators.Exceptions
{
    public class ReachedLimitChunkException : Exception
    {
        public ReachedLimitChunkException(uint limit)
            : base($"{DebugConst.TAG}The limit of the maximum chunks, limit: '{limit}'.")
        { }
    }
}
