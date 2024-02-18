using System;
using Game.Objects;
using Game.UI.InGame;
using Game.Vehicles;
using HarmonyLib;
using Unity.Collections;
using Unity.Entities;

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
                info.label = "Surface Data";
                info.Add(new InfoList.Item($"SnowAmount: {surface.m_SnowAmount}"));
                info.Add(new InfoList.Item($"Snow Accum: {surface.m_AccumulatedSnow}"));
                info.Add(new InfoList.Item($"Dirtyness: {surface.m_Dirtyness}"));
                info.Add(new InfoList.Item($"Wetness: {surface.m_Wetness}"));
                info.Add(new InfoList.Item($"Wetness Accum: {surface.m_AccumulatedWetness}"));                                
            };
            ___m_InfoUISystem.AddDeveloperInfo(new InfoList(
                (entity, prefab) => em.HasComponent<Vehicle>(entity) && em.HasComponent<Surface>(entity),
                updateInfoMethod
            ));
        }
    }

}