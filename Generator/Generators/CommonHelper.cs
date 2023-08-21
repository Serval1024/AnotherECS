namespace AnotherECS.Generator
{
    internal static class CommonHelper
    {
        public static TemplateParser.Variables DefaultVariables(TypeOptions option, GeneratorContext.ComponentFilterData componentFilterData = default)
            => new()
            {
                { "GENERIC_CONSTRAINTS", p => TypeOptionsUtils.GetPoolInterfaces(option) },
                { "POOL_TYPE", p => TypeOptionsUtils.GetPoolFlags(option) },
                { "ADAPTER_TYPE", p => TypeOptionsUtils.GetAdapterFlags(option, componentFilterData) },
                { "HISTORY_TYPE", p => TypeOptionsUtils.GetHistoryFlags(option) },

                { "BLITTABLE", p => option.isBlittable.ToString() },
                { "VERSION", p => option.isVersion.ToString() },
                { "HISTORY", p => option.isHistory.ToString() },
                { "ATTACH", p => option.isAttach.ToString() },
                { "DETACH", p => option.isDetach.ToString() },
                { "HISTORY:BYCHANGE", p => option.isHistoryByChange.ToString() },
                { "HISTORY:BYTICK", p => option.isHistoryByTick.ToString() },
                { "COPYABLE", p => option.isCopyable.ToString() },
                { "SHARED", p => option.isShared.ToString() },
                { "MARKER", p => option.isMarker.ToString() },
                { "DISPOSE", p => option.isDispose.ToString() },
                { "SPARSE:BYTE", p => option.isLimit255.ToString() },
                { "SPARSE:BOOL", p => (option.isEmpty || option.isExceptSparseDirectDense).ToString() },
                { "SPARSE:TYPE_NAME", p => (option.isEmpty || option.isExceptSparseDirectDense) ? "bool" : (option.isLimit255 ? "byte" : "ushort") },
                { "POOLDATA_INDEX:TYPE_NAME", p => option.isLimit255 ? "byte" : "ushort" },
                { "POOLDATA_INDEX:TYPE_UNPACK_NAME", p => option.isLimit255 ? "Byte" : "UInt16" },
                { "EMPTY", p => option.isEmpty.ToString() },
                { "DIRECTACCESS", p => option.isCompileDirectAccess.ToString() },
                { "POOLCAPACITY", p => option.isOverrideCapacity ? option.capacity.ToString() : "componentCapacity" },
                { "INJECT", p => option.isInject.ToString() },
                { "FORCE:ISerialize", p => option.isForceUseISerialize.ToString() },
                { "REFERNCE_POOL", p => option.isReferencePool.ToString() },
            };
    }
}
