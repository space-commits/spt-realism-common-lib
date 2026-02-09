using RealismCommonLib.Events;
using RealismCommonLib.Utils;
using UnityEngine;
using static EFT.Player;
using static RealismCommonLib.Plugin;

namespace RealismCommonLib.StateControllers
{


    public class FiringState : MonoBehaviour
    {
        private TimeGate _firingTimeGate = new TimeGate(PluginConfig.FiringResetDelay.Value);
        private TimeGate _firingDeafenTimeGate = new TimeGate(PluginConfig.FiringDeafenResetDelay.Value);
        private TimeGate _firingMovementTimeGate = new TimeGate(PluginConfig.FiringMovementResetDelay.Value);
        private TimeGate _firingFromStanceTimeGate = new TimeGate(PluginConfig.FiringFromStanceResetDelay.Value);
        private TimeGate _firingVisualRecoilTimeGate = new TimeGate(PluginConfig.FiringVisualResetDelay.Value);

        public bool AutoFireOn 
        {
            get
            {
                return PlayerStateInstance.FirearmController.autoFireOn;
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

        void Awake() 
        {
            InputEvents.ToggleGogglesInput += UpdateHipfireState;
        }

        void Update() 
        {
            UpdateFiringState();
        }

        void OnDestroy() 
        {
            InputEvents.ToggleGogglesInput -= UpdateHipfireState;
        }

        //update hipfire to take NVG toggle into account
        private void UpdateHipfireState() 
        {
            PlayerStateInstance.FirearmController.UpdateHipInaccuracy();
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
            Events.PlayerEvents.RaiseOnShotFired();

            ShotCount++;

            _firingTimeGate.Reset();
            _firingDeafenTimeGate.Reset();
            _firingFromStanceTimeGate.Reset();
            _firingMovementTimeGate.Reset();
            _firingVisualRecoilTimeGate.Reset();
        }
    }
}
