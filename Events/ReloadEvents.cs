using EFT;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealismCommonLib.Events
{
    public static class ReloadEvents
    {
        //if I need priority, can add a dictionary of type int and action, and sort + iterate over it

        /// <summary>
        /// Triggered after weapon reload/manipulation animation
        /// </summary>
        public static event Action? WeaponStateReset;

        public static event Action? ChamberCheck;

        public static event Action? CheckAmmo;

        /// <summary>
        /// Triggered on bolt/pump action usage.
        /// </summary>
        public static event Action? BoltActionReload;

        public static event Action? MalfFix;

        public static event Action? Rechamber;

        public static event Action? MagReload;

        public static event Action? QuickMagReload;

        private static void RaiseEvent(Action? eventHandler, string errorContext)
        {
            if (eventHandler == null) return;

            foreach (Action handler in eventHandler.GetInvocationList())
            {
                try
                {
                    handler();
                }
                catch (Exception ex)
                {
                    Plugin.ModLogger.LogError($"Error invoking {errorContext}: {ex}");
                }
            }
        }

        internal static void RaiseWeaponStateReset()
        {
            RaiseEvent(WeaponStateReset, nameof(WeaponStateReset));
        }

        internal static void RaiseChamberCheck()
        {
            RaiseEvent(ChamberCheck, nameof(ChamberCheck));
        }

        internal static void RaiseCheckAmmo()
        {
            RaiseEvent(CheckAmmo, nameof(CheckAmmo));
        }

        internal static void RaiseBoltActionReload()
        {
            RaiseEvent(BoltActionReload, nameof(BoltActionReload));
        }

        internal static void RaiseOnSetMalfFix()
        {
            RaiseEvent(MalfFix, nameof(MalfFix));
        }

        internal static void RaiseOnRechamber()
        {
            RaiseEvent(Rechamber, nameof(Rechamber));
        }

        internal static void RaiseOnMagReload()
        {
            RaiseEvent(MagReload, nameof(MagReload));
        }

        internal static void RaiseOnQuickMagReload()
        {
            RaiseEvent(QuickMagReload, nameof(QuickMagReload));
        }
    }
}
