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

        void Awake()
        {

            CheckGear();

            PlayerEvents.OnPlayerInit += CheckGear;
            PlayerEvents.OnItemAddedOrRemoved += CheckGear;
            InputEvents.ToggleGogglesInput += CheckGear;
            InputEvents.ChangeScopeInput += CheckGear;
            InputEvents.ChangeScopeMagnificationInput += CheckGear;
        }

        void OnDestroy()
        {
            PlayerEvents.OnItemAddedOrRemoved -= CheckGear;
            PlayerEvents.OnPlayerInit -= CheckGear;
            InputEvents.ToggleGogglesInput -= CheckGear;
            InputEvents.ChangeScopeInput -= CheckGear;
            InputEvents.ChangeScopeMagnificationInput -= CheckGear;
        }

        public void CheckGear()
        {
            if (PlayerStateInstance == null || PlayerStateInstance.Player == null || GameStateInstance == null || !GameStateInstance.PlayerIsInRaidOrHideout)
                return;

            Player player = PlayerStateInstance.Player;
            FaceShieldComponent fsComponent = player.FaceShieldObserver.Component;
            NightVisionComponent nvgComponent = player.NightVisionObserver.Component;
            ThermalVisionComponent thermComponent = player.ThermalVisionObserver.Component;

            FaceShieldIsActive = fsComponent != null && (fsComponent.Togglable == null || fsComponent.Togglable.On);
            NVGIsActive = nvgComponent != null && (nvgComponent.Togglable == null || nvgComponent.Togglable.On);
            ThermalIsActive = thermComponent != null && (thermComponent.Togglable == null || thermComponent.Togglable.On);

            ModLogger.LogWarning($"GearState: FaceShieldIsActive={FaceShieldIsActive}, NVGIsActive={NVGIsActive}, ThermalIsActive={ThermalIsActive}");

            PlayerStateInstance.FirearmController?.UpdateHipInaccuracy();
        }
    }
}
