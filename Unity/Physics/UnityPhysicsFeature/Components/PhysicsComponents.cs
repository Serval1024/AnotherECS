﻿using AnotherECS.Essentials.Physics;
using AnotherECS.Mathematics;
using AnotherECS.Core;
using AnotherECS.Core.Collection;
using System.Runtime.CompilerServices;

namespace AnotherECS.Physics
{
    public struct PhysicData : ISingle
    {
        public float3 gravity;
        public sfloat deltaTime; 
    }

    public struct PhysicsOneShotConfig : IConfig
    {
        public CollisionEvents collisionEvents;
        public TriggerEvents triggerEvents;
    }

    [CompileComponentOption(ComponentOptions.ForceUseSparse)]
    public struct Position : IComponent, IVersion
    {
        public float3 value;
    }

    [CompileComponentOption(ComponentOptions.ForceUseSparse)]
    public struct Rotation : IComponent, IVersion
    {
        public quaternion value;
    }

    public struct IsPhysicsStatic : IComponent { }

    [CompileComponentOption(ComponentOptions.ForceUseSparse)]
    public struct PhysicsCustomTags : IComponent
    {
        public byte value;
    }

    [CompileComponentOption(ComponentOptions.ForceUseSparse)]
    public unsafe struct PhysicsCollider : IComponent, IInject<WPtr<AllocatorSelector>>, IRepairMemoryHandle
    {
        private static readonly BlobAssetReference<Collider> _empty = BlobAssetReference<Collider>.Create(default(BlobAssetReferenceData));

        public BlobAssetReference<Collider> Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.IsValid ? BlobAssetReference<Collider>.Create(*(BlobAssetReferenceData*)_data.GetPtr()) : _empty;
            
            set
            {
                if (value.IsValid)
                {
                    _data.Allocate((uint)value.m_data.GetByteLength());
                    value.m_data.CopyToMemory(_data.ReadPtr(), _data.Length);
                }
                else
                {
                    _data.Dispose();
                }
            }
        }

        private NArray<AllocatorSelector, byte> _data;

        public bool IsValid
            => Value.IsCreated;

        public unsafe Collider* ColliderPtr
            => (Collider*)Value.GetUnsafePtr();

        public MassProperties MassProperties
            => Value.IsCreated ? Value.Value.MassProperties : MassProperties.UnitSphere;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IInject<WPtr<AllocatorSelector>>.Construct(
            [InjectMap(nameof(BAllocator), "allocatorType=1")]
            [InjectMap(nameof(HAllocator), "allocatorType=2")]
            WPtr<AllocatorSelector> allocator)
        {
            _data.SetAllocator(allocator.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IInject.Deconstruct()
        {
            _data.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRepairMemoryHandle.RepairMemoryHandle(ref RepairMemoryContext repairMemoryContext)
        {
            RepairMemoryCaller.Repair(ref _data, ref repairMemoryContext);
        }
    }

    [CompileComponentOption(ComponentOptions.ForceUseSparse)]
    public struct PhysicsVelocity : IComponent
    {
        public float3 linear;
        public float3 angular;
    }

    [CompileComponentOption(ComponentOptions.ForceUseSparse)]
    public struct PhysicsDamping : IComponent
    {
        public sfloat linear;
        public sfloat angular;
    }

    [CompileComponentOption(ComponentOptions.ForceUseSparse)]
    public struct PhysicsMassOverride : IComponent
    {    
        public byte isKinematic;   
    }

    [CompileComponentOption(ComponentOptions.ForceUseSparse)]
    public struct PhysicsMass : IComponent
    {
        public RigidTransform transform;
        public sfloat inverseMass;
        public float3 inverseInertia;
        public sfloat angularExpansionFactor;
        
        public float3 CenterOfMass { get => transform.pos; set => transform.pos = value; }
        public quaternion InertiaOrientation { get => transform.rot; set => transform.rot = value; }

        public static PhysicsMass CreateDynamic(MassProperties massProperties, sfloat mass)
        {
            //SafetyChecks.CheckFiniteAndPositiveAndThrow(mass, nameof(mass));

            return new PhysicsMass
            {
                transform = massProperties.MassDistribution.Transform,
                inverseMass = math.rcp(mass),
                inverseInertia = math.rcp(massProperties.MassDistribution.InertiaTensor * mass),
                angularExpansionFactor = massProperties.AngularExpansionFactor
            };
        }

        public static PhysicsMass CreateKinematic(MassProperties massProperties)
        {
            return new PhysicsMass
            {
                transform = massProperties.MassDistribution.Transform,
                inverseMass = 0,
                inverseInertia = float3.zero,
                angularExpansionFactor = massProperties.AngularExpansionFactor
            };
        }

    }

    [CompileComponentOption(ComponentOptions.ForceUseSparse)]
    public struct PhysicsGravityFactor : IComponent
    {
        public sfloat value;
    }

    public struct PhysicsInternal : ISingle
    {
        public int prevStaticCount;
    }
}