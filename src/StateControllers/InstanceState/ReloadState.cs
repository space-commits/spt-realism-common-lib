using EFT.InventoryLogic;
using RealismCommonLib.ModifierHandlers;
using RealismCommonLib.Utils;
using UnityEngine;
using static RealismCommonLib.Plugin;

namespace RealismCommonLib.StateControllers.InstanceState
{
    public enum EMagReloadState
    {
        None,
        RemovingCurrentMag,
        InsertingNewMag
    }

    public class ReloadState : MonoBehaviour
    {
        public const float MIN_INTERNAL_RELOAD_SPEED = 0.7f;
        public const float MAX_INTERNAL_RELOAD_SPEED = 1.5f;
        public const float MIN_MAG_RELOAD_SPEED = 0.5f;
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

        public EMagReloadState MagReloadState { get; internal set; }

        public float MaxMagReloadSpeed
        {
            get
            {
                return IsQuickReloading ? MAX_QUICK_MAG_RELOAD_SPEED : MAX_MAG_RELOAD_SPEED;
            }
        }

        public float NewMagReloadSpeed { get; internal set; }

        public float CurrentMagReloadSpeed { get; internal set; }

        public MagazineItemClass? NextMagazine { get; internal set; }
        public MagazineItemClass? CurrentMagazine { get; internal set; }

        public float BaseGameAnimationSpeed { get; internal set; } = 1f;

        void Update()
        {
            if (_reloadCheckTimer.Ready())
            {
                ReloadStateCheck();
            }

        }

        public void ResetMagazineReloadState()
        {
            NextMagazine = null;
            CurrentMagazine = null;
            IsMagReloading = false;
            IsQuickReloading = false;
            MagReloadState = EMagReloadState.None;
            PlayerStateInstance.Player.HandsAnimator.SetAnimationSpeed(1f);
        }

        public void ReloadStateCheck()
        {
            //IsInReloadOpertation is a potentially expensive check, try to see if it's needed
            if (IsInReloadOpertation)
            {
                InternalReloadStateCheck();
                MagazineReloadStateCheck();
            }
            else
            {
                IsAttemptingToReloadInternalMag = false;
                IsAttemptingRevolverReload = false;
            }
        }

        public void InternalReloadStateCheck()
        {
            if (IsAttemptingToReloadInternalMag)
            {
                float speed = StatModifiers.InternalReloadModifier.Apply(PluginConfig.InternalMagReloadMulti.Value) * BaseGameAnimationSpeed;
                speed = Mathf.Clamp(speed, MIN_INTERNAL_RELOAD_SPEED, MAX_INTERNAL_RELOAD_SPEED);

                PlayerStateInstance.Player.HandsAnimator.SetAnimationSpeed(speed);

                if (PluginConfig.EnableReloadLogging.Value)
                    ModLogger.LogWarning($"InternalReloadStateCheck: isAttemptingToReloadInternalMag={IsAttemptingToReloadInternalMag}, speed={speed}");

            }

            if (IsAttemptingRevolverReload)
            {
                float speed = PluginConfig.RevolverReloadMulti.Value;
                speed = Mathf.Clamp(StatModifiers.RevolverReloadModifier.Apply(speed), MIN_INTERNAL_RELOAD_SPEED, MAX_INTERNAL_RELOAD_SPEED);

                PlayerStateInstance.Player.HandsAnimator.SetAnimationSpeed(speed);

                if (PluginConfig.EnableReloadLogging.Value)
                    ModLogger.LogWarning($"RevolverReloadStateCheck: isAttemptingRevolverReload={IsAttemptingRevolverReload}, speed={speed}");
            }
        }

        public void MagazineReloadStateCheck()
        {
            if (IsMagReloading || IsQuickReloading || MagReloadState != EMagReloadState.None)
            {
                float totalSpeedModifier = BaseGameAnimationSpeed;

                if (IsQuickReloading)
                {
                    totalSpeedModifier *= StatModifiers.QuickMagReloadSpeed.Apply(PluginConfig.QuickReloadSpeedMulti.Value);
                }

                if (IsMagReloading)
                {
                    totalSpeedModifier *= StatModifiers.MagReloadSpeed.Apply(PluginConfig.GlobalMagReloadSpeedMulti.Value);
                }

                if (MagReloadState == EMagReloadState.RemovingCurrentMag)
                {
                    totalSpeedModifier *= StatModifiers.CurrentMagRemoveSpeed.Apply(PluginConfig.CurrentMagSpeedMulti.Value);
                }

                if (MagReloadState == EMagReloadState.InsertingNewMag)
                {
                    totalSpeedModifier *= StatModifiers.NewMagReloadSpeed.Apply(PluginConfig.NewMagSpeedMulti.Value);
                }

                totalSpeedModifier = Mathf.Clamp(totalSpeedModifier, MIN_MAG_RELOAD_SPEED, MaxMagReloadSpeed);

                PlayerStateInstance.Player.HandsAnimator.SetAnimationSpeed(totalSpeedModifier);

                if (PluginConfig.EnableReloadLogging.Value)
                    ModLogger.LogWarning($"ReloadStateCheck: isMagReloading={IsMagReloading}, isQuickReloading={IsQuickReloading}, magReloadState={MagReloadState}, totalSpeedModifier={totalSpeedModifier}");

            }
        }
    }
}
