using System;
using System.Linq;
using System.Text;
using AnotherECS.Collections;
using AnotherECS.Core;
using AnotherECS.Serializer;

namespace AnotherECS.Generator
{
    internal static class TypeOptionsUtils
    {
        public static string GetCallerFlags(in TypeOptions option)
        {
            var result = GetDefaultStorageFlags(option);

            if (result.Length == 0)
            {
                result.Append("Simple");
            }
            return result.ToString();
        }

        private static StringBuilder GetDefaultStorageFlags(in TypeOptions option)
        {
            var result = new StringBuilder();
            if (option.isHistoryByChange)
            {
                result.Append("H");
            }
            if (option.isHistoryByTick)
            {
                result.Append("T");
            }
            if (option.isCopyable)
            {
                result.Append("C");
            }
            if (option.isUnmanaged)
            {
                result.Append("B");
            }
            if (option.isVersion)
            {
                result.Append("V");
            }
            if (option.isAttach)
            {
                result.Append("A");
            }
            if (option.isDetach)
            {
                result.Append("D");
            }
            if (option.isSingle)
            {
                result.Append("S");
            }
            if (option.isMarker)
            {
                result.Append("M");
            }
            if (option.sparseMode == TypeOptions.SparseMode.Bool)
            {
                result.Append("L0");
            }
            if (option.sparseMode == TypeOptions.SparseMode.Byte)
            {
                result.Append("L1");
            }
            if (option.sparseMode == TypeOptions.SparseMode.Ushort)
            {
                result.Append("L2");
            }
            if (option.isEmpty)
            {
                result.Append("Empty");
            }
            if (option.isUseISerialize)
            {
                result.Append("Ser");
            }
            if (option.isInject)
            {
                result.Append("Inj");
            }
            if (option.isCompileFastAccess)
            {
                result.Append("Fast");
            }
            
            return result;
        }

        public static string GetCallerInterfaces(in TypeOptions option)
        {
            var result = new StringBuilder();
            if (option.isCopyable)
            {
                result.Append(", ");
                result.Append(nameof(ICopyable));
                result.Append("<TComponent>");
            }
            if (option.isAttach)
            {
                result.Append(", ");
                result.Append(nameof(IAttach));
            }
            if (option.isDetach)
            {
                result.Append(", ");
                result.Append(nameof(IDetach));
            }
            if (option.isSingle)
            {
                result.Append(", ");
                result.Append(nameof(IShared));
            }
            if (option.isUseISerialize)
            {
                result.Append(", ");
                result.Append(nameof(ISerialize));
            }
            
            return result.ToString();
        }

        public static string GetHistoryInterfaces(in TypeOptions option)
        {
            var result = new StringBuilder();
            if (option.isCopyable)
            {
                result.Append(", ");
                result.Append(nameof(ICopyable));
                result.Append("<T>");
            }
            if (option.isAttach)
            {
                result.Append(", ");
                result.Append(nameof(IAttach));
            }
            if (option.isDetach)
            {
                result.Append(", ");
                result.Append(nameof(IDetach));
            }
            if (option.isSingle)
            {
                result.Append(", ");
                result.Append(nameof(IShared));
            }
            return result.ToString();
        }
    }

    internal struct TypeOptions
    {
        public Type type;

        public bool isHistory;
        public bool isHistoryByChange;
        public bool isHistoryByTick;

        public bool isEmpty;

        public bool isSingle;
        public bool isCopyable;
        public bool isUnmanaged;
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

        public bool isUseISerialize;
        public bool isUseRecycle;
        public bool isBindToEntity;

        public bool isConfig;

        public bool isDispose;

        public TypeOptions(Type type)
        {
            this.type = type;

            isMarker = ComponentUtils.IsMarker(type);

#if ANOTHERECS_HISTORY_DISABLE
            isHistoryByChange = false;
            isHistoryByTick = false;
#else
            isHistoryByChange = ComponentUtils.IsHistoryByChange(type) && !isMarker;
            isHistoryByTick = ComponentUtils.IsHistoryByTick(type) && !isMarker;
#endif
            isHistory = isHistoryByChange || isHistoryByTick;

            isEmpty = ComponentUtils.IsEmpty(type);

            isSingle = ComponentUtils.IsShared(type);
            isCopyable = ComponentUtils.IsCopyable(type);
            isUnmanaged = ComponentUtils.IsUnmanaged(type);
            isVersion = ComponentUtils.IsVersion(type);
            isAttach = ComponentUtils.IsAttach(type);
            isDetach = ComponentUtils.IsDetach(type);
            sparseMode = GetSparseMode(type);
            isCompileFastAccess = ComponentUtils.IsCompileFastAccess(type);

            isInjectComponent = !isEmpty && ComponentUtils.IsInjectComponent(type);
            isInjectMembers = !isEmpty && ComponentUtils.IsInjectMembers(type);
            isInject = isInjectComponent | isInjectMembers;
            injectMembers = isInjectMembers ? ComponentUtils.GetInjectToMembers(type) : Array.Empty<ComponentUtils.InjectData>();

            isUseISerialize = !isEmpty && ComponentUtils.IsUseISerialize(type);
            isUseRecycle = !isMarker && !isEmpty && !isSingle;
            isBindToEntity = !isMarker;

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
            else if (ComponentUtils.IsStorageLimit255(type))
            {
                return SparseMode.Byte;
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
            if (isHistoryByChange && isHistoryByTick)
            {
                throw new Exceptions.OptionsConflictException(type, $"{nameof(ComponentOptions.HistoryByChange)}, {nameof(ComponentOptions.HistoryByTick)}.");
            }
            if (isCopyable && isEmpty)
            {
                throw new Exceptions.OptionsConflictException(type, $"{nameof(ICopyable)}, {nameof(ComponentOptions.DataFree)}.");
            }
            if (isHistoryByTick && isEmpty)
            {
                throw new Exceptions.OptionsConflictException(type, $"{nameof(ComponentOptions.HistoryByTick)}, {ComponentOptions.DataFree}.");
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
            Byte,
            Ushort,
        }
    }

}
