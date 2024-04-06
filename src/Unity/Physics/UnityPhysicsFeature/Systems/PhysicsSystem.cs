using AnotherECS.Core;
using AnotherECS.Essentials.Physics;
using AnotherECS.Essentials.Physics.Components;
using AnotherECS.Mathematics;
using AnotherECS.Unity.Jobs;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Profiling;
using EntityId = System.Int32;

namespace AnotherECS.Physics
{
    using Bag = JobBagR<
        PhysicsCollider,
        PhysicsVelocity,
        PhysicsCustomTags,
        Position,
        Rotation>;
    using BagApply = JobBag<
        PhysicsVelocity,
        Position,
        Rotation>;
    using BagJoints = JobBagR<
        PhysicsJoint,
        PhysicsConstrainedBodyPair>;
    using BagMotion = JobBagR<
        PhysicsMass,
        PhysicsGravityFactor,
        PhysicsDamping,
        PhysicsMassOverride>;

    [SystemOrder(SystemOrder.First)]
    public sealed class PhysicsSystem : ISystem, ICreateSystem, ITickSystem, IMainThread, IDisposable
    {
        private SimulationContext simulationContext;
        private IdFilter<Position, Rotation, IsPhysicsStatic> staticBodies;
        private IdFilter<Position, Rotation, PhysicsVelocity> dynamicBodies;
        private IdFilter<PhysicsJoint, PhysicsConstrainedBodyPair> joints;

        private readonly DispatchPairSequencer scheduler = new();

        private PhysicsWorld physicsWorldInternal;
        private ref PhysicsWorld PhysicsWorld => ref physicsWorldInternal;

        private State _state;

     
        public void OnCreate(State state)
        {
            _state = state;
            _state.SetOrAdd(new PhysicData() { gravity = new float3(sfloat.zero, -9.8f, sfloat.zero), deltaTime = 1f / 20f });
            _state.SetOrAdd(new PhysicsInternal());

            physicsWorldInternal = new PhysicsWorld(0, 0, 0);

            staticBodies = state.Filter()
                .With<Position>()
                .With<Rotation>()
                .With<IsPhysicsStatic>()
                .BuildAsId();

            dynamicBodies = state.Filter()
                .With<Position>()
                .With<Rotation>()
                .With<PhysicsVelocity>()
                .Without<IsPhysicsStatic>()
                .BuildAsId();

            joints = state.Filter()
                .With<PhysicsJoint>()
                .With<PhysicsConstrainedBodyPair>()
                .BuildAsId();
        }

        public void Dispose()
        {
            simulationContext.Dispose();
            scheduler.Dispose();
            physicsWorldInternal.Dispose();
        }
        
        [BurstCompile]
        internal struct CreateJoints : IJobParallelForBag<BagJoints>
        {
            [ReadOnly] public int NumDynamicBodies;
            [ReadOnly] public NativeParallelHashMap<EntityId, int> EntityBodyIndexMap;

            [NativeDisableParallelForRestriction] public NativeArray<Joint> Joints;
            [NativeDisableParallelForRestriction] public NativeParallelHashMap<EntityId, int>.ParallelWriter EntityJointIndexMap;

            public int DefaultStaticBodyIndex;

            public void Execute(ref BagJoints bag, int index) {

                var joint = bag.ReadT0(index);
                var bodyPair = bag.ReadT1(index);
                var entity = bag.GetEntity(index);
                
                var entityA = bodyPair.EntityA;
                var entityB = bodyPair.EntityB;
                
                // TODO find a reasonable way to look up the constraint body indices
                // - stash body index in a component on the entity? But we don't have random access to Entity data in a job
                // - make a map from entity to rigid body index? Sounds bad and I don't think there is any NativeArray-based map data structure yet

                // If one of the entities is null, use the default static entity
                var pair = new BodyIndexPair
                {
                    BodyIndexA = entityA == EntityConst.Zero ? DefaultStaticBodyIndex : -1,
                    BodyIndexB = entityB == EntityConst.Zero ? DefaultStaticBodyIndex : -1,
                };

                // Find the body indices
                pair.BodyIndexA = EntityBodyIndexMap.TryGetValue(entityA, out var idxA) ? idxA : -1;
                pair.BodyIndexB = EntityBodyIndexMap.TryGetValue(entityB, out var idxB) ? idxB : -1;

                bool isInvalid = false;
                // Invalid if we have not found the body indices...
                isInvalid |= (pair.BodyIndexA == -1 || pair.BodyIndexB == -1);
                // ... or if we are constraining two static bodies
                // Mark static-static invalid since they are not going to affect simulation in any way.
                isInvalid |= (pair.BodyIndexA >= NumDynamicBodies && pair.BodyIndexB >= NumDynamicBodies);
                if (isInvalid)
                {
                    pair = BodyIndexPair.Invalid;
                }

                Joints[index] = new Joint
                {
                    BodyPair = pair,
                    Entity = entity,
                    EnableCollision = (byte)bodyPair.EnableCollision,
                    AFromJoint = joint.BodyAFromJoint.AsMTransform(),
                    BFromJoint = joint.BodyBFromJoint.AsMTransform(),
                    Version = joint.Version,
                    Constraints = joint.GetConstraints(),
                };
                EntityJointIndexMap.TryAdd(entity, index);

            }
        }

        [BurstCompile]
        private struct FillBodiesJob : IJobParallelForBag<Bag>
        {
            [ReadOnly] public int FirstBodyIndex;
            [NativeDisableContainerSafetyRestriction] public NativeArray<RigidBody> bodies;
            [NativeDisableContainerSafetyRestriction] public NativeParallelHashMap<EntityId, int>.ParallelWriter EntityBodyIndexMap;

            public void Execute(ref Bag bag, int index)
            {
                
                int rbIndex = FirstBodyIndex + index;
                var collider = bag.ReadT0(index);
                var hasChunkPhysicsColliderType = bag.HasT0(index);
                var hasChunkPhysicsCustomTagsType = bag.HasT2(index);
                var entity = bag.GetEntity(index);

                var rotation = bag.ReadT4(index).value;
                var position = bag.ReadT3(index).value;

                var worldFromBody = new RigidTransform(rotation, position);
                
                this.bodies[rbIndex] = new RigidBody
                {
                    WorldFromBody = new RigidTransform(worldFromBody.rot, worldFromBody.pos),
                    Collider = hasChunkPhysicsColliderType ? collider.Value : default,
                    Entity = entity,
                    CustomTags = hasChunkPhysicsCustomTagsType ? bag.ReadT2(index).value : (byte)0
                };
                EntityBodyIndexMap.TryAdd(entity, rbIndex);   
            }
        }

        [BurstCompile]
        private struct FillDefaultBodiesJob : IJob
        {    
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<RigidBody> NativeBodies;
            public int BodyIndex;

            [NativeDisableContainerSafetyRestriction]
            public NativeParallelHashMap<EntityId, int>.ParallelWriter EntityBodyIndexMap;

            public void Execute()
            {
                NativeBodies[BodyIndex] = new RigidBody
                {
                    WorldFromBody = new RigidTransform(quaternion.identity, float3.zero),
                    Collider = default,
                    Entity = EntityConst.Zero,
                    CustomTags = 0
                };
                EntityBodyIndexMap.TryAdd(EntityConst.Zero, BodyIndex);
            }
        }

        [BurstCompile]
        private struct FillMotionJob : IJobParallelForBag<Bag>
        {
            public NativeArray<MotionData> motionDatas;
            public NativeArray<MotionVelocity> motionVelocities;
            public BagMotion bagMotion;
            public PhysicsMass defaultPhysicsMass;

            public void Execute(ref Bag bag, int index) 
            {
                var pos = bag.ReadT3(index).value;
                var rot = bag.ReadT4(index).value;
                
                var chunkVelocity = bag.ReadT1(index);
                var chunkMass = this.bagMotion.ReadT0(index);
                var chunkGravityFactor = this.bagMotion.ReadT1(index);
                var chunkDamping = this.bagMotion.ReadT2(index);
                var chunkMassOverride = this.bagMotion.ReadT3(index);
                var hasChunkPhysicsMassType = this.bagMotion.HasT0(index);
                var hasChunkPhysicsGravityFactorType = this.bagMotion.HasT1(index);
                var hasChunkPhysicsDampingType = this.bagMotion.HasT2(index);
                var hasChunkPhysicsMassOverrideType = this.bagMotion.HasT3(index);
                
                // Note: if a dynamic body infinite mass then assume no gravity should be applied
                sfloat defaultGravityFactor = hasChunkPhysicsMassType ? 1 : 0;

                var isKinematic = !hasChunkPhysicsMassType || hasChunkPhysicsMassOverrideType && chunkMassOverride.isKinematic != 0;
                this.motionVelocities[index] = new MotionVelocity {
                    LinearVelocity = chunkVelocity.linear,
                    AngularVelocity = chunkVelocity.angular,
                    InverseInertia = isKinematic ? this.defaultPhysicsMass.inverseInertia : chunkMass.inverseInertia,
                    InverseMass = isKinematic ? this.defaultPhysicsMass.inverseMass : chunkMass.inverseMass,
                    AngularExpansionFactor = hasChunkPhysicsMassType ? chunkMass.angularExpansionFactor : this.defaultPhysicsMass.angularExpansionFactor,
                    GravityFactor = isKinematic ? 0 : hasChunkPhysicsGravityFactorType ? chunkGravityFactor.value : defaultGravityFactor,
                };

                // Note: these defaults assume a dynamic body with infinite mass, hence no damping
                var defaultPhysicsDamping = new PhysicsDamping
                {
                    linear = 0,
                    angular = 0,
                };

                // Create motion datas
                PhysicsMass mass = hasChunkPhysicsMassType ? chunkMass : this.defaultPhysicsMass;
                PhysicsDamping damping = hasChunkPhysicsDampingType ? chunkDamping : defaultPhysicsDamping;

                var a = math.mul(rot, mass.InertiaOrientation);
                var b = math.rotate(rot, mass.CenterOfMass) + pos;
                motionDatas[index] = new MotionData
                {
                    WorldFromMotion = new RigidTransform(a, b),
                    BodyFromMotion = new RigidTransform(mass.InertiaOrientation, mass.CenterOfMass),
                    LinearDamping = damping.linear,
                    AngularDamping = damping.angular,
                };

            }
        }

        [BurstCompile]
        private struct ApplyPhysicsJob : IJobParallelForBag<BagApply>
        {
            public NativeArray<RigidBody> bodies;
            public NativeArray<MotionVelocity> motionVelocities;
            
            public void Execute(ref BagApply bag, int index)
            {
                ref var vel = ref bag.GetT0(index);
                vel.angular = motionVelocities[index].AngularVelocity;
                vel.linear = motionVelocities[index].LinearVelocity;

                ref var pos = ref bag.GetT1(index);
                pos.value = bodies[index].WorldFromBody.pos;

                ref var rot = ref bag.GetT2(index);
                rot.value = bodies[index].WorldFromBody.rot;
            }
        }
        
        [BurstCompile]
        private struct CheckStaticBodyChangesJob : IJobParallelForBag<Bag>
        {
            public uint currentTick;
            public uint prevTick;
            [NativeDisableParallelForRestriction]
            public NativeArray<int> result;
            
            public void Execute(ref Bag bag, int index)
            {    
                bool didBatchChange = bag.ReadVersionT3(index) != currentTick ||
                                      bag.ReadVersionT3(index) != prevTick ||
                                      bag.ReadVersionT4(index) != currentTick ||
                                      bag.ReadVersionT4(index) != prevTick;

                if (didBatchChange)
                {
                    // Note that multiple worker threads may be running at the same time.
                    // They either write 1 to Result[0] or not write at all.  In case multiple
                    // threads are writing 1 to this variable, in C#, reads or writes of int
                    // data type are atomic, which guarantees that Result[0] is 1.
                    result[0] = 1;
                }
            }
        }

        public void OnTick(State state)
        {
            var marker = new ProfilerMarker("[Physics] Reset World");
            marker.Begin();

            var dynamicBodiesEntities = dynamicBodies.GetEntities();
            var staticBodiesEntities = staticBodies.GetEntities();
            var jointsEntities = joints.GetEntities();

            PhysicsWorld.Reset((int)staticBodiesEntities.entities.Length + 1 , (int)dynamicBodiesEntities.entities.Length, (int)jointsEntities.entities.Length);

            marker.End();

            if (this.PhysicsWorld.Bodies.Length == 0) {
                
                return;
                
            }

            ref readonly var config = ref _state.Read<PhysicData>();

            var simulationParameters = new SimulationStepInput()
            {
                Gravity = config.gravity,
                NumSolverIterations = 4,
                SolverStabilizationHeuristicSettings = Solver.StabilizationHeuristicSettings.Default,
                SynchronizeCollisionWorld = true,
                TimeStep = config.deltaTime,
                World = PhysicsWorld,
            };

            ref var internalData = ref _state.Get<PhysicsInternal>();
            var tick = _state.Tick;

            var bag = FilterExtensions.BagJobFactory.CreateR<PhysicsCollider, PhysicsVelocity, PhysicsCustomTags, Position, Rotation>(_state, dynamicBodiesEntities);
            var bagStatic = FilterExtensions.BagJobFactory.CreateR<PhysicsCollider, PhysicsVelocity, PhysicsCustomTags, Position, Rotation>(_state, staticBodiesEntities);
            var bagJoints = FilterExtensions.BagJobFactory.CreateR<PhysicsJoint, PhysicsConstrainedBodyPair>(_state, jointsEntities);
            var bagMotion = FilterExtensions.BagJobFactory.CreateR<PhysicsMass, PhysicsGravityFactor, PhysicsDamping, PhysicsMassOverride>(_state, dynamicBodiesEntities);
            var bagApply = FilterExtensions.BagJobFactory.Create<PhysicsVelocity, Position, Rotation>(_state, dynamicBodiesEntities);

            var buildStaticTree = new NativeArray<int>(1, Allocator.TempJob);
            {
                marker = new ProfilerMarker("[Physics] Build World");
                marker.Begin();

                JobHandle staticBodiesCheckHandle = default;
                buildStaticTree[0] = 0;
                {

                    // Check static has changed
                    if (internalData.prevStaticCount != bagStatic.Count)
                    {
                        buildStaticTree[0] = 1;
                    }
                    else
                    {
                        
                        staticBodiesCheckHandle = new CheckStaticBodyChangesJob
                        {
                            result = buildStaticTree,
                            currentTick = tick,
                            prevTick = tick - 1,
                        }.Schedule(bagStatic);
                    }

                    internalData.prevStaticCount = bagStatic.Count;
                }

                var motionDatas = this.PhysicsWorld.MotionDatas;
                var motionVelocities = this.PhysicsWorld.MotionVelocities;

                JobHandle outputDependency;
                using (var jobHandles = new NativeList<JobHandle>(4, Allocator.Temp))
                {
                    jobHandles.Add(staticBodiesCheckHandle);

                    jobHandles.Add(new FillDefaultBodiesJob
                    {
                        NativeBodies = this.PhysicsWorld.Bodies,
                        BodyIndex = this.PhysicsWorld.Bodies.Length - 1,
                        EntityBodyIndexMap = this.PhysicsWorld.CollisionWorld.EntityBodyIndexMap.AsParallelWriter(),
                    }.Schedule());


                    if (bag.Count > 0)
                    {
                        jobHandles.Add(new FillBodiesJob()
                        {
                            bodies = this.PhysicsWorld.Bodies,
                            FirstBodyIndex = 0,
                            EntityBodyIndexMap = PhysicsWorld.CollisionWorld.EntityBodyIndexMap.AsParallelWriter(),
                        }.Schedule(bag));
                        
                        jobHandles.Add(new FillMotionJob()
                        {
                            bagMotion = bagMotion,
                            defaultPhysicsMass = PhysicsMass.CreateDynamic(MassProperties.UnitSphere, 1f),
                            motionDatas = motionDatas,
                            motionVelocities = motionVelocities,
                        }.Schedule(bag));
                        
                    }


                    if (bagStatic.Count > 0)
                    {
                        jobHandles.Add(new FillBodiesJob()
                        {
                            bodies = this.PhysicsWorld.Bodies,
                            FirstBodyIndex = bag.Count,
                            EntityBodyIndexMap = this.PhysicsWorld.CollisionWorld.EntityBodyIndexMap.AsParallelWriter(),
                        }.Schedule(bagStatic));
                    }

                    var handle = JobHandle.CombineDependencies(jobHandles.AsArray());
                    jobHandles.Clear();

                    if (bagJoints.Count > 0)
                    {
                        jobHandles.Add(new CreateJoints
                        {
                            Joints = this.PhysicsWorld.Joints,
                            DefaultStaticBodyIndex = this.PhysicsWorld.Bodies.Length - 1,
                            NumDynamicBodies = bag.Count,
                            EntityBodyIndexMap = this.PhysicsWorld.CollisionWorld.EntityBodyIndexMap,
                            EntityJointIndexMap = this.PhysicsWorld.DynamicsWorld.EntityJointIndexMap.AsParallelWriter(),
                        }.Schedule(bagJoints, handle));
                    }

                    var buildBroadphaseHandle = this.PhysicsWorld.CollisionWorld.ScheduleBuildBroadphaseJobs(
                        ref this.PhysicsWorld,
                        simulationParameters.TimeStep,
                        simulationParameters.Gravity,
                        buildStaticTree,
                        handle,
                        true);
                    jobHandles.Add(buildBroadphaseHandle);

                    outputDependency = JobHandle.CombineDependencies(jobHandles.AsArray());
                    
                }
                
                marker.End();

                {
                    marker = new ProfilerMarker("[Physics] Step World");
                    marker.Begin();


                    var jobs = Simulation.ScheduleStepJobs(ref simulationParameters,
                                                           ref simulationContext,
                                                           scheduler,
                                                           outputDependency,
                                                           true);

                    
                    {
                        jobs.FinalExecutionHandle.Complete();
                    }

                    marker.End();

                    marker = new ProfilerMarker("[Physics] Apply World");
                    marker.Begin();

                    _state.SetOrAddConfig(new PhysicsOneShotConfig()
                    {
                        dataTick = tick,
                        collisionEvents = simulationContext.CollisionEvents,
                        triggerEvents = simulationContext.TriggerEvents,
                    });
                    
                    {
                        // Sync physics result with entities

                        var bodies = PhysicsWorld.DynamicBodies;
                        new ApplyPhysicsJob()
                        {
                            bodies = bodies,
                            motionVelocities = motionVelocities,
                        }.Schedule(bagApply).Complete();
                    }

                    marker.End();

                }

            }
            
            marker = new ProfilerMarker("[Physics] Clean up");
            marker.Begin();

            buildStaticTree.Dispose();

            marker.End();

        }
    }
    
}
