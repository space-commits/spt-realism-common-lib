using System;
using System.Collections.Generic;
using System.Text;
using EFT;

namespace RealismCommonLib.Events
{
    public static class PlayerEvents
    {
        public static event Action<Player>? OnPlayerInit;

        public static event Action? OnItemAddedOrRemoved;

        public static event Action? AimStateChanged;

        public static event Action? OnShotFired;

        public static event Action? OnWeaponDraw;

        internal static void RaiseOnAimStateChanged()
        {
            BaseEventHandler.RaiseEvent(AimStateChanged);
        }

        internal static void RaiseOnPlayerInit(Player player)
        {
            BaseEventHandler.RaiseEvent(OnPlayerInit, player);
        }

        internal static void RaiseOnItemAddedOrRemoved()
        {
            BaseEventHandler.RaiseEvent(OnItemAddedOrRemoved);
        }

        internal static void RaiseOnShotFired()
        {
            BaseEventHandler.RaiseEvent(OnShotFired);
        }

        internal static void RaiseOnWeaponDraw()
        {
            BaseEventHandler.RaiseEvent(OnWeaponDraw);
        }
    }
}
