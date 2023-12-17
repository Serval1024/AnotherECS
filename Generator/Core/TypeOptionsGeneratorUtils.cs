using System;
using System.Text;
using AnotherECS.Core;
using AnotherECS.Core.Caller;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;

namespace AnotherECS.Generator
{
    internal static class TypeOptionsGeneratorUtils
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
            if (option.isBindToEntity)
            {
                result.Append("NoBE");
            }
            if (option.isUseRecycle)
            {
                result.Append("R");
            }

            return result;
        }

        public static string GetCallerInterfaces(in TypeOptions option)
        {
            var result = new StringBuilder();
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
                result.Append(nameof(ISingle));
            }
            if (option.isUseISerialize)
            {
                result.Append(", ");
                result.Append(nameof(ISerialize));
            }
            
            return result.ToString();
        }

        public static StringBuilder GetCallerDeclaration(in TypeOptions option)
        {
            var (TAllocator, TSparse, TDenseIndex) = GetLayoutDeclaration(option);
            var layoutASCD = $"{TAllocator}, {TSparse}, TComponent, {TDenseIndex}";
            var layoutASC = $"{TAllocator}, {TSparse}, TComponent";
            var layoutAC = $"{TAllocator}, TComponent";
            var layoutC = $"TComponent";
            var layoutS = $"{TSparse}";

            var extraSpace = new string('\t', 4);

            var nothing = nameof(Nothing);
            var nothingSCDTC = $"{typeof(Nothing<,,,>).GetNameWithoutGeneric()}<{layoutASCD}>";
            var singleFeature = $"{typeof(SingleFeature<,,,>).GetNameWithoutGeneric()}<{layoutASCD}>";


            var result = new StringBuilder();
            result.Append("Caller<");
            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            result.Append(layoutASCD);
            result.Append(", ");

            result.Append(Environment.NewLine);
            result.Append("#if ANOTHERECS_HISTORY_DISABLE");
            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            result.Append(nameof(NoHistoryAllocatorProvider));
            result.Append(",");
            result.Append(Environment.NewLine);
            result.Append("#else");
            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            if (option.isHistory)
            {
                result.Append(nameof(HistoryAllocatorProvider));
            }
            else
            {
                result.Append(nameof(NoHistoryAllocatorProvider));
            }
            result.Append(",");
            result.Append(Environment.NewLine);
            result.Append("#endif");

            result.Append(Environment.NewLine);
            result.Append(extraSpace);

            if (option.isSingle || (option.isMarker && option.isEmpty))
            {
                result.Append(nameof(UintNumber));
            }
            else
            { 
                result.Append(nameof(UshortNumber));
            }

            result.Append(",");
            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            if (option.isInject)
            {
                result.Append($"{typeof(InjectFeature<,,,>).GetNameWithoutGeneric()}<{layoutASCD}>");
            }
            else
            {
                result.Append(nothingSCDTC);
            }
            result.Append(",");

            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            if (option.isUseRecycle)
            {
                result.Append($"{typeof(RecycleStorageFeature<,,,>).GetNameWithoutGeneric()}<{layoutASCD}>");
            }
            else if (option.isSingle)
            {
                result.Append($"{typeof(SingleStorageFeature<,,,>).GetNameWithoutGeneric()}<{layoutASCD}>");
            }
            else
            {
                result.Append($"{typeof(IncrementStorageFeature<,,,>).GetNameWithoutGeneric()}<{layoutASCD}>");
            }
            result.Append(",");

            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            if (option.isDefault)
            {
                result.Append($"{typeof(DefaultFeature<>).GetNameWithoutGeneric()}<{layoutC}>");
            }
            else
            {
                result.Append(nothingSCDTC);
            }
            result.Append(",");

            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            if (option.isAttach || option.isDetach)
            {
                result.Append($"{typeof(AttachDetachFeature<>).GetNameWithoutGeneric()}<{layoutS}>");
            }
            else
            {
                result.Append(nothingSCDTC);
            }
            result.Append(",");

            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            if (option.isAttach)
            {
                result.Append($"{typeof(AttachFeature<,,,>).GetNameWithoutGeneric()}<{layoutASCD}>");
            }
            else
            {
                result.Append(nothingSCDTC);
            }
            result.Append(",");

            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            if (option.isDetach)
            {
                result.Append($"{typeof(DetachFeature<,,,>).GetNameWithoutGeneric()}<{layoutASCD}>");
            }
            else
            {
                result.Append(nothingSCDTC);
            }
            result.Append(",");

            result.Append(Environment.NewLine);
            result.Append(extraSpace);

            if (option.isSingle)
            {
                result.Append($"{typeof(SingleSparseFeature<,>).GetNameWithoutGeneric()}<{layoutAC}>");
            }
            else
            {
                if (option.sparseMode == TypeOptions.SparseMode.Bool || option.isEmpty)
                {
                    if (option.isMarker)
                    {
                        result.Append($"{typeof(NonSparseFeature<,>).GetNameWithoutGeneric()}<{layoutAC}>");
                    }
                    else
                    {
                        result.Append($"{typeof(BoolSparseFeature<,>).GetNameWithoutGeneric()}<{layoutAC}>");
                    }
                }
                else if (option.sparseMode == TypeOptions.SparseMode.Ushort)
                {   
                    result.Append($"{typeof(UshortSparseFeature<,>).GetNameWithoutGeneric()} < {layoutAC}>");
                }
            }
            
            result.Append(",");

            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            if (option.isEmpty)
            {
                result.Append($"{typeof(EmptyFeature<,,,>).GetNameWithoutGeneric()}<{layoutASCD}>");
            }
            else
            {
                if (option.isSingle)
                {
                    result.Append(singleFeature);
                }
                else
                {
                    result.Append($"{typeof(UshortDenseFeature<,,>).GetNameWithoutGeneric()}<{layoutASC}>");
                }
            }
            result.Append(",");

            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            if (option.isBindToEntity)
            {
                if (option.isMarker)
                {
                    result.Append(nameof(TempBinderToFilters));
                }
                else
                {
                    result.Append(nameof(BinderToFilters));
                }
            }
            else
            {
                result.Append(nothing);
            }
            result.Append(",");

            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            if (option.isVersion)
            {
                if (option.isSingle)
                {
                    result.Append($"{typeof(UintVersionFeature<,,>).GetNameWithoutGeneric()}<{layoutASC}>");
                }
                else
                {
                    result.Append($"{typeof(UshortVersionFeature<,,>).GetNameWithoutGeneric()}<{layoutASC}>");
                }
            }
            else
            {
                result.Append(nothingSCDTC);
            }
            result.Append(",");

            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            if (option.isUseISerialize || !option.isBlittable)
            {
                result.Append($"{typeof(SSerialize<,,,>).GetNameWithoutGeneric()}<{layoutASCD}>");
            }
            else
            {
                if (option.isBlittable)
                {
                    result.Append($"{typeof(BSerialize<,,,>).GetNameWithoutGeneric()}<{layoutASCD}>");
                }
                else
                {
                    result.Append($"{typeof(CSerialize<,,,>).GetNameWithoutGeneric()}<{layoutASCD}>");
                }
            }

            result.Append(",");
            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            if (option.isRebindMemory)
            {
                result.Append($"{typeof(RebindMemoryFeature<,,,>).GetNameWithoutGeneric()}<{layoutASCD}>");
            }
            else
            {
                result.Append(nothingSCDTC);
            }

            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            result.Append(">");

            return result;
        }

        public static (string TAllocator, string TSparse, string TDenseIndex) GetLayoutDeclaration(in TypeOptions option)
        {
            string TAllocator = string.Empty;
            string TSparse = string.Empty;
            string TDenseIndex = string.Empty;


            switch (option.sparseMode)
            {
                case TypeOptions.SparseMode.Bool:
                    {
                        TSparse = "bool";
                        if (option.isSingle || (option.isMarker && option.isEmpty))
                        {
                            TDenseIndex = "uint";
                        }
                        else
                        {
                            TDenseIndex = "ushort";
                        }
                        break;
                    }
                case TypeOptions.SparseMode.Ushort:
                    {
                        TSparse = "ushort";
                        TDenseIndex = "ushort";
                        break;
                    }
            }

            if (option.isHistory)
            {
                TAllocator = nameof(HAllocator);
            }
            else
            {
                TAllocator = nameof(BAllocator);
            }

            return (TAllocator, TSparse, TDenseIndex);
        }
    }

}
