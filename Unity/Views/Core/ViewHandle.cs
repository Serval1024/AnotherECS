using AnotherECS.Core;
using AnotherECS.Serializer;
using EntityId = System.UInt32;

namespace AnotherECS.Views.Core
{
    public struct ViewSystemReference : IConfig
    {
        internal IViewSystem system;
    }

    public struct ViewHandle : IComponent, IAttach, IDetach, ISerialize
    {
        internal EntityId ownerId;
        internal uint viewId;

        public void OnAttach(State state)
        {
            state.GetConfig<ViewSystemReference>().system.Create(state, ownerId, viewId);
        }

        public void OnDetach(State state)
        {
            state.GetConfig<ViewSystemReference>().system.Destroy(ownerId);
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
