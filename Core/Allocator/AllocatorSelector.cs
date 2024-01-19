using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    public readonly unsafe struct AllocatorSelector : IAllocator
    {
        private readonly AllocatorType _allocatorType;
        private readonly BAllocator* _bAllocator;
        private readonly HAllocator* _hAllocator;

        public AllocatorSelector(AllocatorType allocatorType, BAllocator* bAllocator, HAllocator* hAllocator)
        {
            _allocatorType = allocatorType;
            _bAllocator = bAllocator;
            _hAllocator = hAllocator;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bAllocator != null && _hAllocator != null;
        }

        public MemoryHandle Allocate(uint size)
            => _allocatorType switch
            {
                AllocatorType.HAllocator => _hAllocator->Allocate(size),
                AllocatorType.BAllocator => _bAllocator->Allocate(size),
                _ => throw new System.NotImplementedException(),
            };

        public void Deallocate(ref MemoryHandle memoryHandle)
        {
            switch(_allocatorType)
            {
                case AllocatorType.HAllocator:
                    {
                        _hAllocator->Deallocate(ref memoryHandle);
                        break;
                    }
                case AllocatorType.BAllocator:
                    {
                        _bAllocator->Deallocate(ref memoryHandle);
                        break;
                    }
            }
        }

        public void Dirty(ref MemoryHandle memoryHandle)
        {
            switch (_allocatorType)
            {
                case AllocatorType.HAllocator:
                    {
                        _hAllocator->Dirty(ref memoryHandle);
                        break;
                    }
                case AllocatorType.BAllocator:
                    {
                        _bAllocator->Dirty(ref memoryHandle);
                        break;
                    }
            }
        }

        public void EnterCheckChanges(ref MemoryHandle memoryHandle)
        {
            switch (_allocatorType)
            {
                case AllocatorType.HAllocator:
                    {
                        _hAllocator->EnterCheckChanges(ref memoryHandle);
                        break;
                    }
                case AllocatorType.BAllocator:
                    {
                        _bAllocator->EnterCheckChanges(ref memoryHandle);
                        break;
                    }
            }
        }

        public bool ExitCheckChanges(ref MemoryHandle memoryHandle)
            => _allocatorType switch
            {
                AllocatorType.HAllocator => _hAllocator->ExitCheckChanges(ref memoryHandle),
                AllocatorType.BAllocator => _bAllocator->ExitCheckChanges(ref memoryHandle),
                _ => throw new System.NotImplementedException(),
            };

        public uint GetId()
            => _allocatorType switch
            {
                AllocatorType.HAllocator => _hAllocator->GetId(),
                AllocatorType.BAllocator => _bAllocator->GetId(),
                _ => throw new System.NotImplementedException(),
            };

        public void Repair(ref MemoryHandle memoryHandle)
        {
            switch (_allocatorType)
            {
                case AllocatorType.HAllocator:
                    {
                        _hAllocator->Repair(ref memoryHandle);
                        break;
                    }
                case AllocatorType.BAllocator:
                    {
                        _bAllocator->Repair(ref memoryHandle);
                        break;
                    }
            }
        }

        public void Reuse(ref MemoryHandle memoryHandle, uint size)
        {
            switch (_allocatorType)
            {
                case AllocatorType.HAllocator:
                    {
                        _hAllocator->Reuse(ref memoryHandle, size);
                        break;
                    }
                case AllocatorType.BAllocator:
                    {
                        _bAllocator->Reuse(ref memoryHandle, size);
                        break;
                    }
            }
        }

        public bool TryResize(ref MemoryHandle memoryHandle, uint size)
             => _allocatorType switch
             {
                 AllocatorType.HAllocator => _hAllocator->TryResize(ref memoryHandle, size),
                 AllocatorType.BAllocator => _bAllocator->TryResize(ref memoryHandle, size),
                 _ => throw new System.NotImplementedException(),
             };

        public TAllocator* SelectAllocator<TAllocator>()
            where TAllocator : unmanaged, IAllocator
        {
            if (typeof(TAllocator) == typeof(BAllocator))
            {
                return (TAllocator*)_bAllocator;
            }
            else if (typeof(TAllocator) == typeof(HAllocator))
            {
                return (TAllocator*)_hAllocator;
            }
            throw new System.NotSupportedException();
        }
    }

    public enum AllocatorType : byte
    {
        BAllocator = 1,
        HAllocator = 2,
    }
}

