﻿using System;

namespace AnotherECS.Core.Collection
{
    internal static class HashHelpers
    {
        public const int MaxPrimeArrayLength = 0x7FEFFFFD;

        public static readonly uint[] _primes =
        {
            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
            1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
            17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
            187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
            1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369, 8639249, 10367101,
            12440537, 14928671, 17914409, 21497293, 25796759, 30956117, 37147349, 44576837, 53492207, 64190669,
            77028803, 92434613, 110921543, 133105859, 159727031, 191672443, 230006941, 276008387, 331210079,
            397452101, 476942527, 572331049, 686797261, 824156741, 988988137, 1186785773, 1424142949, 1708971541,
            2050765853, MaxPrimeArrayLength
        };

        public static bool IsPrime(uint candidate)
        {
            if ((candidate & 1) != 0)
            {
                uint limit = (uint)Math.Sqrt(candidate);
                for (uint divisor = 3; divisor <= limit; divisor += 2)
                {
                    if ((candidate % divisor) == 0)
                    {
                        return false;
                    }
                }
                return true;
            }
            return candidate == 2;
        }

        public static uint GetPrime(uint min)
        {
            for (uint i = 0; i < _primes.Length; i++)
            {
                uint prime = _primes[i];
                if (prime >= min)
                {
                    return prime;
                }
            }

            for (uint i = (min | 1); i < uint.MaxValue; i += 2)
            {
                if (IsPrime(i))
                {
                    return i;
                }
            }
            return min;
        }

        public static uint ExpandPrime(uint oldSize)
        {
            uint newSize = 2 * oldSize;

            if (newSize > MaxPrimeArrayLength && MaxPrimeArrayLength > oldSize)
            {
                return MaxPrimeArrayLength;
            }

            return GetPrime(newSize);
        }
    }
}