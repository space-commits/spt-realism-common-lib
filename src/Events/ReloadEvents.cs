using EFT;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealismCommonLib.Events
{
    public static class ReloadEvents
    {
        //if priority is ever needed, use an int -> Action dictionary and iterate it sorted

        /// <summary>
        /// After a reload/manipulation animation finishes.
        /// </summary>
        public static event Action? WeaponStateReset;

        public static event Action? ChamberCheck;

        public static event Action? CheckAmmo;

        /// <summary>
        /// Bolt or pump cycled.
        /// </summary>
        public static event Action? BoltActionPump;

        public static event Action? MalfFix;

        public static event Action? Rechamber;

        public static event Action? MagReload;

        public static event Action? QuickMagReload;

        /// <summary>
        /// Fires more than once per reload.
        /// </summary>
        public static event Action? InsertingNewMag;

        /// <summary>
        /// Fires more than once per reload.
        /// </summary>
        public static event Action? RemovingCurrentMag;

        public static event Action? InternalMagReload;

        public static event Action? NoMagReload;

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

        internal static void RaiseOnRemovingCurrentMag()
        {
            BaseEventHandler.RaiseEvent(RemovingCurrentMag);
        }

        internal static void RaiseOnInsertingNewMag()
        {
            BaseEventHandler.RaiseEvent(InsertingNewMag);
        }

        internal static void RaiseOnInternalMagReload()
        {
            BaseEventHandler.RaiseEvent(InternalMagReload);
        }

        internal static void RaiseOnNoMagReload()
        {
            BaseEventHandler.RaiseEvent(NoMagReload);
        }
    }
}
