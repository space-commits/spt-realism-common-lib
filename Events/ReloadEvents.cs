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
        public static event Action? BoltActionPump;

        public static event Action? MalfFix;

        public static event Action? Rechamber;

        public static event Action? MagReload;

        public static event Action? QuickMagReload;

        public static event Action? NewMagInserted;

        public static event Action? CurrentMagRemoved;

        public static event Action? InternalMagReload;

        private static void RaiseEvent(Action? eventHandler)
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
                    Plugin.ModLogger.LogError($"Error invoking {nameof(eventHandler)}: {ex}");
                }
            }
        }

        internal static void RaiseWeaponAnimationStateReset()
        {
            RaiseEvent(WeaponStateReset);
        }

        internal static void RaiseChamberCheck()
        {
            RaiseEvent(ChamberCheck);
        }

        internal static void RaiseCheckAmmo()
        {
            RaiseEvent(CheckAmmo);
        }

        internal static void RaiseBoltActionPump()
        {
            RaiseEvent(BoltActionPump);
        }

        internal static void RaiseInternalReload()
        {
            RaiseEvent(InternalMagReload);
        }

        internal static void RaiseOnSetMalfFix()
        {
            RaiseEvent(MalfFix);
        }

        internal static void RaiseOnRechamber()
        {
            RaiseEvent(Rechamber);
        }

        internal static void RaiseOnMagReload()
        {
            RaiseEvent(MagReload);
        }

        internal static void RaiseOnQuickMagReload()
        {
            RaiseEvent(QuickMagReload);
        }

        internal static void RaiseOnCurrentMagRemoved()
        {
            RaiseEvent(CurrentMagRemoved);
        }

        internal static void RaiseOnNewMagInserted()
        {
            RaiseEvent(NewMagInserted);
        }

        internal static void RaiseOnInternalMagReload()
        {
            RaiseEvent(InternalMagReload);
        }
    }
}
