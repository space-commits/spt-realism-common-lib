using System;
using EFT;
using EFT.InventoryLogic;
using RealismCommonLib.Events;
using UnityEngine;
using static RealismCommonLib.Plugin;

namespace RealismCommonLib.StateControllers.InstanceState
{
    public class GearState : MonoBehaviour
    {
        public bool HasGasMask { get; private set; }

        public bool FaceShieldIsActive { get; private set; }

        public bool NVGIsActive { get; private set; }

        public bool ThermalIsActive { get; private set; }

        public bool GearBlocksMouth { get; private set; }

        /// <summary>
        /// Fires from CheckGear when a gear flag (face shield, NVG, thermal) actually changes, after the flags are updated.
        /// Use this instead of the events CheckGear listens to, or you can end up reading the flags before they're refreshed.
        /// </summary>
        public Action? OnGearStateChanged;

        void Awake()
        {

            CheckGear();

            // the gear flags can only change with these
            PlayerEvents.OnPlayerInit += CheckGear;
            PlayerEvents.OnItemAddedOrRemoved += CheckGear;
            PlayerEvents.ToggleHeadDevice += CheckGear;

            // these don't change the gear flags, but hip inaccuracy also depends on the weapon in hands, its sight and its state
            InputEvents.ChangeScopeInput += OnWeaponChanged;
            InputEvents.ChangeScopeMagnificationInput += OnWeaponChanged;
            PlayerEvents.OnHandsControllerAssigned += OnWeaponChanged;
            WeaponStateInstance.OnWeaponStateChanged += OnWeaponChanged;
        }

        void OnDestroy()
        {
            PlayerEvents.OnItemAddedOrRemoved -= CheckGear;
            PlayerEvents.OnPlayerInit -= CheckGear;
            PlayerEvents.ToggleHeadDevice -= CheckGear;
            InputEvents.ChangeScopeInput -= OnWeaponChanged;
            InputEvents.ChangeScopeMagnificationInput -= OnWeaponChanged;
            PlayerEvents.OnHandsControllerAssigned -= OnWeaponChanged;
            WeaponStateInstance.OnWeaponStateChanged -= OnWeaponChanged;
        }

        private void OnWeaponChanged()
        {
            CheckGear(); // cheap, and picks up a flag change that none of the gear events reported
            PlayerStateInstance?.RequestHipInaccuracyRefresh();
        }

        /// <summary>
        /// Re-reads the gear flags. Hip inaccuracy refresh and OnGearStateChanged only happen if one changed.
        /// </summary>
        public void CheckGear()
        {
            if (PlayerStateInstance == null || PlayerStateInstance.Player == null || GameStateInstance == null || !GameStateInstance.PlayerIsInRaidOrHideout)
                return;

            Player player = PlayerStateInstance.Player;
            FaceShieldComponent fsComponent = player.FaceShieldObserver.Component;
            NightVisionComponent nvgComponent = player.NightVisionObserver.Component;
            ThermalVisionComponent thermComponent = player.ThermalVisionObserver.Component;

            bool faceShield = fsComponent != null && (fsComponent.Togglable == null || fsComponent.Togglable.On);
            bool nvg = nvgComponent != null && (nvgComponent.Togglable == null || nvgComponent.Togglable.On);
            bool thermal = thermComponent != null && (thermComponent.Togglable == null || thermComponent.Togglable.On);

            if (faceShield == FaceShieldIsActive && nvg == NVGIsActive && thermal == ThermalIsActive)
                return;

            FaceShieldIsActive = faceShield;
            NVGIsActive = nvg;
            ThermalIsActive = thermal;

            PlayerStateInstance.RequestHipInaccuracyRefresh();

            BaseEventHandler.RaiseEvent(OnGearStateChanged);
        }
    }
}
