using Game;
using Game.Buildings;
using Game.Common;
using Game.Net;
using Game.Objects;
using Game.Rendering;
using Game.Simulation;
using Game.Tools;
using Game.Vehicles;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace CS2ModsTesting.Systems {

    public class PropLightingSystem : GameSystemBase
    {
        EntityQuery propQuery;
        LightingSystem lightingSystem;
        EndFrameBarrier endFrameBarrier;
        SimulationSystem simulationSystem;

        ComponentTypeHandle<StreetLight> streetLightTypeHandle;
        // ComponentTypeHandle<LightState> lightStateTypeHandle;
        EntityTypeHandle entityTypeHandle;
        ComponentTypeHandle<PseudoRandomSeed> pseudoRandomSeedHandle;
        ComponentLookup<Owner> ownerLookup;        
        

        protected override void OnCreate()
        {
            base.OnCreate();
            EntityQueryBuilder eqb = new EntityQueryBuilder(Allocator.Temp);
            propQuery = eqb.WithAll<UpdateFrame, StreetLight, LightState>()
                .WithNone<Road, Building, Watercraft, Deleted, Destroyed, Temp, Owner>()
                .Build(this.EntityManager);
            lightingSystem = World.GetOrCreateSystemManaged<LightingSystem>();
            endFrameBarrier = World.GetOrCreateSystemManaged<EndFrameBarrier>();
            simulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();
            
            streetLightTypeHandle = GetComponentTypeHandle<StreetLight>();
            entityTypeHandle = GetEntityTypeHandle();
            pseudoRandomSeedHandle = GetComponentTypeHandle<PseudoRandomSeed>();
            ownerLookup = GetComponentLookup<Owner>(true);

            eqb.Dispose();
            base.RequireForUpdate(propQuery);
        }

        protected override void OnUpdate()
        {
            this.propQuery.ResetFilter();
            this.propQuery.SetSharedComponentFilter<UpdateFrame>(new UpdateFrame(SimulationUtils.GetUpdateFrameWithInterval(this.simulationSystem.frameIndex, (uint)this.GetUpdateInterval(SystemUpdatePhase.GameSimulation), 16)));

            entityTypeHandle.Update(this);
            streetLightTypeHandle.Update(this);
            ownerLookup.Update(this);
        }

        private struct UpdateLightsJob : IJobChunk
        {
            public EntityTypeHandle entityTypeHandle;
            public ComponentTypeHandle<StreetLight> streetLightTypeHandle;
            // public ComponentTypeHandle<LightState> lightStateTypeHandle;
            public ComponentLookup<Owner> ownerLookup; 

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entities = chunk.GetNativeArray(this.entityTypeHandle);
                NativeArray<StreetLight> streetLights = chunk.GetNativeArray(ref this.streetLightTypeHandle);
                // NativeArray<LightState> lightStates = chunk.GetNativeArray(ref this.lightStateTypeHandle);
                for(int i = 0; i < entities.Length; i++) {

                }
            }
        }
    }

}