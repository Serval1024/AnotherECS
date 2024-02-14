using AnotherECS.Core;
using AnotherECS.Core.Allocators;
using AnotherECS.Generator.Exceptions;
using System;
using System.Linq;

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

        public bool isForceUseSparse;
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
            repairStateIdMembers = isRepairStateIdMembers ? GetRepairStateId(type) : Array.Empty<ComponentUtils.FieldData>();

            isUnmanaged = ComponentUtils.IsUnmanaged(type);

            isForceUseSparse = ComponentUtils.IsForceUseSparse(type);
            isUseRecycle = !isMarker && !isEmpty && !isSingle;
            isBindToEntity = !isSingle;
            isConfig = ComponentUtils.IsConfig(type);
            isDispose = false;

            Validate();
        }

        private static ComponentUtils.FieldData[] GetRepairStateId(Type type)
            => ComponentUtils.GetFieldToMembers<IRepairStateId>(type)
                .Where(p => ((IRepairStateId)Activator.CreateInstance(p.fieldType)).IsRepairStateId)
                .ToArray();

        private static SparseMode GetSparseMode(Type type)
            => (ComponentUtils.IsWithoutSparseDirectDense(type) || ComponentUtils.IsSingle(type) || ComponentUtils.IsEmpty(type))
            ? SparseMode.Bool
            : SparseMode.Ushort;

        private void Validate()
        {
            if (!isUnmanaged)
            {
                throw new OptionsConflictException(type, $"The component must not contain reference types.");
            }
            if (isMarker && isHistory)
            {
                throw new OptionsConflictException(type, $"{nameof(IMarker)}, Any history option.");
            }
            if (isDefault && isEmpty)
            {
                throw new OptionsConflictException(type, $"{nameof(IDefault)}, {nameof(ComponentOptions.DataFree)}.");
            }
            if (isForceUseSparse && (isEmpty || isSingle))
            {
                throw new OptionsConflictException(type, $"{ComponentOptions.ForceUseSparse}, {nameof(ComponentOptions.DataFree)} or {nameof(ISingle)}.");
            }
        }

        public enum SparseMode
        {
            Bool,
            Ushort,
        }
    }

}
