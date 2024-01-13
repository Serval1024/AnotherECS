using System;
using System.Linq;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core.Threading
{
    public unsafe struct ThreadRestrictionsBuilder : IDisposable
    {
        private readonly State _state;
        private NList<BAllocator, ushort> _components;

        internal ThreadRestrictionsBuilder(State state)
        {
            _state = state;
            _components = new NList<BAllocator, ushort>(&state.GetDependencies()->bAllocator, 16);
        }

        public void Dispose()
        {
            _components.Dispose();
        }

        public ThreadRestrictionsBuilder Use<T>()
            where T : IComponent
        {
            if (!_components.Contains(_state.GetIdByType<T>()))
            {
                _components.Add(_state.GetIdByType<T>());
            }

            return this;
        }

        public ThreadRestrictionsBuilder Use<T0>(Filter<T0> filter)
            where T0 : IComponent
        {
            Use<T0>();
            return this;
        }

        public ThreadRestrictionsBuilder Use<T0, T1>(Filter<T0, T1> filter)
            where T0 : IComponent
            where T1 : IComponent
        {
            Use<T0>();
            Use<T1>();
            return this;
        }

        public ThreadRestrictionsBuilder Use<T0, T1, T2>(Filter<T0, T1, T2> filter)
            where T0 : IComponent
            where T1 : IComponent
            where T2 : IComponent
        {
            Use<T0>();
            Use<T1>();
            Use<T2>();
            return this;
        }

        public ThreadRestrictionsBuilder Use<T0, T1, T2, T3>(Filter<T0, T1, T2, T3> filter)
            where T0 : IComponent
            where T1 : IComponent
            where T2 : IComponent
            where T3 : IComponent
        {
            Use<T0>();
            Use<T1>();
            Use<T2>();
            Use<T3>();
            return this;
        }

        public ThreadRestrictionsBuilder Use<T0, T1, T2, T3, T4>(Filter<T0, T1, T2, T3, T4> filter)
            where T0 : IComponent
            where T1 : IComponent
            where T2 : IComponent
            where T3 : IComponent
            where T4 : IComponent
        {
            Use<T0>();
            Use<T1>();
            Use<T2>();
            Use<T3>();
            Use<T4>();
            return this;
        }

        public ThreadRestrictionsBuilder Use<T0, T1, T2, T3, T4, T5>(Filter<T0, T1, T2, T3, T4, T5> filter)
            where T0 : IComponent
            where T1 : IComponent
            where T2 : IComponent
            where T3 : IComponent
            where T4 : IComponent
            where T5 : IComponent
        {
            Use<T0>();
            Use<T1>();
            Use<T2>();
            Use<T3>();
            Use<T4>();
            Use<T5>();
            return this;
        }

        public ThreadRestrictionsBuilder Use<T0, T1, T2, T3, T4, T5, T6>(Filter<T0, T1, T2, T3, T4, T5, T6> filter)
            where T0 : IComponent
            where T1 : IComponent
            where T2 : IComponent
            where T3 : IComponent
            where T4 : IComponent
            where T5 : IComponent
            where T6 : IComponent
        {
            Use<T0>();
            Use<T1>();
            Use<T2>();
            Use<T3>();
            Use<T4>();
            Use<T5>();
            Use<T6>();
            return this;
        }

        public ThreadRestrictionsBuilder Use<T0, T1, T2, T3, T4, T5, T6, T7>(Filter<T0, T1, T2, T3, T4, T5, T6, T7> filter)
            where T0 : IComponent
            where T1 : IComponent
            where T2 : IComponent
            where T3 : IComponent
            where T4 : IComponent
            where T5 : IComponent
            where T6 : IComponent
            where T7 : IComponent
        {
            Use<T0>();
            Use<T1>();
            Use<T2>();
            Use<T3>();
            Use<T4>();
            Use<T5>();
            Use<T6>();
            Use<T7>();
            return this;
        }

        internal ThreadRestrictions Build()
        {
            _components.Sort();
            return new ThreadRestrictions(_components);
        }


        internal struct ThreadRestrictions : IDisposable
        {
            public NList<BAllocator, ushort> components;

            public bool IsValid
                => components.IsValid;

            public bool IsEmpty
                => components.Count == 0;

            public ThreadRestrictions(BAllocator* allocator)
            {
                components = new NList<BAllocator, ushort>(allocator, 16);
            }

            public ThreadRestrictions(NList<BAllocator, ushort> components)
            {
                this.components = components;
            }

            public bool IsCollision(in ThreadRestrictions other)
            {
                for (uint i = 0; i < components.Count; ++i)
                {
                    for (uint j = 0; j < other.components.Count; ++j)
                    {
                        if (components.Read(i) >= other.components.Read(j))
                        {
                            if (components.Read(i) == other.components.Read(j))
                            {
                                return true;
                            }
                            break;
                        }
                    }        
                }
                return false;
            }

            public void Add(in ThreadRestrictions other)
            {
                if (other.components.IsValid)
                {
                    for (uint i = 0; i < other.components.Count; ++i)
                    {
                        components.AddSort(other.components.Read(i));
                    }
                }
            }

            public void Clear()
            {
                components.Clear();
            }

            public void Dispose()
            {
                components.Dispose();
            }
        }
    }
}