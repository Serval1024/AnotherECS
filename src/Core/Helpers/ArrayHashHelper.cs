namespace AnotherECS.Core
{
    internal static class ArrayHashHelper
    {
        public static int GetHashCode(byte[] bytes)
        {
            unchecked
            {
                const int p = 16777619;
                int hash = (int)2166136261;

                for (int i = 0; i < bytes.Length; i++)
                    hash = (hash ^ bytes[i]) * p;

                hash += hash << 13;
                hash ^= hash >> 7;
                hash += hash << 3;
                hash ^= hash >> 17;
                hash += hash << 5;
                return hash;
            }
        }
    }
}
