using EFT;
using System;

namespace RealismCommonLib.Events
{
    public static class PWAEvents
    {
        internal static event Action? OnUpdateWeaponVariables;
        internal static event Action<Player.FirearmController>? OnUpdateWeaponVariablesWithFc;

        internal static void RaiseOnUpdateWeaponVariables(Player.FirearmController fc)
        {
            BaseEventHandler.RaiseEvent(OnUpdateWeaponVariablesWithFc, fc);
            BaseEventHandler.RaiseEvent(OnUpdateWeaponVariables);
        }
    }
}
