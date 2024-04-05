// <auto-generated>
// This source code was auto-generated by LayoutInstallerGenerator.cs
// </auto-generated>

using AnotherECS.Core;
using AnotherECS.Core.Allocators;
using AnotherECS.Gen.Common;
using System.Runtime.CompilerServices;

namespace AnotherECS.Gen.Project
{
    public static class LayoutInstaller
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Install_Health(State state)
        {
            CommonLayoutInstaller.Install_BL2NoBER<Health>(state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Install_AnotherECS_Random_RandomSingle(State state)
        {
            CommonLayoutInstaller.Install_BSL0<AnotherECS.Random.RandomSingle>(state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Install_AnotherECS_Views_Core_ViewHandle(State state)
        {
            CommonLayoutInstaller.Install_BADL2NoBER<AnotherECS.Views.Core.ViewHandle>(state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Install_AnotherECS_Physics_PhysicData(State state)
        {
            CommonLayoutInstaller.Install_BSL0<AnotherECS.Physics.PhysicData>(state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Install_AnotherECS_Physics_Position(State state)
        {
            CommonLayoutInstaller.Install_BVL2NoBER<AnotherECS.Physics.Position>(state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Install_AnotherECS_Physics_Rotation(State state)
        {
            CommonLayoutInstaller.Install_BVL2NoBER<AnotherECS.Physics.Rotation>(state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Install_AnotherECS_Physics_IsPhysicsStatic(State state)
        {
            CommonLayoutInstaller.Install_BL0EmptyNoBE<AnotherECS.Physics.IsPhysicsStatic>(state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Install_AnotherECS_Physics_PhysicsCustomTags(State state)
        {
            CommonLayoutInstaller.Install_BL2NoBER<AnotherECS.Physics.PhysicsCustomTags>(state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Install_AnotherECS_Physics_PhysicsCollider(State state)
        {
            CommonLayoutInstaller.Install_BL2InjRmNoBER<AnotherECS.Physics.PhysicsCollider>(state, new ComponentFunction<AnotherECS.Physics.PhysicsCollider>() { construct = &Construct, deconstruct = &Deconstruct, repairMemory = &RepairMemory, });

            static void Construct(ref InjectContainer injectContainer, ref AnotherECS.Physics.PhysicsCollider component)
            {
#if !ANOTHERECS_RELEASE
                ComponentReflectionUtils.Construct(ref component, ref injectContainer);
#else

                ComponentCompileUtils.Construct(ref component, injectContainer.HAllocator);


#endif
            }

            static void Deconstruct(ref InjectContainer injectContainer, ref AnotherECS.Physics.PhysicsCollider component)
            {
#if !ANOTHERECS_RELEASE
                ComponentReflectionUtils.Deconstruct(ref component, ref injectContainer);
#else

                ComponentCompileUtils.Deconstruct(ref component);


#endif
            }


            static void RepairMemory(ref RepairMemoryContext repairMemoryContext, ref AnotherECS.Physics.PhysicsCollider component)
            {
#if !ANOTHERECS_RELEASE
                ComponentReflectionUtils.RepairMemoryHandle(ref component, ref repairMemoryContext);
#else

                ComponentCompileUtils.RepairMemoryHandle(ref component, ref repairMemoryContext);


#endif
            }


        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Install_AnotherECS_Physics_PhysicsVelocity(State state)
        {
            CommonLayoutInstaller.Install_BL2NoBER<AnotherECS.Physics.PhysicsVelocity>(state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Install_AnotherECS_Physics_PhysicsDamping(State state)
        {
            CommonLayoutInstaller.Install_BL2NoBER<AnotherECS.Physics.PhysicsDamping>(state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Install_AnotherECS_Physics_PhysicsMassOverride(State state)
        {
            CommonLayoutInstaller.Install_BL2NoBER<AnotherECS.Physics.PhysicsMassOverride>(state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Install_AnotherECS_Physics_PhysicsMass(State state)
        {
            CommonLayoutInstaller.Install_BL2NoBER<AnotherECS.Physics.PhysicsMass>(state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Install_AnotherECS_Physics_PhysicsGravityFactor(State state)
        {
            CommonLayoutInstaller.Install_BL2NoBER<AnotherECS.Physics.PhysicsGravityFactor>(state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Install_AnotherECS_Physics_PhysicsInternal(State state)
        {
            CommonLayoutInstaller.Install_BSL0<AnotherECS.Physics.PhysicsInternal>(state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Install_AnotherECS_Essentials_Physics_Components_PhysicsConstrainedBodyPair(State state)
        {
            CommonLayoutInstaller.Install_BL2NoBER<AnotherECS.Essentials.Physics.Components.PhysicsConstrainedBodyPair>(state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Install_AnotherECS_Essentials_Physics_Components_PhysicsJointCompanion(State state)
        {
            CommonLayoutInstaller.Install_BL2NoBER<AnotherECS.Essentials.Physics.Components.PhysicsJointCompanion>(state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Install_AnotherECS_Essentials_Physics_Components_PhysicsJoint(State state)
        {
            CommonLayoutInstaller.Install_BL2NoBER<AnotherECS.Essentials.Physics.Components.PhysicsJoint>(state);
        }


    }
}        
        