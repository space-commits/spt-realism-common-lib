using System;
using System.Collections.Generic;
using System.Text;
using EFT;

namespace RealismCommonLib.Events
{
    public static class PlayerEvents
    {
        /// <summary>
        /// Fires when the player is initialized, providing the Player instance as an argument.
        /// </summary>
        public static event Action<Player>? OnPlayerInitArgs;

        /// <summary>
        /// Fires when the player is initialized, does not pass Palyer instance.
        /// </summary>
        public static event Action? OnPlayerInit;

        public static event Action? OnItemAddedOrRemoved;

        public static event Action? AimStateChanged;

        public static event Action? OnShotFired;

        public static event Action? OnWeaponDraw;

        internal static void RaiseOnAimStateChanged()
        {
            BaseEventHandler.RaiseEvent(AimStateChanged);
        }

        internal static void RaiseOnPlayerInit()
        {
            BaseEventHandler.RaiseEvent(OnPlayerInit);
        }

        internal static void RaiseOnPlayerInitArgs(Player player)
        {
            BaseEventHandler.RaiseEvent(OnPlayerInitArgs, player);
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
