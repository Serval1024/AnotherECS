using System;
using AnotherECS.Core;
using AnotherECS.Core.Caller;

namespace AnotherECS.Generator
{
    internal struct TypeOptions
    {
        public Type type;

        public bool isHistory;

        public bool isEmpty;

        public bool isDefault;
        public bool isSingle;
        public bool isVersion;
        public bool isAttach;
        public bool isDetach;
        public bool isMarker;
        public SparseMode sparseMode;
        public AllocatorType allocatorType;

        public bool isCompileFastAccess;

        public bool isInject;
        public bool isInjectComponent;
        public bool isInjectMembers;
        public ComponentUtils.FieldData[] injectMembers;

        public bool isRepairMemory;
        public bool isRepairMemoryComponent;
        public bool isRepairMemoryMembers;
        public ComponentUtils.FieldData[] repairMemoryMembers;

        public bool isRepairStateId;
        public bool isRepairStateIdComponent;
        public bool isRepairStateIdMembers;
        public ComponentUtils.FieldData[] repairStateIdMembers;

        public bool isUnmanaged;
        //public bool isBlittable;

        public bool isUseRecycle;
        public bool isBindToEntity;
        public bool isDispose;
        public bool isConfig;

        public TypeOptions(Type type)
        {
            this.type = type;

            isMarker = ComponentUtils.IsMarker(type);

            isHistory = ComponentUtils.IsHistory(type) && !isMarker;

            isEmpty = ComponentUtils.IsEmpty(type);

            isDefault = ComponentUtils.IsDefault(type);
            isSingle = ComponentUtils.IsSingle(type);
            isVersion = ComponentUtils.IsVersion(type);
            isAttach = ComponentUtils.IsAttach(type);
            isDetach = ComponentUtils.IsDetach(type);
            sparseMode = GetSparseMode(type);
            allocatorType = ComponentUtils.GetAllocator(type);

            isCompileFastAccess = ComponentUtils.IsCompileFastAccess(type);

            isInjectComponent = !isEmpty && ComponentUtils.IsInjectComponent(type);
            isInjectMembers = !isEmpty && ComponentUtils.IsInjectMembers(type);
            isInject = isInjectComponent | isInjectMembers;
            injectMembers = isInjectMembers ? ComponentUtils.GetFieldToMembers<IInject>(type) : Array.Empty<ComponentUtils.FieldData>();

            isRepairMemoryComponent = !isEmpty && ComponentUtils.IsRepairMemory(type);
            isRepairMemoryMembers = !isEmpty && ComponentUtils.IsRepairMemoryMembers(type);
            isRepairMemory = isRepairMemoryComponent | isRepairMemoryMembers;
            repairMemoryMembers = isRepairMemoryMembers ? ComponentUtils.GetFieldToMembers<IRepairMemoryHandle>(type) : Array.Empty<ComponentUtils.FieldData>();

            isRepairStateIdComponent = !isEmpty && ComponentUtils.IsRepairStateId(type);
            isRepairStateIdMembers = !isEmpty && ComponentUtils.IsRepairStateIdMembers(type);
            isRepairStateId = isRepairStateIdComponent | isRepairStateIdMembers;
            repairStateIdMembers = isRepairStateIdMembers ? ComponentUtils.GetFieldToMembers<IRepairStateId>(type) : Array.Empty<ComponentUtils.FieldData>();

            isUnmanaged = ComponentUtils.IsUnmanaged(type);
            //isBlittable = ComponentUtils.IsBlittable(type);

            isUseRecycle = !isMarker && !isEmpty && !isSingle;
            isBindToEntity = !isSingle;
            isConfig = ComponentUtils.IsConfig(type);
            isDispose = false;

            Validate();
        }

        private static SparseMode GetSparseMode(Type type)
            => (ComponentUtils.IsWithoutSparseDirectDense(type) || ComponentUtils.IsSingle(type) || ComponentUtils.IsEmpty(type))
            ? SparseMode.Bool
            : SparseMode.Ushort;

        private void Validate()
        {
            if (!isUnmanaged)
            {
                throw new Exceptions.OptionsConflictException(type, $"The component must not contain reference types.");
            }
            if (isMarker && isHistory)
            {
                throw new Exceptions.OptionsConflictException(type, $"{nameof(IMarker)}, Any history option.");
            }
            if (isDefault && isEmpty)
            {
                throw new Exceptions.OptionsConflictException(type, $"{nameof(IDefault)}, {nameof(ComponentOptions.DataFree)}.");
            }
        }

        public enum SparseMode
        {
            Bool,
            Ushort,
        }
    }

}
