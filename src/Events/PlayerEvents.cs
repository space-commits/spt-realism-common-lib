using System;
using System.Collections.Generic;
using System.Text;
using EFT;

namespace RealismCommonLib.Events
{
    public static class PlayerEvents
    {
        /// <summary>
        /// Player init, with the Player passed in.
        /// </summary>
        public static event Action<Player>? OnPlayerInitRef;

        /// <summary>
        /// Player init, without the Player.
        /// </summary>
        public static event Action? OnPlayerInit;

        public static event Action? OnItemAddedOrRemoved;

        public static event Action? AimStateChanged;

        public static event Action? OnShotFired;

        public static event Action? ToggleHeadDevice;

        /// <summary>
        /// Weapons included.
        /// </summary>
        public static event Action? OnAnyItemEquipped;

        /// <summary>
        /// Raised just before Player.HandsController is assigned. PWA and WeaponState already have the new weapon,
        /// but HandsController (and PlayerState.FirearmController) is still the old one.
        /// Use OnHandsControllerAssigned if you need the new controller.
        /// </summary>
        public static event Action? OnWeaponEquipped;

        /// <summary>
        /// Raised just after Player.HandsController is assigned, for any item not just weapons.
        /// PlayerState.FirearmController is the new controller by now.
        /// </summary>
        public static event Action? OnHandsControllerAssigned;

        internal static void RaiseOnHandsControllerAssigned()
        {
            BaseEventHandler.RaiseEvent(OnHandsControllerAssigned);
        }

        public static event Action? OnWeaponSwap;

        public static event Action? OnSwappedFromGunToItem;

        public static event Action? OnSwappedFromItemToGun;

        public static event Action? OnNonWeaponEquipped;

        public static event Action? OnInventoryOpened;

        public static event Action? OnInventoryClosed;

        /// <summary>
        /// The actual tilt reached or left a full left lean (lean key only, not the incremental lean).
        /// True = now fully leaning left.
        /// </summary>
        public static event Action<bool>? OnFullLeftLeanChanged;

        /// <summary>
        /// Entered or left vanilla weapon mounting (MovementContext.IsInMountedState). True = now mounted.
        /// </summary>
        public static event Action<bool>? OnMountedStateChanged;

        /// <summary>
        /// After EFT recalcs carry weight (Physical.OnWeightUpdated). EFT overwrites inertia, SprintAcceleration
        /// and PreSprintAcceleration there, so anything we write into those has to be re-applied.
        /// </summary>
        public static event Action? OnWeightUpdated;

        internal static void RaiseOnWeightUpdated()
        {
            BaseEventHandler.RaiseEvent(OnWeightUpdated);
        }

        internal static void RaiseOnFullLeftLeanChanged(bool isFullyLeaningLeft)
        {
            BaseEventHandler.RaiseEvent(OnFullLeftLeanChanged, isFullyLeaningLeft);
        }

        internal static void RaiseOnMountedStateChanged(bool isMounted)
        {
            BaseEventHandler.RaiseEvent(OnMountedStateChanged, isMounted);
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
