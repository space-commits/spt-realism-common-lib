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
        public static event Action<Player>? OnPlayerInitRef;

        /// <summary>
        /// Fires when the player is initialized, does not pass Palyer instance.
        /// </summary>
        public static event Action? OnPlayerInit;

        public static event Action? OnItemAddedOrRemoved;

        public static event Action? AimStateChanged;

        public static event Action? OnShotFired;

        public static event Action? ToggleHeadDevice;

        /// <summary>
        /// Includes weapons
        /// </summary>
        public static event Action? OnAnyItemEquipped;

        public static event Action? OnWeaponEquipped;

        public static event Action? OnWeaponSwap;

        public static event Action? OnSwappedFromGunToItem;

        public static event Action? OnSwappedFromItemToGun;

        public static event Action? OnNonWeaponEquipped;

        public static event Action? OnInventoryOpened;

        public static event Action? OnInventoryClosed;

        /// <summary>
        /// Fires when the player's actual tilt reaches or leaves a full left lean (the lean key, not the
        /// incremental lean). True = now fully leaning left, false = no longer fully leaning left.
        /// </summary>
        public static event Action<bool>? OnFullLeftLeanChanged;

        internal static void RaiseOnFullLeftLeanChanged(bool isFullyLeaningLeft)
        {
            BaseEventHandler.RaiseEvent(OnFullLeftLeanChanged, isFullyLeaningLeft);
        }

        internal static void RaiseOnInventoryOpened()
        {
            BaseEventHandler.RaiseEvent(OnInventoryOpened);
        }

        internal static void RaiseOnInventoryClosed()
        {
            BaseEventHandler.RaiseEvent(OnInventoryClosed);
        }

        internal static void RaiseOnAimStateChanged()
        {
            BaseEventHandler.RaiseEvent(AimStateChanged);
        }

        internal static void RaiseOnPlayerInit(Player player)
        {
            BaseEventHandler.RaiseEvent(OnPlayerInitRef, player);
            BaseEventHandler.RaiseEvent(OnPlayerInit);
        }

        internal static void RaiseOnItemAddedOrRemoved()
        {
            BaseEventHandler.RaiseEvent(OnItemAddedOrRemoved);
        }

        internal static void RaiseOnShotFired()
        {
            BaseEventHandler.RaiseEvent(OnShotFired);
        }

        internal static void RaiseEquippedWeapon()
        {
            BaseEventHandler.RaiseEvent(OnWeaponEquipped);
        }

        internal static void RaiseToggleHeadDevice()
        {
            BaseEventHandler.RaiseEvent(ToggleHeadDevice);
        }

        internal static void RaiseOnAnyItemEquipped()
        {
            BaseEventHandler.RaiseEvent(OnAnyItemEquipped);
        }

        internal static void RaiseOnWeaponSwap()
        {
            BaseEventHandler.RaiseEvent(OnWeaponSwap);
        }

        internal static void RaiseOnSwappedFromGunToItem()
        {
            BaseEventHandler.RaiseEvent(OnSwappedFromGunToItem);
        }

        internal static void RaiseOnSwappedFromItemToGun()
        {
            BaseEventHandler.RaiseEvent(OnSwappedFromItemToGun);
        }

        internal static void RaiseEquippedNonWeaponItem()
        {
            BaseEventHandler.RaiseEvent(OnNonWeaponEquipped);
        }
    }
}
