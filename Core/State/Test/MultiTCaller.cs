using AnotherECS.Core.Actions;
using AnotherECS.Core.Collection;
using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal struct Nothing : IAttachDetachStorage, IEmptyStorage
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(State state, ref GlobalDepencies depencies) { }
        public void Init() { }
    }

    internal interface IAttachDetachStorage
    {
        void Init(State state, ref GlobalDepencies depencies);
    }

    internal interface IEmptyStorage
    {
        void Init();
    }

    internal interface ILayoutAllocator<QComponent>
        where QComponent : unmanaged
    {
        void Init(ref UnmanagedLayout<QComponent> layout, ref GlobalDepencies depencies);
    }

    

    internal struct AttachDetachStorage<QSparse> : IAttachDetachStorage
        where QSparse : unmanaged
    {
        public State state;
        public ArrayPtr<QSparse> bufferCopyTemp;
        public ArrayPtr<Op> opsTemp;

        public void Init(State state, ref GlobalDepencies depencies)
        {            
            this.state = state;
            bufferCopyTemp.Allocate(depencies.config.general.entityCapacity);
            opsTemp.Allocate(depencies.config.general.entityCapacity);
        }
    }

    internal struct EmptyStorage<QComponent> : IAttachDetachStorage
        where QComponent : unmanaged
    {
        public DefaultContainer storage;

        public void Init(State state, ref GlobalDepencies depencies)
        {
            storage = new DefaultContainer();
        }

        public class DefaultContainer
        {
            public QComponent value;
        }
    }

    internal struct LayoutAllocator<QComponent> : ILayoutAllocator<QComponent>
        where QComponent : unmanaged
    {
        public void Init(ref UnmanagedLayout<QComponent> layout, ref GlobalDepencies depencies)
        {
            //CallerFacadeActions<QComponent>.AllocateSparse <#STORAGE:MODE#><#IF MULTI#><<#SPARSE:TYPE_NAME#>><#END#>(ref *_layout, ref *_depencies, <#HISTORY:FLAG#>);
            //CallerFacadeActions<QComponent>.AllocateSparseMulti<>
        }
    }



    internal unsafe struct Caller
        <
        QSparse,
        WDense,
        EAttachDetachStorage,
        REmptyStorage
        >
        : ICaller
        where QSparse : unmanaged
        where WDense : unmanaged
        where EAttachDetachStorage : struct, IAttachDetachStorage
        where REmptyStorage : struct, IEmptyStorage
    {
        private UnmanagedLayout<QSparse, WDense>* _layout;
        private GlobalDepencies* _depencies;
        private ushort _elementId;

        EAttachDetachStorage attachDetachStorage;
        REmptyStorage emptyStorage;

        Type ICaller.GetElementType()
            => typeof(WDense);
        
        void ICaller.Config(UnmanagedLayout* layout, GlobalDepencies* depencies, ushort id, State state)
        {
            _layout = (UnmanagedLayout<QSparse, WDense>*)layout;
            _depencies = depencies;
            _elementId = id;
            attachDetachStorage.Init(state, ref *depencies);
            emptyStorage.Init();
        }


        void ICaller.AllocateLayout()
        {
            CallerFacadeActions<QSparse, WDense>.AllocateSparse <#STORAGE:MODE#><#IF MULTI#><<#SPARSE:TYPE_NAME#>><#END#>(ref *_layout, ref *_depencies, <#HISTORY:FLAG#>);
<#IF !EMPTY#>
            CallerFacadeActions<TComponent>.AllocateDense <#STORAGE:MODE#>(ref *_layout, ref *_depencies, <#HISTORY:FLAG#>);
<#END#>
<#IF ALLOCATOR:RECYCLE#>
            CallerFacadeActions<TComponent>.AllocateRecycle <#STORAGE:MODE#>(ref *_layout, ref *_depencies, <#HISTORY:FLAG#>);
<#END#>
<#IF VERSION#>
            CallerFacadeActions<TComponent>.AllocateVersion <#STORAGE:MODE#>(ref *_layout, ref *_depencies, <#HISTORY:FLAG#>);
<#END#>
        
        }
    }
}
