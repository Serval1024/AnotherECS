//GENERATOR = AnotherECS.Generator.CommonGenericGenerator FILENAME = FilterExtensions.cs N = 8

// <auto-generated>
// This source code was auto-generated by CommonGenericGenerator.cs
// </auto-generated>


using AnotherECS.Core;
using Unity.Jobs;
using EntityId = System.UInt32;

namespace AnotherECS.Unity.Jobs
{
    public unsafe static class FilterExtensions
    {
		internal unsafe static class BagJobFactory
		{
			public static JobBag<T0> Create<T0>(Filter<T0> filter)
				where T0 : unmanaged, IComponent

			{
				var state = filter.GetState();
				var filterData = filter.GetFilterData();
				var handles = state.GetModuleData<NativeArrayHandles>(NativeArrayHandles.USER_DATA_ID);

				JobBag<T0> bag;
				bag.indexes = handles.GetNativeArray<T0, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse0 = handles.GetNativeArray<T0, ushort>(1, state.GetSparse<T0, ushort>());
				bag.component0 = handles.GetNativeArray<T0, T0>(2, state.GetDense<T0>());
				bag.version0 = handles.GetNativeArray<T0, uint>(3, state.GetVersion<T0>());

				return bag;
			}
			public static JobBag<T0, T1> Create<T0, T1>(Filter<T0, T1> filter)
				where T0 : unmanaged, IComponent
				where T1 : unmanaged, IComponent

			{
				var state = filter.GetState();
				var filterData = filter.GetFilterData();
				var handles = state.GetModuleData<NativeArrayHandles>(NativeArrayHandles.USER_DATA_ID);

				JobBag<T0, T1> bag;
				bag.indexes = handles.GetNativeArray<T0, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse0 = handles.GetNativeArray<T0, ushort>(1, state.GetSparse<T0, ushort>());
				bag.component0 = handles.GetNativeArray<T0, T0>(2, state.GetDense<T0>());
				bag.version0 = handles.GetNativeArray<T0, uint>(3, state.GetVersion<T0>());
				bag.indexes = handles.GetNativeArray<T1, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse1 = handles.GetNativeArray<T1, ushort>(1, state.GetSparse<T1, ushort>());
				bag.component1 = handles.GetNativeArray<T1, T1>(2, state.GetDense<T1>());
				bag.version1 = handles.GetNativeArray<T1, uint>(3, state.GetVersion<T1>());

				return bag;
			}
			public static JobBag<T0, T1, T2> Create<T0, T1, T2>(Filter<T0, T1, T2> filter)
				where T0 : unmanaged, IComponent
				where T1 : unmanaged, IComponent
				where T2 : unmanaged, IComponent

			{
				var state = filter.GetState();
				var filterData = filter.GetFilterData();
				var handles = state.GetModuleData<NativeArrayHandles>(NativeArrayHandles.USER_DATA_ID);

				JobBag<T0, T1, T2> bag;
				bag.indexes = handles.GetNativeArray<T0, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse0 = handles.GetNativeArray<T0, ushort>(1, state.GetSparse<T0, ushort>());
				bag.component0 = handles.GetNativeArray<T0, T0>(2, state.GetDense<T0>());
				bag.version0 = handles.GetNativeArray<T0, uint>(3, state.GetVersion<T0>());
				bag.indexes = handles.GetNativeArray<T1, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse1 = handles.GetNativeArray<T1, ushort>(1, state.GetSparse<T1, ushort>());
				bag.component1 = handles.GetNativeArray<T1, T1>(2, state.GetDense<T1>());
				bag.version1 = handles.GetNativeArray<T1, uint>(3, state.GetVersion<T1>());
				bag.indexes = handles.GetNativeArray<T2, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse2 = handles.GetNativeArray<T2, ushort>(1, state.GetSparse<T2, ushort>());
				bag.component2 = handles.GetNativeArray<T2, T2>(2, state.GetDense<T2>());
				bag.version2 = handles.GetNativeArray<T2, uint>(3, state.GetVersion<T2>());

				return bag;
			}
			public static JobBag<T0, T1, T2, T3> Create<T0, T1, T2, T3>(Filter<T0, T1, T2, T3> filter)
				where T0 : unmanaged, IComponent
				where T1 : unmanaged, IComponent
				where T2 : unmanaged, IComponent
				where T3 : unmanaged, IComponent

			{
				var state = filter.GetState();
				var filterData = filter.GetFilterData();
				var handles = state.GetModuleData<NativeArrayHandles>(NativeArrayHandles.USER_DATA_ID);

				JobBag<T0, T1, T2, T3> bag;
				bag.indexes = handles.GetNativeArray<T0, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse0 = handles.GetNativeArray<T0, ushort>(1, state.GetSparse<T0, ushort>());
				bag.component0 = handles.GetNativeArray<T0, T0>(2, state.GetDense<T0>());
				bag.version0 = handles.GetNativeArray<T0, uint>(3, state.GetVersion<T0>());
				bag.indexes = handles.GetNativeArray<T1, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse1 = handles.GetNativeArray<T1, ushort>(1, state.GetSparse<T1, ushort>());
				bag.component1 = handles.GetNativeArray<T1, T1>(2, state.GetDense<T1>());
				bag.version1 = handles.GetNativeArray<T1, uint>(3, state.GetVersion<T1>());
				bag.indexes = handles.GetNativeArray<T2, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse2 = handles.GetNativeArray<T2, ushort>(1, state.GetSparse<T2, ushort>());
				bag.component2 = handles.GetNativeArray<T2, T2>(2, state.GetDense<T2>());
				bag.version2 = handles.GetNativeArray<T2, uint>(3, state.GetVersion<T2>());
				bag.indexes = handles.GetNativeArray<T3, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse3 = handles.GetNativeArray<T3, ushort>(1, state.GetSparse<T3, ushort>());
				bag.component3 = handles.GetNativeArray<T3, T3>(2, state.GetDense<T3>());
				bag.version3 = handles.GetNativeArray<T3, uint>(3, state.GetVersion<T3>());

				return bag;
			}
			public static JobBag<T0, T1, T2, T3, T4> Create<T0, T1, T2, T3, T4>(Filter<T0, T1, T2, T3, T4> filter)
				where T0 : unmanaged, IComponent
				where T1 : unmanaged, IComponent
				where T2 : unmanaged, IComponent
				where T3 : unmanaged, IComponent
				where T4 : unmanaged, IComponent

			{
				var state = filter.GetState();
				var filterData = filter.GetFilterData();
				var handles = state.GetModuleData<NativeArrayHandles>(NativeArrayHandles.USER_DATA_ID);

				JobBag<T0, T1, T2, T3, T4> bag;
				bag.indexes = handles.GetNativeArray<T0, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse0 = handles.GetNativeArray<T0, ushort>(1, state.GetSparse<T0, ushort>());
				bag.component0 = handles.GetNativeArray<T0, T0>(2, state.GetDense<T0>());
				bag.version0 = handles.GetNativeArray<T0, uint>(3, state.GetVersion<T0>());
				bag.indexes = handles.GetNativeArray<T1, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse1 = handles.GetNativeArray<T1, ushort>(1, state.GetSparse<T1, ushort>());
				bag.component1 = handles.GetNativeArray<T1, T1>(2, state.GetDense<T1>());
				bag.version1 = handles.GetNativeArray<T1, uint>(3, state.GetVersion<T1>());
				bag.indexes = handles.GetNativeArray<T2, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse2 = handles.GetNativeArray<T2, ushort>(1, state.GetSparse<T2, ushort>());
				bag.component2 = handles.GetNativeArray<T2, T2>(2, state.GetDense<T2>());
				bag.version2 = handles.GetNativeArray<T2, uint>(3, state.GetVersion<T2>());
				bag.indexes = handles.GetNativeArray<T3, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse3 = handles.GetNativeArray<T3, ushort>(1, state.GetSparse<T3, ushort>());
				bag.component3 = handles.GetNativeArray<T3, T3>(2, state.GetDense<T3>());
				bag.version3 = handles.GetNativeArray<T3, uint>(3, state.GetVersion<T3>());
				bag.indexes = handles.GetNativeArray<T4, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse4 = handles.GetNativeArray<T4, ushort>(1, state.GetSparse<T4, ushort>());
				bag.component4 = handles.GetNativeArray<T4, T4>(2, state.GetDense<T4>());
				bag.version4 = handles.GetNativeArray<T4, uint>(3, state.GetVersion<T4>());

				return bag;
			}
			public static JobBag<T0, T1, T2, T3, T4, T5> Create<T0, T1, T2, T3, T4, T5>(Filter<T0, T1, T2, T3, T4, T5> filter)
				where T0 : unmanaged, IComponent
				where T1 : unmanaged, IComponent
				where T2 : unmanaged, IComponent
				where T3 : unmanaged, IComponent
				where T4 : unmanaged, IComponent
				where T5 : unmanaged, IComponent

			{
				var state = filter.GetState();
				var filterData = filter.GetFilterData();
				var handles = state.GetModuleData<NativeArrayHandles>(NativeArrayHandles.USER_DATA_ID);

				JobBag<T0, T1, T2, T3, T4, T5> bag;
				bag.indexes = handles.GetNativeArray<T0, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse0 = handles.GetNativeArray<T0, ushort>(1, state.GetSparse<T0, ushort>());
				bag.component0 = handles.GetNativeArray<T0, T0>(2, state.GetDense<T0>());
				bag.version0 = handles.GetNativeArray<T0, uint>(3, state.GetVersion<T0>());
				bag.indexes = handles.GetNativeArray<T1, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse1 = handles.GetNativeArray<T1, ushort>(1, state.GetSparse<T1, ushort>());
				bag.component1 = handles.GetNativeArray<T1, T1>(2, state.GetDense<T1>());
				bag.version1 = handles.GetNativeArray<T1, uint>(3, state.GetVersion<T1>());
				bag.indexes = handles.GetNativeArray<T2, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse2 = handles.GetNativeArray<T2, ushort>(1, state.GetSparse<T2, ushort>());
				bag.component2 = handles.GetNativeArray<T2, T2>(2, state.GetDense<T2>());
				bag.version2 = handles.GetNativeArray<T2, uint>(3, state.GetVersion<T2>());
				bag.indexes = handles.GetNativeArray<T3, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse3 = handles.GetNativeArray<T3, ushort>(1, state.GetSparse<T3, ushort>());
				bag.component3 = handles.GetNativeArray<T3, T3>(2, state.GetDense<T3>());
				bag.version3 = handles.GetNativeArray<T3, uint>(3, state.GetVersion<T3>());
				bag.indexes = handles.GetNativeArray<T4, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse4 = handles.GetNativeArray<T4, ushort>(1, state.GetSparse<T4, ushort>());
				bag.component4 = handles.GetNativeArray<T4, T4>(2, state.GetDense<T4>());
				bag.version4 = handles.GetNativeArray<T4, uint>(3, state.GetVersion<T4>());
				bag.indexes = handles.GetNativeArray<T5, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse5 = handles.GetNativeArray<T5, ushort>(1, state.GetSparse<T5, ushort>());
				bag.component5 = handles.GetNativeArray<T5, T5>(2, state.GetDense<T5>());
				bag.version5 = handles.GetNativeArray<T5, uint>(3, state.GetVersion<T5>());

				return bag;
			}
			public static JobBag<T0, T1, T2, T3, T4, T5, T6> Create<T0, T1, T2, T3, T4, T5, T6>(Filter<T0, T1, T2, T3, T4, T5, T6> filter)
				where T0 : unmanaged, IComponent
				where T1 : unmanaged, IComponent
				where T2 : unmanaged, IComponent
				where T3 : unmanaged, IComponent
				where T4 : unmanaged, IComponent
				where T5 : unmanaged, IComponent
				where T6 : unmanaged, IComponent

			{
				var state = filter.GetState();
				var filterData = filter.GetFilterData();
				var handles = state.GetModuleData<NativeArrayHandles>(NativeArrayHandles.USER_DATA_ID);

				JobBag<T0, T1, T2, T3, T4, T5, T6> bag;
				bag.indexes = handles.GetNativeArray<T0, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse0 = handles.GetNativeArray<T0, ushort>(1, state.GetSparse<T0, ushort>());
				bag.component0 = handles.GetNativeArray<T0, T0>(2, state.GetDense<T0>());
				bag.version0 = handles.GetNativeArray<T0, uint>(3, state.GetVersion<T0>());
				bag.indexes = handles.GetNativeArray<T1, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse1 = handles.GetNativeArray<T1, ushort>(1, state.GetSparse<T1, ushort>());
				bag.component1 = handles.GetNativeArray<T1, T1>(2, state.GetDense<T1>());
				bag.version1 = handles.GetNativeArray<T1, uint>(3, state.GetVersion<T1>());
				bag.indexes = handles.GetNativeArray<T2, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse2 = handles.GetNativeArray<T2, ushort>(1, state.GetSparse<T2, ushort>());
				bag.component2 = handles.GetNativeArray<T2, T2>(2, state.GetDense<T2>());
				bag.version2 = handles.GetNativeArray<T2, uint>(3, state.GetVersion<T2>());
				bag.indexes = handles.GetNativeArray<T3, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse3 = handles.GetNativeArray<T3, ushort>(1, state.GetSparse<T3, ushort>());
				bag.component3 = handles.GetNativeArray<T3, T3>(2, state.GetDense<T3>());
				bag.version3 = handles.GetNativeArray<T3, uint>(3, state.GetVersion<T3>());
				bag.indexes = handles.GetNativeArray<T4, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse4 = handles.GetNativeArray<T4, ushort>(1, state.GetSparse<T4, ushort>());
				bag.component4 = handles.GetNativeArray<T4, T4>(2, state.GetDense<T4>());
				bag.version4 = handles.GetNativeArray<T4, uint>(3, state.GetVersion<T4>());
				bag.indexes = handles.GetNativeArray<T5, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse5 = handles.GetNativeArray<T5, ushort>(1, state.GetSparse<T5, ushort>());
				bag.component5 = handles.GetNativeArray<T5, T5>(2, state.GetDense<T5>());
				bag.version5 = handles.GetNativeArray<T5, uint>(3, state.GetVersion<T5>());
				bag.indexes = handles.GetNativeArray<T6, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse6 = handles.GetNativeArray<T6, ushort>(1, state.GetSparse<T6, ushort>());
				bag.component6 = handles.GetNativeArray<T6, T6>(2, state.GetDense<T6>());
				bag.version6 = handles.GetNativeArray<T6, uint>(3, state.GetVersion<T6>());

				return bag;
			}
			public static JobBag<T0, T1, T2, T3, T4, T5, T6, T7> Create<T0, T1, T2, T3, T4, T5, T6, T7>(Filter<T0, T1, T2, T3, T4, T5, T6, T7> filter)
				where T0 : unmanaged, IComponent
				where T1 : unmanaged, IComponent
				where T2 : unmanaged, IComponent
				where T3 : unmanaged, IComponent
				where T4 : unmanaged, IComponent
				where T5 : unmanaged, IComponent
				where T6 : unmanaged, IComponent
				where T7 : unmanaged, IComponent

			{
				var state = filter.GetState();
				var filterData = filter.GetFilterData();
				var handles = state.GetModuleData<NativeArrayHandles>(NativeArrayHandles.USER_DATA_ID);

				JobBag<T0, T1, T2, T3, T4, T5, T6, T7> bag;
				bag.indexes = handles.GetNativeArray<T0, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse0 = handles.GetNativeArray<T0, ushort>(1, state.GetSparse<T0, ushort>());
				bag.component0 = handles.GetNativeArray<T0, T0>(2, state.GetDense<T0>());
				bag.version0 = handles.GetNativeArray<T0, uint>(3, state.GetVersion<T0>());
				bag.indexes = handles.GetNativeArray<T1, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse1 = handles.GetNativeArray<T1, ushort>(1, state.GetSparse<T1, ushort>());
				bag.component1 = handles.GetNativeArray<T1, T1>(2, state.GetDense<T1>());
				bag.version1 = handles.GetNativeArray<T1, uint>(3, state.GetVersion<T1>());
				bag.indexes = handles.GetNativeArray<T2, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse2 = handles.GetNativeArray<T2, ushort>(1, state.GetSparse<T2, ushort>());
				bag.component2 = handles.GetNativeArray<T2, T2>(2, state.GetDense<T2>());
				bag.version2 = handles.GetNativeArray<T2, uint>(3, state.GetVersion<T2>());
				bag.indexes = handles.GetNativeArray<T3, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse3 = handles.GetNativeArray<T3, ushort>(1, state.GetSparse<T3, ushort>());
				bag.component3 = handles.GetNativeArray<T3, T3>(2, state.GetDense<T3>());
				bag.version3 = handles.GetNativeArray<T3, uint>(3, state.GetVersion<T3>());
				bag.indexes = handles.GetNativeArray<T4, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse4 = handles.GetNativeArray<T4, ushort>(1, state.GetSparse<T4, ushort>());
				bag.component4 = handles.GetNativeArray<T4, T4>(2, state.GetDense<T4>());
				bag.version4 = handles.GetNativeArray<T4, uint>(3, state.GetVersion<T4>());
				bag.indexes = handles.GetNativeArray<T5, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse5 = handles.GetNativeArray<T5, ushort>(1, state.GetSparse<T5, ushort>());
				bag.component5 = handles.GetNativeArray<T5, T5>(2, state.GetDense<T5>());
				bag.version5 = handles.GetNativeArray<T5, uint>(3, state.GetVersion<T5>());
				bag.indexes = handles.GetNativeArray<T6, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse6 = handles.GetNativeArray<T6, ushort>(1, state.GetSparse<T6, ushort>());
				bag.component6 = handles.GetNativeArray<T6, T6>(2, state.GetDense<T6>());
				bag.version6 = handles.GetNativeArray<T6, uint>(3, state.GetVersion<T6>());
				bag.indexes = handles.GetNativeArray<T7, EntityId>(0, filterData->GetEntities());
				bag.count = (int)filterData->entityCount;
				bag.sparse7 = handles.GetNativeArray<T7, ushort>(1, state.GetSparse<T7, ushort>());
				bag.component7 = handles.GetNativeArray<T7, T7>(2, state.GetDense<T7>());
				bag.version7 = handles.GetNativeArray<T7, uint>(3, state.GetVersion<T7>());

				return bag;
			}

		}
		public static JobHandle AsJobParallel<TJob, T0>(this Filter<T0> filter, TJob job = default)
            where TJob : struct, IJobParallelFilterBag<JobBag<T0>>
            where T0 : unmanaged, IComponent

			=> job.Schedule(BagJobFactory.Create(filter));
        
		public static JobHandle AsJobParallel<TJob, T0, T1>(this Filter<T0, T1> filter, TJob job = default)
            where TJob : struct, IJobParallelFilterBag<JobBag<T0, T1>>
            where T0 : unmanaged, IComponent
            where T1 : unmanaged, IComponent

			=> job.Schedule(BagJobFactory.Create(filter));
        
		public static JobHandle AsJobParallel<TJob, T0, T1, T2>(this Filter<T0, T1, T2> filter, TJob job = default)
            where TJob : struct, IJobParallelFilterBag<JobBag<T0, T1, T2>>
            where T0 : unmanaged, IComponent
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent

			=> job.Schedule(BagJobFactory.Create(filter));
        
		public static JobHandle AsJobParallel<TJob, T0, T1, T2, T3>(this Filter<T0, T1, T2, T3> filter, TJob job = default)
            where TJob : struct, IJobParallelFilterBag<JobBag<T0, T1, T2, T3>>
            where T0 : unmanaged, IComponent
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent
            where T3 : unmanaged, IComponent

			=> job.Schedule(BagJobFactory.Create(filter));
        
		public static JobHandle AsJobParallel<TJob, T0, T1, T2, T3, T4>(this Filter<T0, T1, T2, T3, T4> filter, TJob job = default)
            where TJob : struct, IJobParallelFilterBag<JobBag<T0, T1, T2, T3, T4>>
            where T0 : unmanaged, IComponent
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent
            where T3 : unmanaged, IComponent
            where T4 : unmanaged, IComponent

			=> job.Schedule(BagJobFactory.Create(filter));
        
		public static JobHandle AsJobParallel<TJob, T0, T1, T2, T3, T4, T5>(this Filter<T0, T1, T2, T3, T4, T5> filter, TJob job = default)
            where TJob : struct, IJobParallelFilterBag<JobBag<T0, T1, T2, T3, T4, T5>>
            where T0 : unmanaged, IComponent
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent
            where T3 : unmanaged, IComponent
            where T4 : unmanaged, IComponent
            where T5 : unmanaged, IComponent

			=> job.Schedule(BagJobFactory.Create(filter));
        
		public static JobHandle AsJobParallel<TJob, T0, T1, T2, T3, T4, T5, T6>(this Filter<T0, T1, T2, T3, T4, T5, T6> filter, TJob job = default)
            where TJob : struct, IJobParallelFilterBag<JobBag<T0, T1, T2, T3, T4, T5, T6>>
            where T0 : unmanaged, IComponent
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent
            where T3 : unmanaged, IComponent
            where T4 : unmanaged, IComponent
            where T5 : unmanaged, IComponent
            where T6 : unmanaged, IComponent

			=> job.Schedule(BagJobFactory.Create(filter));
        
		public static JobHandle AsJobParallel<TJob, T0, T1, T2, T3, T4, T5, T6, T7>(this Filter<T0, T1, T2, T3, T4, T5, T6, T7> filter, TJob job = default)
            where TJob : struct, IJobParallelFilterBag<JobBag<T0, T1, T2, T3, T4, T5, T6, T7>>
            where T0 : unmanaged, IComponent
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent
            where T3 : unmanaged, IComponent
            where T4 : unmanaged, IComponent
            where T5 : unmanaged, IComponent
            where T6 : unmanaged, IComponent
            where T7 : unmanaged, IComponent

			=> job.Schedule(BagJobFactory.Create(filter));
        

        public static JobHandle AsJob<TJob, T0>(this Filter<T0> filter, TJob job = default)
            where TJob : struct, IJobFilterBag<JobBag<T0>>
            where T0 : unmanaged, IComponent

			=> job.Schedule(BagJobFactory.Create(filter));
        public static JobHandle AsJob<TJob, T0, T1>(this Filter<T0, T1> filter, TJob job = default)
            where TJob : struct, IJobFilterBag<JobBag<T0, T1>>
            where T0 : unmanaged, IComponent
            where T1 : unmanaged, IComponent

			=> job.Schedule(BagJobFactory.Create(filter));
        public static JobHandle AsJob<TJob, T0, T1, T2>(this Filter<T0, T1, T2> filter, TJob job = default)
            where TJob : struct, IJobFilterBag<JobBag<T0, T1, T2>>
            where T0 : unmanaged, IComponent
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent

			=> job.Schedule(BagJobFactory.Create(filter));
        public static JobHandle AsJob<TJob, T0, T1, T2, T3>(this Filter<T0, T1, T2, T3> filter, TJob job = default)
            where TJob : struct, IJobFilterBag<JobBag<T0, T1, T2, T3>>
            where T0 : unmanaged, IComponent
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent
            where T3 : unmanaged, IComponent

			=> job.Schedule(BagJobFactory.Create(filter));
        public static JobHandle AsJob<TJob, T0, T1, T2, T3, T4>(this Filter<T0, T1, T2, T3, T4> filter, TJob job = default)
            where TJob : struct, IJobFilterBag<JobBag<T0, T1, T2, T3, T4>>
            where T0 : unmanaged, IComponent
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent
            where T3 : unmanaged, IComponent
            where T4 : unmanaged, IComponent

			=> job.Schedule(BagJobFactory.Create(filter));
        public static JobHandle AsJob<TJob, T0, T1, T2, T3, T4, T5>(this Filter<T0, T1, T2, T3, T4, T5> filter, TJob job = default)
            where TJob : struct, IJobFilterBag<JobBag<T0, T1, T2, T3, T4, T5>>
            where T0 : unmanaged, IComponent
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent
            where T3 : unmanaged, IComponent
            where T4 : unmanaged, IComponent
            where T5 : unmanaged, IComponent

			=> job.Schedule(BagJobFactory.Create(filter));
        public static JobHandle AsJob<TJob, T0, T1, T2, T3, T4, T5, T6>(this Filter<T0, T1, T2, T3, T4, T5, T6> filter, TJob job = default)
            where TJob : struct, IJobFilterBag<JobBag<T0, T1, T2, T3, T4, T5, T6>>
            where T0 : unmanaged, IComponent
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent
            where T3 : unmanaged, IComponent
            where T4 : unmanaged, IComponent
            where T5 : unmanaged, IComponent
            where T6 : unmanaged, IComponent

			=> job.Schedule(BagJobFactory.Create(filter));
        public static JobHandle AsJob<TJob, T0, T1, T2, T3, T4, T5, T6, T7>(this Filter<T0, T1, T2, T3, T4, T5, T6, T7> filter, TJob job = default)
            where TJob : struct, IJobFilterBag<JobBag<T0, T1, T2, T3, T4, T5, T6, T7>>
            where T0 : unmanaged, IComponent
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent
            where T3 : unmanaged, IComponent
            where T4 : unmanaged, IComponent
            where T5 : unmanaged, IComponent
            where T6 : unmanaged, IComponent
            where T7 : unmanaged, IComponent

			=> job.Schedule(BagJobFactory.Create(filter));

    }
}