using System;
using System.Collections.Generic;

namespace AnotherECS.Core.Collection
{
    public interface INDictionary<TValue> : INative
    {
        IEnumerable<TValue> Values { get; }
    }

    public interface INDictionary<TKey, TValue> : INDictionary<TValue>, INative, IEnumerable<Pair<TKey, TValue>>
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        bool ContainsKey(TKey key);
        bool TryGetValue(TKey key, out TValue value);
        TValue this[TKey key] { get; set; }
        void Add(TKey key, TValue value);
        bool Remove(TKey key);
    }
}