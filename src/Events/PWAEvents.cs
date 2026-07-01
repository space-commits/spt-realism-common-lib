using EFT;
using System;

namespace RealismCommonLib.Events
{
    public static class PWAEvents
    {
        public static event Action<Player.FirearmController>? OnUpdateWeaponVariables;

        public static event Action<Player.FirearmController>? OnPWAInitialize;

        internal static void RaiseOnUpdateWeaponVariables(Player.FirearmController fc)
        {
            BaseEventHandler.RaiseEvent(OnUpdateWeaponVariables, fc);
        }

        internal static void RaiseOnPWAInitialize(Player.FirearmController fc)
        {
            BaseEventHandler.RaiseEvent(OnPWAInitialize, fc);
        }
    }
}
