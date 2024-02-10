using AnotherECS.Core;
using AnotherECS.Core.Caller;
using AnotherECS.Serializer;
using EntityId = System.UInt32;

namespace AnotherECS.Views.Core
{
    public struct ViewSystemReference : IConfig
    {
        internal IViewSystem system;
    }

    public struct ViewHandle : IComponent, IAttachExternal, IDetachExternal, ISerialize
    {
        internal EntityId ownerId;
        internal uint viewId;

        public void OnAttach(ref ADExternalContext context)
        {
            context.GetConfig<ViewSystemReference>().system.Create(ownerId, viewId);
        }

        public void OnDetach(ref ADExternalContext context)
        {
            context.GetConfig<ViewSystemReference>().system.Destroy(ownerId);
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
