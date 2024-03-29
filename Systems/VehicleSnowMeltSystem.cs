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
        EntityTypeHandle mEntityTypeHandle;
        
        EndFrameBarrier mEndFrameBarrier;
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
            mEntityTypeHandle = this.GetEntityTypeHandle();
            mWetnessSystem = this.World.GetOrCreateSystemManaged<WetnessSystem>();
            mEndFrameBarrier = World.GetOrCreateSystemManaged<EndFrameBarrier>();
            // mSubObjectEventArchetype = this.EntityManager.CreateArchetype( new ComponentType[] {
            //     ComponentType.ReadWrite<Event>(),
            //     ComponentType.ReadWrite<SubObjectsUpdated>()
            // });

            // mModificationEndBarrier = this.World.GetOrCreateSystemManaged<ModificationEndBarrier>();

            Log = Mod.Instance.Log;
            mWetnessSystem.Enabled = false;
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
                this.mEntityTypeHandle.Update(this);           
                // var ecb = mModificationEndBarrier.CreateCommandBuffer().AsParallelWriter();                
                MeltSnowJob job = new() {
                    mMovingHandle = mMovingTypeHandle,
                    mSurfaceHandle = mSurfaceTypeHandle,
                    mEntityTypeHandle = mEntityTypeHandle,
                    ecb = mEndFrameBarrier.CreateCommandBuffer().AsParallelWriter()
                    // mSubObjectEventArchetype = mSubObjectEventArchetype
                };                
                this.Dependency = job.ScheduleParallel(mQuery, this.Dependency);                
                mEndFrameBarrier.AddJobHandleForProducer(this.Dependency);
            }            
        }

        [BurstCompile]
        private struct MeltSnowJob : IJobChunk {

            public EntityTypeHandle mEntityTypeHandle;
            public ComponentTypeHandle<Moving> mMovingHandle;
            public ComponentTypeHandle<Surface> mSurfaceHandle;
            // public EntityArchetype mSubObjectEventArchetype;
            public EntityCommandBuffer.ParallelWriter ecb;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var moving = chunk.GetNativeArray(ref mMovingHandle);
                var surface = chunk.GetNativeArray(ref mSurfaceHandle);
                var entities = chunk.GetNativeArray(mEntityTypeHandle);
                for(int i = 0; i < chunk.Count; i++) {
                    if (math.length(moving[i].m_Velocity) >= 1.0f && (surface[i].m_AccumulatedSnow > 0 || surface[i].m_SnowAmount > 0)) {
                        ref Surface s =  ref surface.ElementAt(i);                                    
                        s.m_AccumulatedSnow = (byte) 0;
                        s.m_SnowAmount = (byte) 0;     
                        //use the EndFrameBuffer to add EffectsUpdated to the entity. See StreetLightSystem line 263 for an example
                        ecb.AddComponent<EffectsUpdated>(unfilteredChunkIndex, entities[i]);
                    }
                }
            }
            
        }

    }

}