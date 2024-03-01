using System;
using Game.Buildings;
using Game.Common;
using Game.Net;
using Game.Objects;
using Game.Rendering;
using Game.Simulation;
using Game.Tools;
using Game.UI.InGame;
using Game.Vehicles;
using HarmonyLib;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;

namespace CS2ModsTesting {


    [HarmonyPatch]
    public class DevUIListComponents {

        [HarmonyPatch(typeof(DeveloperInfoUISystem), "OnCreate")]
        [HarmonyPostfix]
        public static void AddComponentsList(ref DeveloperInfoUISystem __instance, ref SelectedInfoUISystem ___m_InfoUISystem) {  
            var em = __instance.EntityManager;   
            var updateInfoMethod = (Entity entity, Entity prefab, InfoList info) => {
                info.label = "Instance ECS Components";
                NativeArray<ComponentType> arr = em.GetChunk(entity).Archetype.GetComponentTypes(Allocator.Temp);
                for (int i = 0; i < arr.Length; i++) {
                    var ct = arr[i];
                    info.Add(new InfoList.Item(ct.GetManagedType().FullName));
                }
            };    
            ___m_InfoUISystem.AddDeveloperInfo(new InfoList( 
                (entity, prefab) => entity != null, updateInfoMethod
            ));

            updateInfoMethod = (Entity entity, Entity prefab, InfoList info) => {
                info.label = "Prefab ECS Components";
                NativeArray<ComponentType> arr = em.GetChunk(prefab).Archetype.GetComponentTypes(Allocator.Temp);
                for (int i = 0; i < arr.Length; i++) {
                    var ct = arr[i];
                    info.Add(new InfoList.Item(ct.GetManagedType().FullName));
                }
            };    
            ___m_InfoUISystem.AddDeveloperInfo(new InfoList( 
                (entity, prefab) => prefab != Entity.Null, updateInfoMethod
            ));

            updateInfoMethod = (Entity entity, Entity prefab, InfoList info) => {
                var surface = em.GetComponentData<Surface>(entity);
                var flag = em.HasComponent<SubObject>(entity) && !em.HasComponent<Owner>(entity);
                info.label = "Surface Data";
                info.Add(new InfoList.Item($"SnowAmount: {surface.m_SnowAmount}"));
                info.Add(new InfoList.Item($"Snow Accum: {surface.m_AccumulatedSnow}"));
                info.Add(new InfoList.Item($"Dirtyness: {surface.m_Dirtyness}"));
                info.Add(new InfoList.Item($"Wetness: {surface.m_Wetness}"));
                info.Add(new InfoList.Item($"Wetness Accum: {surface.m_AccumulatedWetness}"));     
                info.Add(new InfoList.Item($"Flag: {flag}"));
            };
            ___m_InfoUISystem.AddDeveloperInfo(new InfoList(
                (entity, prefab) => em.HasComponent<Vehicle>(entity) && em.HasComponent<Surface>(entity),
                updateInfoMethod
            ));

            EntityQueryBuilder eqb = new EntityQueryBuilder(Allocator.Temp);
            var streetLightSystemQuery = eqb.WithAll<StreetLight, LightState, Emissive>()
                .WithNone<Road, Building, Watercraft, Deleted, Destroyed, Temp, Owner>()
                .Build(em);
            eqb.Dispose();
            updateInfoMethod = (Entity entity, Entity prefab, InfoList info) => {
                var streetLight = em.GetComponentData<StreetLight>(entity);                
                var streetLightStates = em.GetBuffer<LightState>(entity, true);
                var emissives = em.GetBuffer<Emissive>(entity, true);
                info.label = "Street Light Info";
                info.Add(new InfoList.Item($"Street light State: {streetLight.m_State.ToString()}"));                
                foreach(LightState state in streetLightStates) {
                    info.Add(new InfoList.Item($"LightState Intensity: {state.m_Intensity}, Colour: {state.m_Color}"));
                }
                foreach(Emissive emissive in emissives) {
                    info.Add(new InfoList.Item($"Emissive Intensity: {emissive.m_LightOffset}, Colour: {emissive.m_Updated}"));
                }        
                info.Add(new InfoList.Item($"Matches Query: {streetLightSystemQuery.Matches(entity)}"));
            };
            ___m_InfoUISystem.AddDeveloperInfo(new InfoList(
                (entity, prefab) => em.HasComponent<StreetLight>(entity) && em.HasBuffer<LightState>(entity) && em.HasBuffer<Emissive>(entity),
                updateInfoMethod
            ));
        }
    }

}