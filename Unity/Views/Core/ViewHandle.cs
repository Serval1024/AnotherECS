using AnotherECS.Core;
using AnotherECS.Core.Caller;
using AnotherECS.Serializer;
using AnotherECS.Unity.Views;
using System.Runtime.CompilerServices;
using EntityId = System.UInt32;

namespace AnotherECS.Views.Core
{
    public struct ViewHandle : IComponent, IAttachExternal, IDetachExternal, ISerialize
    {
        internal EntityId ownerId;
        internal ViewId viewId;

        internal ViewHandle(EntityId ownerId, ViewId viewId)
        {
            this.ownerId = ownerId;
            this.viewId = viewId;
        }

        public void OnAttach(ref ADExternalContext context)
        {
#if !ANOTHERECS_RELEASE
            Validate(context._state);
#endif
            context.GetConfig<ViewSystemReference>().module.Create(ownerId, viewId);
        }

        public void OnDetach(ref ADExternalContext context)
        {
#if !ANOTHERECS_RELEASE
            Validate(context._state);
#endif
            context.GetConfig<ViewSystemReference>().module.Destroy(ownerId);
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(ownerId);
            viewId.Pack(ref writer);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            ownerId = reader.ReadUInt32();
            viewId.Unpack(ref reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Validate(State state)
        {
#if !ANOTHERECS_RELEASE
            if (!state.IsHasConfig<ViewSystemReference>())
            {
                throw new AnotherECS.Core.Exceptions.FeatureNotExists(nameof(UnityViewModule));
            }
#endif
        }
    }
}
