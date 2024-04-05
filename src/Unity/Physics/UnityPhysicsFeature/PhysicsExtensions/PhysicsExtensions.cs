using AnotherECS.Core;
using AnotherECS.Essentials.Physics;
using AnotherECS.Mathematics;
using AnotherECS.Physics.Exceptions;
using EntityId = System.UInt32;

namespace AnotherECS.Physics
{
    public static class PhysicsExtensions
    {
        public static PhysicsOneShotConfig GetPhysicsData(this State state)
        {
#if !ANOTHERECS_RELEASE
            if(!state.IsHasConfig<PhysicsOneShotConfig>())
            {
                throw new System.InvalidOperationException("Physics has not been added to ecs.");
            }
            var dataTick = state.GetConfig<PhysicsOneShotConfig>().dataTick;
            if (state.Tick != dataTick)
            {
                throw new FreshPhysicsDataException(state.Tick, dataTick);
            }
#endif
            return state.GetConfig<PhysicsOneShotConfig>();
        }

        public static void CreatePhysics(this State state, EntityId id, BlobAssetReference<Collider> collider, float3 position, quaternion rotation)
        {
            state.Add<PhysicsVelocity>(id);
            state.Add<PhysicsCollider>(id).Value = collider;
            state.Add(id, new Position() { value = position });
            state.Add(id, new Rotation() { value = rotation });
        }

        public static void CreatePhysics(this State state, EntityId id, BlobAssetReference<Collider> collider, float3 position = default)
        {
            CreatePhysics(state, id, collider, position, quaternion.identity);
        }

        public static void CreatePhysics(this State state, EntityId id, float3 position, quaternion rotation)
        {
            state.Add<PhysicsVelocity>(id);
            state.Add(id, new Position() { value = position });
            state.Add(id, new Rotation() { value = rotation });
        }

        public static void CreatePhysics(this State state, EntityId id, float3 position = default)
        {
            CreatePhysics(state, id, position, quaternion.identity);
        }

        public static void CreatePhysicsStatic(this State state, EntityId id, BlobAssetReference<Collider> collider, float3 position, quaternion rotation)
        {
            state.Add<IsPhysicsStatic>(id);
            state.Add<PhysicsCollider>(id).Value = collider;
            state.Add(id, new Position() { value = position });
            state.Add(id, new Rotation() { value = rotation });
        }

        public static void CreatePhysicsStatic(this State state, EntityId id, BlobAssetReference<Collider> collider, float3 position = default)
        {
            CreatePhysicsStatic(state, id, collider, position, quaternion.identity);
        }
    }
}
