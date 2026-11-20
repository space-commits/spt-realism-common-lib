using EFT.InventoryLogic;
using EFT;
using RealismCommonLib.Events;
using RealismCommonLib.Utils;
using UnityEngine;
using static EFT.Player;
using static RealismCommonLib.Plugin;
using RealismCommonLib.ModifierHandlers;

namespace RealismCommonLib.StateControllers.InstanceState
{
    public class AimingState : MonoBehaviour
    {
        // last value sent through PlayerEvents.AimStateChanged
        private bool _lastReportedAiming;

        public bool PlayerCanAim
        {
            get
            {
                return BoolHandlers.CanAim.Evaluate();
            }
        }

        public bool IsAiming
        {
            get
            {
                return PlayerStateInstance.PWA.IsAiming;
            }
        }

        // Backup for AimingChangedPatch, in case something changes aim without going through AimingChanged.
        // Stops listeners that cache the aim state getting stuck on a stale value.
        void Update()
        {
            if (PlayerStateInstance?.PWA == null) return;

            bool isAiming = IsAiming;
            if (isAiming != _lastReportedAiming)
                ReportAimState(isAiming);
        }

        internal void ReportAimState(bool isAiming)
        {
            if (isAiming == _lastReportedAiming) return;

            _lastReportedAiming = isAiming;
            PlayerEvents.RaiseOnAimStateChanged();
        }
    }
}
