using BSG.CameraEffects;
using EFT;
using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using UnityEngine;

namespace RealismCommonLib.StateControllers
{
    public class GearState: MonoBehaviour
    {
        public Player _player;

        public bool HasGasMask { get;  private set; }

        public bool FaceShieldIsActive { get; private set; }

        public bool NVGIsActive { get; private set; }

        public bool ThermalIsActive { get; private set; }

        public bool GearBlocksMouth { get; private set; }

        //stance controller has a say via config option
        public bool NVGsCouldBlockADS { get; private set; }

        //need a check for gear overhaul to check if gear permits ADS with face shield on
        //stance controller also has a say via config option
        public bool FaceShieldCouldBlockADS { get; private set; }

        public void CheckGear()
        {
            FaceShieldComponent fsComponent = _player.FaceShieldObserver.Component;
            NightVisionComponent nvgComponent = _player.NightVisionObserver.Component;
            ThermalVisionComponent thermComponent = _player.ThermalVisionObserver.Component;

            FaceShieldIsActive = fsComponent != null && (fsComponent.Togglable == null || fsComponent.Togglable.On);
            NVGIsActive = nvgComponent != null && (nvgComponent.Togglable == null || nvgComponent.Togglable.On);
            ThermalIsActive = thermComponent != null && (thermComponent.Togglable == null || thermComponent.Togglable.On);
            FaceShieldCouldBlockADS = FaceShieldIsActive;
            NVGsCouldBlockADS =  (NVGIsActive && _player.ProceduralWeaponAnimation.CurrentScope.IsOptic) || ThermalIsActive;
        }
    }
}
