using Comfort.Common;
using EFT;
using EFT.Animations;
using EFT.InventoryLogic;
using HarmonyLib;
using RealismCommonLib.Utils;
using System.Numerics;
using UnityEngine;
using static RealismCommonLib.Plugin;

namespace RealismCommonLib.StateControllers
{
    public class ReloadState: MonoBehaviour
    {
        private TimeInterval _reloadCheckTimer = new TimeInterval(0.1f);

        public bool ReloadAnimationSupportsActiveAim
        {
            get
            {
                return IsInReloadOpertation && !IsAttemptingToReloadInternalMag && !IsAttemptingRevolverReload && !IsQuickReloading;
            }
        }

        public bool IsDoingAnyReloadAction
        {
            get
            {
                return IsInReloadOpertation || IsMagReloading || IsAttemptingToReloadInternalMag || IsQuickReloading || IsAttemptingRevolverReload;
            }
        }

        public bool IsInReloadOpertation { get; private set; }

        public bool IsAttemptingToReloadInternalMag { get; internal set; }

        public bool IsAttemptingRevolverReload { get; internal set; }

        public bool IsQuickReloading { get; internal set; }

        public bool IsMagReloading { get; internal set; }

        void Update() 
        {
            if (_reloadCheckTimer.Ready()) ReloadStateCheck();
        }

        public void ResetReloadSpeed() 
        {
            PlayerStateInstance.Player.HandsAnimator.SetAnimationSpeed(1f);
        }

        public void ReloadStateCheck()
        {
            IsInReloadOpertation = PlayerStateInstance.FirearmController.IsInReloadOperation();

            if (IsInReloadOpertation)
            {
                if (IsAttemptingToReloadInternalMag)
                {
                    //handle stance reload bonuses here

                    if (PluginConfig.EnableReloadLogging.Value == true)
                    {
                        ModLogger.LogWarning("IsAttemptingToReloadInternalMag" );
                    }
                }
            }
            else
            {
                IsAttemptingToReloadInternalMag = false;
                IsAttemptingRevolverReload = false;
            }
        }

    }
}
