using AnotherECS.Core;
using AnotherECS.Serializer;
using EntityId = System.UInt32;

namespace AnotherECS.Views.Core
{
    [ComponentOption(ComponentOptions.HistoryNonSync | ComponentOptions.NoCompileFastAccess | ComponentOptions.CompileSortAtLast)]
    public struct ViewSystemReference : IShared
    {
        public IViewSystem system;
    }

    [ComponentOption(ComponentOptions.NoCompileFastAccess | ComponentOptions.CompileSortAtLast)]
    public struct ViewHandle : IComponent, IAttach, IDetach, ISerialize
    {
        internal EntityId ownerId;
        internal uint viewId;

        public void OnAttach(State state)
        { 
            //state.Get<ViewSystemReference>().system.Create(state, ownerId, viewId);
        }

        public void OnDetach(State state)
        {
            //state.Get<ViewSystemReference>().system.Destroy(ownerId);
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(ownerId);
            writer.Write(viewId);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            ownerId = reader.ReadUInt32();
            viewId = reader.ReadUInt32();
        }
    }
}
