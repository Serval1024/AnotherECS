using System;
using AnotherECS.Core;

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

        public bool isCompileFastAccess;

        public bool isInject;
        public bool isInjectComponent;
        public bool isInjectMembers;
        public ComponentUtils.FieldData[] injectMembers;

        public bool isRebindMemory;
        public bool isRebindMemoryComponent;
        public bool isRebindMemoryMembers;
        public ComponentUtils.FieldData[] rebindMemoryMembers;

        public bool isUnmanaged;
        public bool isBlittable;

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

            isCompileFastAccess = ComponentUtils.IsCompileFastAccess(type);

            isInjectComponent = !isEmpty && ComponentUtils.IsInjectComponent(type);
            isInjectMembers = !isEmpty && ComponentUtils.IsInjectMembers(type);
            isInject = isInjectComponent | isInjectMembers;
            injectMembers = isInjectMembers ? ComponentUtils.GetFieldToMembers<IInject>(type) : Array.Empty<ComponentUtils.FieldData>();

            isRebindMemoryComponent = !isEmpty && ComponentUtils.IsRebindMemory(type);
            isRebindMemoryMembers = !isEmpty && ComponentUtils.IsRebindMemoryMembers(type);
            isRebindMemory = isRebindMemoryComponent | isRebindMemoryMembers;
            rebindMemoryMembers = isRebindMemoryMembers ? ComponentUtils.GetFieldToMembers<IRebindMemoryHandle>(type) : Array.Empty<ComponentUtils.FieldData>();

            isUnmanaged = ComponentUtils.IsUnmanaged(type);
            isBlittable = ComponentUtils.IsBlittable(type);

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
