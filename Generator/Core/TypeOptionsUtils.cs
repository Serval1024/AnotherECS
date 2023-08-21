using System;
using System.Linq;
using System.Text;
using AnotherECS.Collections;
using AnotherECS.Core;

namespace AnotherECS.Generator
{
    internal static class TypeOptionsUtils
    {
        public static string GetAdapterFlags(in TypeOptions option, GeneratorContext.ComponentFilterData componentFilterData)
        {
            var (includes, excludes) = componentFilterData;

            var result = GetDefaultPoolFlags(option);

            if (includes.Contains(option.type))
            {
                result.Append("I");
            }
            if (excludes.Contains(option.type))
            {
                result.Append("E");
            }
            if (result.Length == 0)
            {
                result.Append("F");
            }
            return result.ToString();
        }

        public static string GetPoolFlags(in TypeOptions option)
        {
            var result = GetDefaultPoolFlags(option);

            if (result.Length == 0)
            {
                result.Append("F");
            }
            return result.ToString();
        }

        private static StringBuilder GetDefaultPoolFlags(in TypeOptions option)
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
            if (option.isBlittable)
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
            if (option.isShared)
            {
                result.Append("S");
            }
            if (option.isMarker)
            {
                result.Append("M");
            }
            if (option.isLimit255)
            {
                result.Append("L255");
            }
            if (option.isEmpty)
            {
                result.Append("Empty");
            }
            if (option.isExceptSparseDirectDense)
            {
                result.Append("Lite");
            }
            if (option.isForceUseISerialize)
            {
                result.Append("Ser");
            }
            if (option.isInject)
            {
                result.Append("Inj");
            }
            if (option.isCompileDirectAccess)
            {
                result.Append("Dir");
            }
            return result;
        }

        public static string GetPoolInterfaces(in TypeOptions option)
        {
            var result = new StringBuilder();
            if (option.isCopyable)
            {
                result.Append(", ");
                result.Append(nameof(ICopyable));
                result.Append("<T>");
            }
            if (option.isVersion)
            {
                result.Append(", ");
                result.Append(nameof(IVersion));
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
            if (option.isShared)
            {
                result.Append(", ");
                result.Append(nameof(IShared));
            }
            if (option.isMarker)
            {
                result.Append(", ");
                result.Append(nameof(IMarker));
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
            if (option.isShared)
            {
                result.Append(", ");
                result.Append(nameof(IShared));
            }
            return result.ToString();
        }

        public static string GetHistoryFlags(in TypeOptions option)
            => GetPoolFlags(option);
    }

    internal struct TypeOptions
    {
        public Type type;

        public bool isHistoryByChange;
        public bool isHistoryByTick;
        public bool isHistory;
        public bool isCopyable;
        public bool isBlittable;
        public bool isVersion;
        public bool isAttach;
        public bool isDetach;
        public bool isShared;
        public bool isMarker;
        public bool isDispose;
        public bool isLimit255;
        public bool isEmpty;
        public bool isExceptSparseDirectDense;
        public bool isCompileDirectAccess;
        public bool isOverrideCapacity;
        public int capacity;
        public bool isInjectComponent;
        public bool isInjectMembers;
        public bool isInject;
        public ComponentUtils.InjectData[] injectMembers;
        public bool isForceUseISerialize;

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
            isCopyable = ComponentUtils.IsCopyable(type);
            isBlittable = ComponentUtils.IsBlittable(type);
            isVersion = ComponentUtils.IsVersion(type);
            isAttach = ComponentUtils.IsAttach(type);
            isDetach = ComponentUtils.IsDetach(type);
            isShared = ComponentUtils.IsShared(type);
            isLimit255 = ComponentUtils.IsLimit255(type);
            isEmpty = ComponentUtils.IsEmpty(type);
            isExceptSparseDirectDense = !isEmpty && ComponentUtils.IsExceptSparseDirectDense(type);
            isCompileDirectAccess = ComponentUtils.IsCompileDirectAccess(type);
            isOverrideCapacity = ComponentUtils.IsOverrideCapacity(type);
            capacity = ComponentUtils.GetOverrideCapacity(type);
            isInjectComponent = !isEmpty && ComponentUtils.IsInjectComponent(type);
            isInjectMembers = !isEmpty && ComponentUtils.IsInjectMembers(type);
            isInject = isInjectComponent | isInjectMembers;
            injectMembers = isInjectMembers ? ComponentUtils.GetInjectToMembers(type) : Array.Empty<ComponentUtils.InjectData>();
            isForceUseISerialize = !isEmpty && ComponentUtils.IsForceUseISerialize(type);

            isDispose = false;

            Validate();
        }

        private void Validate()
        {
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
                    if (member.argumentTypes.Any(p1 => p1 == nameof(DArrayStorage)))
                    {
                        throw new Exceptions.OptionsConflictException(type, $"{typeof(DArray<>).Name}, {typeof(DList<>).Name} with 'no history' option.");
                    }
                }
            }
        }
    }

}
