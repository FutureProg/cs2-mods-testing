using Colossal.Logging;
using Colossal.Serialization.Entities;
using Game;
using Game.Common;
using Game.Objects;
using Game.Vehicles;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace CS2ModsTesting.Systems {

    public class VehicleSnowMeltSystem : GameSystemBase
    {
        ILog Log;
        EntityQuery mQuery;
        bool run = false;
        
        ComponentTypeHandle<Moving> mMovingTypeHandle;
        ComponentTypeHandle<Surface> mSurfaceTypeHandle;

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            return 128;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            var eqb = new EntityQueryBuilder(Allocator.Temp);
            mQuery = eqb.WithAll<Vehicle, Moving, Surface>()
                .WithAny<Car, Train>()
                .WithNone<ParkedCar, Deleted>()
                .Build(EntityManager);
            eqb.Dispose();

            mSurfaceTypeHandle = this.GetComponentTypeHandle<Surface>();
            mMovingTypeHandle = this.GetComponentTypeHandle<Moving>();

            // mModificationEndBarrier = this.World.GetOrCreateSystemManaged<ModificationEndBarrier>();

            Log = Mod.Instance.Log;
        }

        protected override void OnGameLoaded(Context serializationContext)
        {
            base.OnGameLoaded(serializationContext);
            run = serializationContext.purpose == Purpose.LoadGame || serializationContext.purpose == Purpose.NewGame;
        }

        protected override void OnUpdate()
        {
            if (run) {
                mSurfaceTypeHandle.Update(this);
                mMovingTypeHandle.Update(this);                
                Log.Info($"Found {mQuery.CalculateEntityCount()} entities matching this query");

                // var ecb = mModificationEndBarrier.CreateCommandBuffer().AsParallelWriter();                
                MeltSnowJob job = new() {
                    mMovingHandle = mMovingTypeHandle,
                    mSurfaceHandle = mSurfaceTypeHandle
                };                
                this.Dependency = job.ScheduleParallel(mQuery, this.Dependency);
            }            
        }

        [BurstCompile]
        private struct MeltSnowJob : IJobChunk {

            public ComponentTypeHandle<Moving> mMovingHandle;
            public ComponentTypeHandle<Surface> mSurfaceHandle;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var moving = chunk.GetNativeArray(ref mMovingHandle);
                var surface = chunk.GetNativeArray(ref mSurfaceHandle);
                for(int i = 0; i < chunk.Count; i++) {
                    if (math.length(moving[i].m_Velocity) >= 1.0f && (surface[i].m_AccumulatedSnow > 0 || surface[i].m_SnowAmount > 0)) {
                        Surface s = surface[i];                        
                        s.m_AccumulatedSnow = 0;
                        s.m_SnowAmount = 0;
                        surface[i] = s;
                    }
                }
            }
            
        }

    }

}