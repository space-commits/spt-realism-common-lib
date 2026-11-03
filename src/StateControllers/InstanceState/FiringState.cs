using RealismCommonLib.Events;
using RealismCommonLib.Utils;
using UnityEngine;
using static EFT.Player;
using static RealismCommonLib.Plugin;

namespace RealismCommonLib.StateControllers.InstanceState
{


    public class FiringState : MonoBehaviour
    {
        private DelayTimer _firingTimer = new DelayTimer(PluginConfig.FiringResetDelay.Value);
        private DelayTimer _firingDeafenTimer = new DelayTimer(PluginConfig.FiringDeafenResetDelay.Value);
        private DelayTimer _firingMovementTimer = new DelayTimer(PluginConfig.FiringMovementResetDelay.Value);
        private DelayTimer _firingFromStanceTimer = new DelayTimer(PluginConfig.FiringFromStanceResetDelay.Value);
        private DelayTimer _firingVisualRecoilTimer = new DelayTimer(PluginConfig.FiringVisualResetDelay.Value);

        public bool AutoFireOn
        {
            get
            {
                return PlayerStateInstance?.FirearmController?.autoFireOn ?? false;
            }
        }

        //exposed for external use
        public bool IsFiring
        {
            get
            {
                return _firingTimer.IsActive || AutoFireOn;
            }
        }

        public bool IsFiringDeafen
        {
            get
            {
                return _firingDeafenTimer.IsActive;
            }
        }

        public bool IsFiringMovement
        {
            get
            {
                return _firingMovementTimer.IsActive;
            }
        }

        public bool IsFiringFromStance
        {
            get
            {
                return _firingFromStanceTimer.IsActive;
            }
        }

        public bool DoFiringVisualRecoil
        {
            get
            {
                return _firingVisualRecoilTimer.IsActive;
            }
        }

        public int ShotCount { get; private set; } = 0;

        public float FiringDuration { get; private set; } = 0;

        void Awake()
        {
        }

        void Update()
        {
            if (PlayerStateInstance?.FirearmController == null || !PlayerStateInstance.WeaponIsReady) return;
            UpdateFiringState();
        }

        void OnDestroy()
        {
        }

        public void UpdateFiringState()
        {
            _firingTimer.Update();
            _firingDeafenTimer.Update();
            _firingMovementTimer.Update();
            _firingFromStanceTimer.Update();
            _firingVisualRecoilTimer.Update();

            if (IsFiring) FiringDuration += Time.deltaTime;

            if (!_firingTimer.IsActive && !AutoFireOn) ShotCount = 0;

            if (!AutoFireOn) FiringDuration = 0f;

        }

        public void SetFiringState()
        {
            PlayerEvents.RaiseOnShotFired();

            ShotCount++;

            _firingTimer.Start();
            _firingDeafenTimer.Start();
            _firingFromStanceTimer.Start();
            _firingMovementTimer.Start();
            _firingVisualRecoilTimer.Start();
        }
    }
}
