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

        internal static void RaiseWeaponAnimationStateReset()
        {
            BaseEventHandler.RaiseEvent(WeaponStateReset);
        }

        internal static void RaiseChamberCheck()
        {
            BaseEventHandler.RaiseEvent(ChamberCheck);
        }

        internal static void RaiseCheckAmmo()
        {
            BaseEventHandler.RaiseEvent(CheckAmmo);
        }

        internal static void RaiseBoltActionPump()
        {
            BaseEventHandler.RaiseEvent(BoltActionPump);
        }

        internal static void RaiseInternalReload()
        {
            BaseEventHandler.RaiseEvent(InternalMagReload);
        }

        internal static void RaiseOnSetMalfFix()
        {
            BaseEventHandler.RaiseEvent(MalfFix);
        }

        internal static void RaiseOnRechamber()
        {
            BaseEventHandler.RaiseEvent(Rechamber);
        }

        internal static void RaiseOnMagReload()
        {
            BaseEventHandler.RaiseEvent(MagReload);
        }

        internal static void RaiseOnQuickMagReload()
        {
            BaseEventHandler.RaiseEvent(QuickMagReload);
        }

        internal static void RaiseOnCurrentMagRemoved()
        {
            BaseEventHandler.RaiseEvent(CurrentMagRemoved);
        }

        internal static void RaiseOnNewMagInserted()
        {
            BaseEventHandler.RaiseEvent(NewMagInserted);
        }

        internal static void RaiseOnInternalMagReload()
        {
            BaseEventHandler.RaiseEvent(InternalMagReload);
        }
    }
}
