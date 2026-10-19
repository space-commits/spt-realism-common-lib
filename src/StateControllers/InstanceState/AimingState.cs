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
        // The aim state last reported through PlayerEvents.AimStateChanged
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

        // Self-heal: AimingChangedPatch should report every change, but if one slips past it (a path that bypasses
        // AimingChanged), report it here so listeners caching the aim state don't stay stuck on a stale value.
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
