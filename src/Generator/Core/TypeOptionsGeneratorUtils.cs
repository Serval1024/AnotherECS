using AnotherECS.Core;
using AnotherECS.Core.Caller;
using System;
using System.Text;

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

        public static string GetCallerInterfaces(in TypeOptions option)
        {
            var result = new StringBuilder();
            if (option.isAttachExternal)
            {
                result.Append(", ");
                result.Append(nameof(IAttachExternal));
            }
            if (option.isDetachExternal)
            {
                result.Append(", ");
                result.Append(nameof(IDetachExternal));
            }
            if (option.isSingle)
            {
                result.Append(", ");
                result.Append(nameof(ISingle));
            }
            if (option.isDefault)
            {
                result.Append(", ");
                result.Append(nameof(IDefault));
            }
            if (option.isVersion)
            {
                result.Append(", ");
                result.Append(nameof(IVersion));
            }

            return result.ToString();
        }

        public static StringBuilder DeclarationToString(in GenericDeclaration declaration)
        {
            var result = new StringBuilder();
            DeclarationToString(declaration, 0, true, 0, result);
            return result;
        }
        
        public static StringBuilder GetCallerDeclaration(TypeOptions option)
        {
            option.type = typeof(TComponent);   // Swap to generic TComponent
            return DeclarationToString(CallerDeclaration.GetCallerDeclaration(option));
        }

        public static (string TAllocator, string TSparse, string TDenseIndex) GetLayoutDeclaration(in TypeOptions option)
        {
            var (TAllocator, TSparse, TDenseIndex) = CallerDeclaration.GetLayoutDeclaration(option);
            return (TAllocator.Type.Name, TSparse.Type.Name, TDenseIndex.Type.Name);
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
            if (option.isAttachExternal)
            {
                result.Append("A");
            }
            if (option.isDetachExternal)
            {
                result.Append("D");
            }
            if (option.isSingle)
            {
                result.Append("S");
            }
            if (option.isDefault)
            {
                result.Append("Def");
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
            if (option.isInject)
            {
                result.Append("Inj");
            }
            if (option.isRepairMemory)
            {
                result.Append("Rm");
            }
            if (option.isRepairStateId)
            {
                result.Append("Rs");
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

        private static void DeclarationToString(in GenericDeclaration declaration, int newLineDeep, bool isEnd, int deep, StringBuilder result)
        {
            result.Append(ReflectionUtils.GetNameWithoutGeneric(declaration.Type));
            if (declaration.Generic.Count != 0)
            {
                result.Append('<');
                if (deep <= newLineDeep)
                {
                    result.Append(Environment.NewLine);
                }

                for (int i = 0; i < declaration.Generic.Count; ++i)
                {
                    DeclarationToString(
                        declaration.Generic[i],
                        newLineDeep,
                        i == declaration.Generic.Count - 1,
                        deep + 1,
                        result);

                    if (deep <= newLineDeep)
                    {
                        result.Append(Environment.NewLine);
                    }
                }
                result.Append('>');
            }

            if (!isEnd)
            {
                result.Append(',');
            }
        }

        private struct TComponent { }
    }
}
