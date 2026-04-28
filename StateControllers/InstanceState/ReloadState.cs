using Comfort.Common;
using EFT;
using EFT.Animations;
using EFT.InventoryLogic;
using HarmonyLib;
using RealismCommonLib.ModifierHandlers;
using RealismCommonLib.Utils;
using System.Numerics;
using UnityEngine;
using static RealismCommonLib.Plugin;

namespace RealismCommonLib.StateControllers.InstanceState
{
    public class ReloadState : MonoBehaviour
    {
        public const float MIN_INTERNAL_RELOAD_SPEED = 0.7f;
        public const float MAX_INTERNAL_RELOAD_SPEED = 1.5f;
        public const float MAX_MAG_RELOAD_SPEED = 1.4f;
        public const float MAX_QUICK_MAG_RELOAD_SPEED = 1.65f;

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

        public bool IsInReloadOpertation
        {
            get
            {
                return PlayerStateInstance?.FirearmController == null || PlayerStateInstance.FirearmController.IsInReloadOperation();
            }
        }

        public bool IsAttemptingToReloadInternalMag { get; internal set; }

        public bool IsAttemptingRevolverReload { get; internal set; }

        public bool IsQuickReloading { get; internal set; }

        public bool IsMagReloading { get; internal set; }

        public bool NoCurrentMagazineReload { get; internal set; }

        public float MaxMagReloadSpeed
        {
            get
            {
                return IsQuickReloading ? MAX_QUICK_MAG_RELOAD_SPEED : MAX_MAG_RELOAD_SPEED;
            }
        }

        public float NewMagReloadSpeed { get; internal set; }

        public float CurrentMagReloadSpeed { get; internal set; }

        public MagazineItemClass CurrentMagazine { get; internal set; }

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
            //IsInReloadOpertation is a potentially expensive check, try to see if it's needed
            if (IsInReloadOpertation)
            {
                if (IsAttemptingToReloadInternalMag)
                {
                    float config = PluginConfig.InternalMagReloadMulti.Value;
                    float modifier = Mathf.Clamp(StatModifiers.InternalReloadModifier.Apply(config), MIN_INTERNAL_RELOAD_SPEED, MAX_INTERNAL_RELOAD_SPEED);

                    PlayerStateInstance.Player.HandsAnimator.SetAnimationSpeed(config);

                    if (PluginConfig.EnableReloadLogging.Value) ModLogger.LogWarning("IsAttemptingToReloadInternalMag");
                }
            }
            else
            {
                IsAttemptingToReloadInternalMag = false;
                IsAttemptingRevolverReload = false;
            }
        }

        public float GetMagReloadSpeed(float configModifier, FloatMultiplierSet handler)
        {
            float baseSpeed = ReloadStateInstance.GetBaseMagReloadSpeed();
            float modifier = handler.Apply(configModifier);
            float finalSpeed = Mathf.Clamp(modifier * baseSpeed, 0.5f, 1.8f);

            if (PluginConfig.EnableReloadLogging.Value) ModLogger.LogWarning($"Final mag reload speed {finalSpeed}");
            return finalSpeed; //TODO: replace with per-weapon min/max if attachment module is installed?
        }

        public float GetBaseMagReloadSpeed()
        {
            float config = IsQuickReloading ? PluginConfig.QuickReloadSpeedMulti.Value : PluginConfig.GlobalMagReloadSpeedMulti.Value;
            return StatModifiers.MagReloadSpeed.Apply(config);
        }
    }
}
