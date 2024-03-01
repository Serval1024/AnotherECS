using AnotherECS.Core.Allocators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    public struct SystemGroup :
        IGroupSystemInternal,
        IEnumerable<ISystem>,
        IDisposable
    {
        private bool _isInit;
        private bool _isDisposed;
        private SortOrder _order;
        private readonly List<ISystem> _systems;

        public SortOrder SortOrder
        {
            get => _order;
            set => _order = value;
        }

        public int SystemCount
            => _systems.Count;

        public SystemGroup(SortOrder order = SortOrder.Declaration)
        {
            _isInit = true;
            _order = order;
            _systems = new List<ISystem>();
            _isDisposed = false;
        }

        public SystemGroup(ISystem system, SortOrder order = SortOrder.Declaration)
            : this(order)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            _systems.Add(system);
        }

        public SystemGroup(IEnumerable<ISystem> systems, SortOrder order = SortOrder.Declaration)
            : this(order)
        {
            if (systems == null)
            {
                throw new ArgumentNullException(nameof(systems));
            }

            _systems.AddRange(systems);

            for (int i = 0; i < _systems.Count; ++i)
            {
                if (_systems[i] == null)
                {
                    throw new ArgumentNullException(nameof(systems));
                }
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
#if !ANOTHERECS_RELEASE
            ThrowIfDisposed();
#endif
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            Init();
            _systems.Remove(system);
        }

        public IEnumerator<ISystem> GetEnumerator()
        {
#if !ANOTHERECS_RELEASE
            ThrowIfDisposed();
#endif
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
#if !ANOTHERECS_RELEASE
            ThrowIfDisposed();
#endif
            Init();
            foreach (var system in _systems)
            {
                if (system is IGroupSystem iGroupSystem)
                {
                    foreach (var childSystem in iGroupSystem.GetSystemsAll())
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
#if !ANOTHERECS_RELEASE
            ThrowIfDisposed();
#endif
            Init();
            return _systems;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        void IInstallSystem.Install(ref InstallContext context)
        {
#if !ANOTHERECS_RELEASE
            ThrowIfDisposed();
#endif
            Init();

            for (int i = 0; i < _systems.Count; ++i)
            {
                if (_systems[i] is IInstallSystem iInstallSystem)
                {
                    var childContext = new InstallContext(context.World);
                    iInstallSystem.Install(ref childContext);

                    var systemGroup = childContext.GetSystemGroup();
                    GroupSystemInternalCaller.Install(ref systemGroup, ref childContext);
                    
                    childContext.AddSystem(_systems[i]);
                    _systems[i] = systemGroup.SystemCount == 1 ? systemGroup.First() : systemGroup;
                }
            }
        }

        void IGroupSystemInternal.Append(ISystem system)
        {
#if !ANOTHERECS_RELEASE
            ThrowIfDisposed();
#endif
            Init();
            _systems.Add(system);
        }

        void IGroupSystemInternal.Prepend(ISystem system)
        {
#if !ANOTHERECS_RELEASE
            ThrowIfDisposed();
#endif
            Init();
            _systems.Insert(0, system);
        }

        void IGroupSystemInternal.Sort()
        {
#if !ANOTHERECS_RELEASE
            ThrowIfDisposed();
#endif
            if (_order == SortOrder.Attributes)
            {
                Init();

                var order = SystemGlobalRegister.GetOrders();
                _systems.Sort((p0, p1) =>
                     (order.TryGetValue(p0.GetType(), out var v0) && order.TryGetValue(p1.GetType(), out var v1))
                     ? (v0 - v1)
                     : 0);

                for (int i = 0; i < _systems.Count; ++i)
                {
                    if (_systems[i] is IGroupSystemInternal group)
                    {
                        group.Sort();
                    }
                }
            }
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

        private static class GroupSystemInternalCaller
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Install<T>(ref T data, ref InstallContext installContext)
                where T : struct, IGroupSystemInternal
            {
                data.Install(ref installContext);
            }
        }
    }

    public enum SortOrder
    {
        Declaration,
        Attributes,
    }
}