using System;
using System.Linq;
using AnotherECS.Collections;
using AnotherECS.Core;

namespace AnotherECS.Generator
{
    internal struct TypeOptions
    {
        public Type type;

        public bool isHistory;
        public bool isHistoryByChange;
        public bool isHistoryByTick;
        public bool isHistoryByVersion;

        public bool isEmpty;

        public bool isDefault;
        public bool isSingle;
        public bool isCopyable;
        public bool isVersion;
        public bool isAttach;
        public bool isDetach;
        public bool isMarker;
        public SparseMode sparseMode;

        public bool isCompileFastAccess;

        public bool isInject;
        public bool isInjectComponent;
        public bool isInjectMembers;
        public ComponentUtils.InjectData[] injectMembers;

        public bool isUnmanaged;
        public bool isBlittable;

        public bool isUseISerialize;
        public bool isUseRecycle;
        public bool isBindToEntity;
        public bool isDispose;
        public bool isConfig;

        public TypeOptions(Type type)
        {
            this.type = type;

            isMarker = ComponentUtils.IsMarker(type);

            isHistoryByChange = ComponentUtils.IsHistoryByChange(type) && !isMarker;
            isHistoryByTick = ComponentUtils.IsHistoryByTick(type) && !isMarker;
            isHistoryByVersion = ComponentUtils.IsHistoryByVersion(type) && !isMarker;
            isHistory = isHistoryByChange || isHistoryByTick || isHistoryByVersion;

            isEmpty = ComponentUtils.IsEmpty(type);

            isDefault = ComponentUtils.IsDefault(type);
            isSingle = ComponentUtils.IsShared(type);
            isCopyable = ComponentUtils.IsCopyable(type) && !isEmpty;
            isVersion = ComponentUtils.IsVersion(type) || isHistoryByVersion;
            isAttach = ComponentUtils.IsAttach(type);
            isDetach = ComponentUtils.IsDetach(type);
            sparseMode = GetSparseMode(type);

            isCompileFastAccess = ComponentUtils.IsCompileFastAccess(type);

            isInjectComponent = !isEmpty && ComponentUtils.IsInjectComponent(type);
            isInjectMembers = !isEmpty && ComponentUtils.IsInjectMembers(type);
            isInject = isInjectComponent | isInjectMembers;
            injectMembers = isInjectMembers ? ComponentUtils.GetInjectToMembers(type) : Array.Empty<ComponentUtils.InjectData>();

            isUnmanaged = ComponentUtils.IsUnmanaged(type);
            isBlittable = ComponentUtils.IsBlittable(type);

            isUseISerialize = !isEmpty && (ComponentUtils.IsUseISerialize(type) || !isBlittable);
            isUseRecycle = !isMarker && !isEmpty && !isSingle;
            isBindToEntity = !isSingle;
            isConfig = ComponentUtils.IsConfig(type);
            isDispose = isAttach || isDetach || isCopyable;

            Validate();
        }

        private static SparseMode GetSparseMode(Type type)
        {
            if (ComponentUtils.IsWithoutSparseDirectDense(type) || ComponentUtils.IsShared(type) || ComponentUtils.IsEmpty(type))
            {
                return SparseMode.Bool;
            }
            
            return SparseMode.Ushort;
        }

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
            if (((isHistoryByChange ? 1 : 0) + (isHistoryByTick ? 1 : 0) + (isHistoryByVersion ? 1 : 0)) > 1)
            {
                throw new Exceptions.OptionsConflictException(type, $"{nameof(ComponentOptions.HistoryByChange)}, {nameof(ComponentOptions.HistoryByTick)}, {nameof(ComponentOptions.HistoryByVersion)}.");
            }
            if (isCopyable && isEmpty)
            {
                throw new Exceptions.OptionsConflictException(type, $"{nameof(ICopyable)}, {nameof(ComponentOptions.DataFree)}.");
            }
            if (isDefault && isEmpty)
            {
                throw new Exceptions.OptionsConflictException(type, $"{nameof(IDefault)}, {nameof(ComponentOptions.DataFree)}.");
            }
            if (isHistoryByTick && isEmpty)
            {
                throw new Exceptions.OptionsConflictException(type, $"{nameof(ComponentOptions.HistoryByTick)}, {ComponentOptions.DataFree}.");
            }
            if (isHistoryByVersion && isEmpty)
            {
                throw new Exceptions.OptionsConflictException(type, $"{nameof(ComponentOptions.HistoryByVersion)}, {ComponentOptions.DataFree}.");
            }
            if (!isHistory && isInject)
            {
                foreach (var member in injectMembers)
                {
                    if (member.argumentTypes.Any(p1 => p1 == nameof(DArrayCaller)))
                    {
                        throw new Exceptions.OptionsConflictException(type, $"{typeof(DArray<>).Name}, {typeof(DList<>).Name} with 'no history' option.");
                    }
                }
            }
        }

        public enum SparseMode
        {
            Bool,
            Ushort,
        }
    }

}
