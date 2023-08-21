using AnotherECS.Core;
using AnotherECS.Serializer;

namespace AnotherECS.Views
{
    [ComponentOption(ComponentOptions.HistoryNonSync | ComponentOptions.NoCompileDirectAccess | ComponentOptions.CompileSortAtLast)]
    public struct ViewSystemReference : IShared
    {
        public IViewSystem system;
    }

    [ComponentOption(ComponentOptions.NoCompileDirectAccess | ComponentOptions.CompileSortAtLast)]
    public struct ViewHandle : IComponent, IAttach, IDetach, ISerialize
    {
        internal int ownerId;
        internal uint viewId;

        public void OnAttach(State state)
        { 
            state.Get<ViewSystemReference>().system.Create(state, ownerId, viewId);
        }

        public void OnDetach(State state)
        {
            state.Get<ViewSystemReference>().system.Destroy(ownerId);
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(ownerId);
            writer.Write(viewId);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            ownerId = reader.ReadInt32();
            viewId = reader.ReadUInt32();
        }
    }
}
