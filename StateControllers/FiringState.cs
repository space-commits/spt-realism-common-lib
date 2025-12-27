using EFT;
using EFT.Animations;
using EFT.InventoryLogic;
using RealismCommonLib.Utils;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Numerics;
using System.Reflection;
using UnityEngine;
using static EFT.Player;

namespace RealismCommonLib.StateControllers
{
    public class FiringState : MonoBehaviour
    {
        private Player _player;
        private Player.FirearmController _fc;

        private TimeGate _firingTimeGate = new TimeGate(PluginConfig.FiringResetDelay.Value);
        private TimeGate _firingDeafenTimeGate = new TimeGate(PluginConfig.FiringDeafenResetDelay.Value);
        private TimeGate _firingMovementTimeGate = new TimeGate(PluginConfig.FiringMovementResetDelay.Value);
        private TimeGate _firingFromStanceTimeGate = new TimeGate(PluginConfig.FiringFromStanceResetDelay.Value);
        private TimeGate _firingVisualRecoilTimeGate = new TimeGate(PluginConfig.FiringVisualResetDelay.Value);

        public bool AutoFireOn 
        {
            get
            {
                return _fc != null && _fc.autoFireOn;
            }
        }

        //exposed for external use
        public bool IsFiring 
        { 
            get 
            { 
                return _firingTimeGate.IsActive || AutoFireOn;
            }
        }

        public bool IsFiringDeafen
        {
            get
            {
                return _firingDeafenTimeGate.IsActive;
            }
        }

        public bool IsFiringMovement
        {
            get
            {
                return _firingMovementTimeGate.IsActive;
            }
        }

        public bool IsFiringFromStance
        {
            get
            {
                return _firingFromStanceTimeGate.IsActive;
            }
        }

        public bool DoFiringVisualRecoil
        {
            get
            {
                return _firingVisualRecoilTimeGate.IsActive;
            }
        }

        public int ShotCount { get; private set; } = 0;

        public float FiringDuration { get; private set; } = 0;

        public void Update() 
        {

            UpdateFiringState();


        }

        public void UpdateFiringState()
        {
            _firingTimeGate.Update();
            _firingDeafenTimeGate.Update();
            _firingMovementTimeGate.Update();
            _firingFromStanceTimeGate.Update();
            _firingVisualRecoilTimeGate.Update();

            if (IsFiring) FiringDuration += Time.deltaTime;

            if (!_firingTimeGate.IsActive && !AutoFireOn) ShotCount = 0;

            if (!AutoFireOn) FiringDuration = 0f;

        }

        public void SetFiringState()
        {
            ShotCount++;

            _firingTimeGate.Reset();
            _firingDeafenTimeGate.Reset();
            _firingFromStanceTimeGate.Reset();
            _firingMovementTimeGate.Reset();
            _firingVisualRecoilTimeGate.Reset();
        }
    }
}
