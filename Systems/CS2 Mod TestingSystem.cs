using Game;
using Game.Audio;
using Game.Prefabs;
using Game.Simulation;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting;

namespace CS2 Mod Testing.Systems
{
    public class CS2 Mod TestingSystem : GameSystemBase
    {
        // private SimulationSystem simulation;

        protected override void OnCreate()
        {
            base.OnCreate();
            CreateKeyBinding();
            // Example on how to get a existing ECS System from the ECS World
            // this.simulation = World.GetExistingSystemManaged<SimulationSystem>();
        }

        private void CreateKeyBinding()
        {
            var inputAction = new InputAction("MyModHotkeyPress");
            inputAction.AddBinding("<Keyboard>/n");
            inputAction.performed += OnHotkeyPress;
            inputAction.Enable();
        }

        private void OnHotkeyPress(InputAction.CallbackContext obj)
        {
            UnityEngine.Debug.Log("You pressed the hotkey, very cool! Good job matey");
        }

        protected override void OnUpdate() {}
    }
}
