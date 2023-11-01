using System;
using System.Text;
using AnotherECS.Core;
using AnotherECS.Core.Caller;
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

        public static StringBuilder GetCallerDeclaration(in TypeOptions option)
        {
            var (TSparse, TDenseIndex, TTickData) = GetLayoutDeclaration(option);
            var layoutSCDTC = $"{TSparse}, TComponent, {TDenseIndex}, {TTickData}, TComponent";
            var layoutSCDT = $"{TSparse}, TComponent, {TDenseIndex}, {TTickData}";
            var layoutSCT = $"{TSparse}, TComponent, {TTickData}";
            var layoutSCD = $"{TSparse}, TComponent, {TDenseIndex}";
            var layoutCTC = $"TComponent, {TTickData}, TComponent";
            var layoutC = $"TComponent";
            var layoutS = $"{TSparse}";

            var extraSpace = new string('\t', 4);

            var nothing = $"{typeof(Nothing<,,,,>).GetNameWithoutGeneric()}<{layoutSCDTC}>";
            var singleFeature = $"{typeof(SingleFeature<,,,>).GetNameWithoutGeneric()}<{layoutSCDT}>";


            var result = new StringBuilder();
            result.Append("Caller<");
            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            result.Append(layoutSCDTC);
            result.Append(", ");

            if (option.isSingle)
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
                result.Append($"{typeof(InjectFeature<,,,>).GetNameWithoutGeneric()}<{layoutSCDT}>");
            }
            else
            {
                result.Append(nothing);
            }
            result.Append(",");

            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            if (option.isUseRecycle)
            {
                result.Append($"{typeof(RecycleStorageFeature<,,,,>).GetNameWithoutGeneric()}<{layoutSCDTC}>");
            }
            else if (option.isSingle)
            {
                result.Append($"{typeof(SingleStorageFeature<,,,,>).GetNameWithoutGeneric()}<{layoutSCDTC}>");
            }
            else
            {
                result.Append($"{typeof(IncrementStorageFeature<,,,,>).GetNameWithoutGeneric()}<{layoutSCDTC}>");
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
                result.Append(nothing);
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
                result.Append(nothing);
            }
            result.Append(",");

            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            if (option.isAttach)
            {
                result.Append($"{typeof(AttachFeature<,,,>).GetNameWithoutGeneric()}<{layoutSCDT}>");
            }
            else
            {
                result.Append(nothing);
            }
            result.Append(",");

            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            if (option.isDetach)
            {
                result.Append($"{typeof(DetachFeature<,,,>).GetNameWithoutGeneric()}<{layoutSCDT}>");
            }
            else
            {
                result.Append(nothing);
            }
            result.Append(",");

            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            if (option.isSingle || option.isEmpty)
            {
                result.Append($"{typeof(SingleSparseFeature<,,>).GetNameWithoutGeneric()}<{layoutCTC}>");
            }
            else
            { 
                if (option.sparseMode == TypeOptions.SparseMode.Bool)
                {
                    result.Append($"{typeof(BoolSparseFeature<,,>).GetNameWithoutGeneric()}<{layoutCTC}>");
                }
                else if (option.sparseMode == TypeOptions.SparseMode.Ushort)
                {
                    result.Append($"{typeof(UshortSparseFeature<,,>).GetNameWithoutGeneric()}<{layoutCTC}>");
                }
            }
            result.Append(",");

            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            if (option.isEmpty)
            {
                result.Append($"{typeof(EmptyFeature<,,,>).GetNameWithoutGeneric()}<{layoutSCDT}>");
            }
            else
            {
                if (option.isSingle)
                {
                    result.Append(singleFeature);
                }
                else
                {
                    result.Append($"{typeof(UshortDenseFeature<,,>).GetNameWithoutGeneric()}<{layoutSCT}>");
                }
            }
            result.Append(",");

            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            if (option.isBindToEntity)
            {
                result.Append(nameof(TrueConst));
            }
            else
            {
                result.Append(nothing);
            }
            result.Append(",");

            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            if (option.isCopyable)
            {
                result.Append($"{typeof(CopyableFeature<>).GetNameWithoutGeneric()}<{layoutC}>");
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
                    result.Append($"{typeof(UintVersionFeature<,,>).GetNameWithoutGeneric()}<{layoutSCT}>");
                }
                else
                {
                    result.Append($"{typeof(UshortVersionFeature<,,>).GetNameWithoutGeneric()}<{layoutSCT}>");
                }
            }
            else
            {
                result.Append(nothing);
            }
            result.Append(",");

            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            if (option.isHistory)
            {
                if (option.isHistoryByChange)
                {
                    result.Append($"{typeof(ByChangeHistoryFeature<,,>).GetNameWithoutGeneric()}<{layoutSCD}>");
                }
                else if (option.isHistoryByTick)
                {
                    result.Append($"{typeof(ByTickHistoryFeature<,,>).GetNameWithoutGeneric()}<{layoutSCD}>");
                }
                else if (option.isHistoryByVersion)
                {
                    result.Append($"{typeof(ByVersionHistoryFeature<,,>).GetNameWithoutGeneric()}<{layoutSCD}>");
                }
            }
            else
            {
                result.Append(nothing);
            }
            result.Append(",");

            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            if (option.isUseISerialize || !option.isBlittable)
            {
                result.Append($"{typeof(SSSerialize<,,,>).GetNameWithoutGeneric()}<{layoutSCDT}>");
            }
            else
            {
                if (option.isHistoryByTick)
                {
                    result.Append($"{typeof(BSSerialize<,,,>).GetNameWithoutGeneric()}<{layoutSCDT}>");
                }
                else
                {
                    result.Append($"{typeof(BBSerialize<,,,>).GetNameWithoutGeneric()}<{layoutSCDT}>");
                }
            }
            result.Append(",");

            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            result.Append(nothing);
            result.Append(Environment.NewLine);
            result.Append(extraSpace);
            result.Append(">");

            return result;

            /* temple example:
             * "Caller<
             * ushort, TComponent, ushort, TickOffsetData<TComponent>, TComponent,
             * UshortNumber,
             * InjectFeature<ushort, TComponent, ushort, TickOffsetData<TComponent>>,
             * RecycleStorageFeature<ushort, TComponent, ushort, TickOffsetData<TComponent>, TComponent>,
             * DefaultFeature<TComponent>,
             * AttachDetachFeature<ushort>,
             * AttachFeature<ushort, TComponent, ushort, TickOffsetData<TComponent>>,
             * DetachFeature<ushort, TComponent, ushort, TickOffsetData<TComponent>>,
             * UshortSparseFeature<TComponent, TickOffsetData<TComponent>, TComponent>,
             * UshortDenseFeature<ushort, TComponent, TickOffsetData<TComponent>>,
             * TrueConst,
             * CopyableFeature<TComponent>,
             * UshortVersionFeature<ushort, TComponent, TickOffsetData<TComponent>>,
             * ByChangeHistoryFeature<ushort, TComponent, ushort>,
             * BBSerialize<ushort, TComponent, ushort, TickOffsetData<TComponent>>,
             * Nothing<ushort, TComponent, ushort, TickOffsetData<TComponent>, TComponent>
             * >";
            */
        }

        public static (string TSparse, string TDenseIndex, string TTickData) GetLayoutDeclaration(in TypeOptions option)
        {
            string TSparse = string.Empty;
            string TDenseIndex = string.Empty;
            string TTickData = string.Empty;

            switch (option.sparseMode)
            {
                case TypeOptions.SparseMode.Bool:
                    {
                        TSparse = "bool";
                        if (option.isSingle)
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
                if (option.isHistoryByChange)
                {
                    TTickData = $"{typeof(TOData<>).GetNameWithoutGeneric()}<TComponent>";
                }
                else if (option.isHistoryByTick)
                {
                    TTickData = $"{typeof(TData<>).GetNameWithoutGeneric()}<ArrayPtr<TComponent>>";
                }
                else if (option.isHistoryByVersion)
                {
                    TTickData = $"{typeof(TIOData<>).GetNameWithoutGeneric()}<TComponent>";
                }
            }
            else
            {
                TTickData = nameof(Nothing);
            }

            return (TSparse, TDenseIndex, TTickData);
        }
    }

}
