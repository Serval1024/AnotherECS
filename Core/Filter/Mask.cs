using System;

namespace AnotherECS.Core
{
    public unsafe struct Mask
    {
        public delegate*<State, int, bool> selector;
        public uint id;
        public long hash;
        public bool isAutoClear;
        public ushort[] includes;
        public ushort[] excludes;
        
        public bool IsValide
            => id != 0;

        public Mask(delegate*<State, int, bool> selector, uint id, bool isAutoClear, ushort[] includes, ushort[] excludes)
            : this()
        {
            Array.Sort(includes, 0, includes.Length);
            Array.Sort(excludes, 0, excludes.Length);

            this.id = id;
            this.selector = selector;
            this.isAutoClear = isAutoClear;
            this.includes = includes;
            this.excludes = excludes;
            hash = FilterUtils.GetHash(includes, excludes);
        }

        public Mask(uint id, bool isAutoClear, ushort[] includes, ushort[] excludes)
            : this(null, id, isAutoClear, includes, excludes)
        {
        }
    }
}
