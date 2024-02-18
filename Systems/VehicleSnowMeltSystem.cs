using Colossal.Collections;
using Colossal.Logging;
using Colossal.Serialization.Entities;
using Game;
using Game.Common;
using Game.Objects;
using Game.Simulation;
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

        WetnessSystem mWetnessSystem;
        // ComponentTypeHandle<Owner> mOwnerTypeHandle;
        // ComponentTypeHandle<

        // EntityArchetype mSubObjectEventArchetype;

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            return mWetnessSystem.GetUpdateInterval(phase);
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            var eqb = new EntityQueryBuilder(Allocator.Temp);
            mQuery = eqb.WithAll<Vehicle, Moving, Surface>()
                .WithNone<ParkedCar, Deleted>()
                .Build(EntityManager);
            eqb.Dispose();

            mSurfaceTypeHandle = this.GetComponentTypeHandle<Surface>(false);
            mMovingTypeHandle = this.GetComponentTypeHandle<Moving>(true);
            mWetnessSystem = this.World.GetOrCreateSystemManaged<WetnessSystem>();
            // mSubObjectEventArchetype = this.EntityManager.CreateArchetype( new ComponentType[] {
            //     ComponentType.ReadWrite<Event>(),
            //     ComponentType.ReadWrite<SubObjectsUpdated>()
            // });

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
                // var ecb = mModificationEndBarrier.CreateCommandBuffer().AsParallelWriter();                
                MeltSnowJob job = new() {
                    mMovingHandle = mMovingTypeHandle,
                    mSurfaceHandle = mSurfaceTypeHandle
                    // mSubObjectEventArchetype = mSubObjectEventArchetype
                };                
                this.Dependency = job.ScheduleParallel(mQuery, this.Dependency);
            }            
        }

        [BurstCompile]
        private struct MeltSnowJob : IJobChunk {

            public ComponentTypeHandle<Moving> mMovingHandle;
            public ComponentTypeHandle<Surface> mSurfaceHandle;
            // public EntityArchetype mSubObjectEventArchetype;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var moving = chunk.GetNativeArray(ref mMovingHandle);
                var surface = chunk.GetNativeArray(ref mSurfaceHandle);
                for(int i = 0; i < chunk.Count; i++) {
                    if (math.length(moving[i].m_Velocity) >= 1.0f && (surface[i].m_AccumulatedSnow > 0 || surface[i].m_SnowAmount > 0)) {
                        ref Surface s =  ref surface.ElementAt(i);                                    
                        s.m_AccumulatedSnow = (byte) 0;
                        s.m_SnowAmount = (byte) 0;                        
                    }
                }
            }
            
        }

    }

}