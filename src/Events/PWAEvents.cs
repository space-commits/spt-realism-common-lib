using EFT;
using System;

namespace RealismCommonLib.Events
{
    public static class PWAEvents
    {
        public static event Action? OnUpdateWeaponVariables;

        public static event Action? OnPWAInitialize;

        internal static void RaiseOnUpdateWeaponVariables()
        {
            BaseEventHandler.RaiseEvent(OnUpdateWeaponVariables);
        }

        internal static void RaiseOnPWAInitialize()
        {
            BaseEventHandler.RaiseEvent(OnPWAInitialize);
        }
    }
}
