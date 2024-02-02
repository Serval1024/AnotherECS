using System;
using System.Collections;
using System.Collections.Generic;

namespace AnotherECS.Core
{
    public struct SystemGroup :
        IGroupSystemInternal,
        IEnumerable<ISystem>,
        IDisposable
    {
        private bool _isInit;
        private bool _isDisposed;
        private readonly List<ISystem> _systems;
        private readonly SortOrder _order;

        public SystemGroup(SortOrder order = SortOrder.Attributes)
        {
            _isInit = true;
            _order = order;
            _systems = new List<ISystem>();
            _isDisposed = false;
        }

        public SystemGroup(IEnumerable<ISystem> systems, SortOrder order = SortOrder.Attributes)
            : this(order)
        {
            if (systems == null)
            {
                throw new ArgumentNullException(nameof(systems));
            }

            foreach (var system in systems)
            {
                Add(system);
            }
        }

        public SystemGroup Add(ISystem system)
        {
#if !ANOTHERECS_RELEASE
            ThrowIfDisposed();
#endif
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            Init();
            _systems.Add(system);
            return this;
        }

        public void Remove(ISystem system)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            Init();
            _systems.Remove(system);
        }

        public IEnumerator<ISystem> GetEnumerator()
        {
            Init();
            return _systems.GetEnumerator();
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;

                if (_systems != null)
                {
                    for (int i = 0; i < _systems.Count; ++i)
                    {
                        if (_systems[i] is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                    }
                    _systems.Clear();
                }
            }
        }

        public IEnumerable<ISystem> GetSystemsAll()
        {
            Init();
            foreach (var system in _systems)
            {
                if (system is IEnumerable<ISystem> enumerable)
                {
                    foreach (var childSystem in enumerable)
                    {
                        yield return childSystem;
                    }
                }
                else
                {
                    yield return system;
                }
            }
        }

        internal IEnumerable<ISystem> GetSystems()
        {
            Init(); 
            return _systems;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        void IGroupSystemInternal.Sort()
        {
            if (_order == SortOrder.Attributes)
            {
                Init();

                var order = SystemGlobalRegister.GetOrders();
                _systems.Sort((p0, p1) =>
                     (order.TryGetValue(p0.GetType(), out int v0) && order.TryGetValue(p1.GetType(), out int v1)) 
                     ? (v0 - v1)
                     : 0
                );
            }
        }

        void IGroupSystemInternal.Prepend(ISystem system)
        {
            Init();
            _systems.Insert(0, system);
        }

        private void Init()
        {
            if (!_isInit)
            {
                this = new SystemGroup(SortOrder.Attributes);
            }
        }

#if !ANOTHERECS_RELEASE
        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(SystemGroup));
            }
        }
#endif

        public enum SortOrder
        {
            Attributes,
            Declaration,
        }
    }
}