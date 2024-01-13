using AnotherECS.Core;
using AnotherECS.Core.Threading;
using AnotherECS.Essentials.Physics;
using AnotherECS.Mathematics;
using EntityId = System.UInt32;

namespace AnotherECS.Physics
{
    public static class PhysicsExtensions
    {
        public static ThreadRestrictionsBuilder UsePhysicsData(ref this ThreadRestrictionsBuilder builder)
            => builder.Use<PhysicsOneShotData>();

        public static PhysicsOneShotData GetPhysicsData(this State state)
        {
#if !ANOTHERECS_RELEASE
            if(!state.IsHasConfig<PhysicsOneShotConfig>())
            {
                throw new System.InvalidOperationException("Physics has not been added to ecs.");
            }
#endif
            return state.GetConfig<PhysicsOneShotConfig>().data;
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
