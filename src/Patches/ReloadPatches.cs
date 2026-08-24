using Comfort.Common;
using EFT;
using HarmonyLib;
using RealismCommonLib.StateControllers.InstanceState;
using RealismCommonLib.Events;
using RealismCommonLib.ModifierHandlers;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using UnityEngine;
using static EFT.Player;
using static RealismCommonLib.Plugin;
using WeaponStatSubclass = EFT.Player.FirearmController.GClass2050;
using WeaponEventHandlerClass = EFT.Player.FirearmController.GClass2005;
using MagReloadClass = EFT.Player.FirearmController.GClass2039;

namespace RealismCommonLib.Patches
{
    public class SetAnimatorAndProceduralValuesPatch : ModulePatch
    {
        private static FieldInfo _playerField;

        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(FirearmController), "_player");
            return typeof(EFT.Player.FirearmController).GetMethod("SetAnimatorAndProceduralValues", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(EFT.Player.FirearmController __instance)
        {
            Player player = (Player)_playerField.GetValue(__instance);
            if (player.IsYourPlayer)
            {
                if (PluginConfig.EnableReloadLogging.Value) Logger.LogWarning("SetAnimatorAndProceduralValues");

                ReloadEvents.RaiseWeaponAnimationStateReset();
            }
        }
    }

    public class CheckAmmoPatch : ModulePatch
    {
        private static FieldInfo _playerField;

        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(FirearmController), "_player");
            return typeof(EFT.Player.FirearmController).GetMethod("CheckAmmo", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(EFT.Player.FirearmController __instance)
        {
            Player player = (Player)_playerField.GetValue(__instance);
            if (player.IsYourPlayer)
            {
                if (PluginConfig.EnableReloadLogging.Value) Logger.LogWarning("===CheckAmmo===");

                ReloadEvents.RaiseCheckAmmo();

                float config =
                    WeaponStateInstance.IsPistol ? PluginConfig.GlobalCheckAmmoPistolSpeedMulti.Value :
                    PluginConfig.GlobalCheckAmmoMulti.Value;

                float speed = StatModifiers.CheckAmmoSpeed.Apply(config) * ReloadStateInstance.BaseGameAnimationSpeed;
                speed = Mathf.Clamp(speed, 0.7f, 1.35f);

                __instance.FirearmsAnimator.SetAnimationSpeed(speed);
            }
        }
    }

    public class CheckChamberPatch : ModulePatch
    {
        private static FieldInfo _playerField;

        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(FirearmController), "_player");
            return typeof(EFT.Player.FirearmController).GetMethod("CheckChamber", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void PatchPostfix(EFT.Player.FirearmController __instance)
        {
            Player player = (Player)_playerField.GetValue(__instance);
            if (player == null) return;
            if (player.IsYourPlayer)
            {
                if (PluginConfig.EnableReloadLogging.Value == true) Logger.LogWarning("===CheckChamber===");

                ReloadEvents.RaiseChamberCheck();

                float config =
                    WeaponStateInstance.IsPistol ? PluginConfig.GlobalCheckChamberPistolSpeedMulti.Value :
                    WeaponStateInstance.IsShotgun ? PluginConfig.GlobalCheckChamberShotgunSpeedMulti.Value :
                    PluginConfig.GlobalCheckChamberSpeedMulti.Value;


                float speed = StatModifiers.CheckChamberSpeed.Apply(config) * ReloadStateInstance.BaseGameAnimationSpeed;
                speed = Mathf.Clamp(speed, 0.6f, 1.8f);

                __instance.FirearmsAnimator.SetAnimationSpeed(speed);
            }
        }
    }


    public class BoltActionReloadPatch : ModulePatch
    {
        private static FieldInfo _playerField;

        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(FirearmController), "_player");
            return typeof(Player.FirearmController).GetMethod("InitiateShot", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void PatchPrefix(Player.FirearmController __instance)
        {
            Player player = (Player)_playerField.GetValue(__instance);
            if (player == null || !player.IsYourPlayer) return;
            bool isUBGL = __instance.Item.IsUnderBarrelDeviceActive || __instance.Item.IsGrenadeLauncher;
            if (WeaponStateInstance.IsManuallyOperated || isUBGL)
            {
                if (PluginConfig.EnableReloadLogging.Value) Logger.LogWarning("===SetBoltActionReload===");

                ReloadEvents.RaiseBoltActionPump();

                float config =
                    WeaponStateInstance.IsShotgun ? PluginConfig.GlobalShotgunRackSpeedMulti.Value :
                    isUBGL ? PluginConfig.GlobalUBGLReloadMulti.Value :
                    WeaponStateInstance.IsBoltAction ? PluginConfig.GlobalBoltSpeedMulti.Value :
                    1f;

                float speed = StatModifiers.PumpBoltSpeed.Apply(config) * ReloadStateInstance.BaseGameAnimationSpeed;
                speed = Mathf.Clamp(speed, 0.7f, 1.5f);

                __instance.FirearmsAnimator.SetAnimationSpeed(speed);
            }
        }
    }

    public class SetMalfRepairSpeedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("SetMalfRepairSpeed", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(FirearmsAnimator __instance, float fix)
        {
            Player player = Plugin.PlayerStateInstance.Player;
            if (player == null) return;
            Player.FirearmController fc = player.HandsController as Player.FirearmController;
            if (fc == null) return;
            if (fc.FirearmsAnimator == __instance)
            {
                if (PluginConfig.EnableReloadLogging.Value) Logger.LogWarning("===SetMalfRepairSpeed===");

                ReloadEvents.RaiseOnSetMalfFix();

                float baseSpeed = fix * PluginConfig.GlobalFixSpeedMulti.Value;
                float speed = StatModifiers.MalfFixSpeed.Apply(baseSpeed);
                speed = Mathf.Clamp(speed, 0.75f, 1.18f);

                __instance.SetAnimationSpeed(speed);
            }
        }
    }

    public class RechamberPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("Rechamber");
        }

        [PatchPrefix]
        private static void PatchPrefix(FirearmsAnimator __instance)
        {
            Player player = Plugin.PlayerStateInstance.Player;
            if (player == null) return;
            Player.FirearmController fc = player.HandsController as Player.FirearmController;
            if (fc == null) return;
            if (fc.FirearmsAnimator == __instance)
            {
                if (PluginConfig.EnableReloadLogging.Value == true) Logger.LogWarning("===Rechamber===");

                ReloadEvents.RaiseOnRechamber();

                float config =
                    WeaponStateInstance.IsPistol ? PluginConfig.RechamberPistolSpeedMulti.Value :
                    PluginConfig.GlobalRechamberSpeedMulti.Value;

                float speed = StatModifiers.RechamberSpeed.Apply(config);
                speed = Mathf.Clamp(speed * ReloadStateInstance.BaseGameAnimationSpeed, 0.75f, 1.75f);

                fc.FirearmsAnimator.SetAnimationSpeed(speed);
            }
        }
    }

    public class CanStartReloadPatch : ModulePatch
    {
        private static FieldInfo _playerField;

        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(FirearmController), "_player");
            return typeof(Player.FirearmController).GetMethod("CanStartReload", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(Player.FirearmController __instance, bool __result)
        {
            Player player = (Player)_playerField.GetValue(__instance);
            if (player.IsYourPlayer == true)
            {
                if (__result == true)
                {
                    if (PluginConfig.EnableReloadLogging.Value)
                    {
                        ModLogger.LogWarning("===CanStartReload===");
                        ModLogger.LogWarning($"Current magazine = {__instance.Item.GetCurrentMagazine()?.LocalizedName() ?? "null"}");
                    }

                    var mag = __instance.Item.GetCurrentMagazine();
                    var hasMag = mag != null;

                    if (hasMag && __instance.Weapon.ReloadMode == EFT.InventoryLogic.Weapon.EReloadMode.ExternalMagazine)
                    {
                        ReloadStateInstance.MagReloadState = EMagReloadState.RemovingCurrentMag;
                        ReloadStateInstance.CurrentMagazine = mag;
                        ReloadStateInstance.IsMagReloading = true;
                        ReloadEvents.RaiseOnRemovingCurrentMag();
                    }
                    else
                    {
                        ReloadStateInstance.CurrentMagazine = null;
                        ReloadEvents.RaiseOnNoMagReload();
                    }
                }
            }
        }
    }

    //This is actually called second after SetMagTypeCurrent
    //Reliably called only once so good for entry point
    //The magazine here will be the one being reloaded to
    public class ReloadMagPatch : ModulePatch
    {
        private static FieldInfo _playerField;

        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(FirearmController), "_player");
            return typeof(Player.FirearmController).GetMethod("ReloadMag", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void PatchPrefix(Player.FirearmController __instance, MagazineItemClass magazine)
        {
            Player player = (Player)_playerField.GetValue(__instance);
            if (player.IsYourPlayer)
            {
                if (PluginConfig.EnableReloadLogging.Value)
                {
                    Logger.LogWarning("==ReloadMag Patch==");
                    Logger.LogWarning("magazine = " + magazine.LocalizedName());
                    Logger.LogWarning("magazine weight = " + magazine.TotalWeight);
                }

                ReloadStateInstance.NextMagazine = magazine;
                ReloadStateInstance.IsMagReloading = true;
                ReloadEvents.RaiseOnMagReload();
            }
        }
    }

    public class QuickReloadMagPatch : ModulePatch
    {
        private static FieldInfo _playerField;

        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(FirearmController), "_player");
            return typeof(Player.FirearmController).GetMethod("QuickReloadMag", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void PatchPrefix(Player.FirearmController __instance, MagazineItemClass magazine)
        {
            Player player = (Player)_playerField.GetValue(__instance);
            if (player.IsYourPlayer)
            {
                if (PluginConfig.EnableReloadLogging.Value)
                {
                    Logger.LogWarning("==QuickReloadMag Patch==");
                    Logger.LogWarning("magazine = " + magazine.LocalizedName());
                    Logger.LogWarning("magazine weight = " + magazine.TotalWeight);
                }

                ReloadStateInstance.NextMagazine = magazine;
                ReloadStateInstance.IsQuickReloading = true;
                ReloadStateInstance.IsMagReloading = true;
                ReloadEvents.RaiseOnQuickMagReload();
            }
        }
    }

    //reload speed of current mag
    //always called first when starting reload, even when reloading from no mag
    //will be called twice during process, once at start and once after new mag insterted but before OnMagInsertedPatch
    public class SetMagTypeCurrentPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("SetMagTypeCurrent", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(FirearmsAnimator __instance)
        {
            Player player = PlayerStateInstance.Player;
            if (player == null) return;
            Player.FirearmController fc = player.HandsController as Player.FirearmController;
            if (fc == null) return;
            if (fc.FirearmsAnimator == __instance)
            {
                if (PluginConfig.EnableReloadLogging.Value)
                {
                    Logger.LogWarning("===SetMagTypeCurrent===");
                    Logger.LogWarning("magazine = " + (ReloadStateInstance?.CurrentMagazine?.LocalizedName() ?? "null"));
                    Logger.LogWarning("magazine weight = " + (ReloadStateInstance?.CurrentMagazine?.TotalWeight ?? 0));
                }
            }
        }
    }

    //edge cases for current mag reload
    public class SetMagTypeNewPatch : ModulePatch
    {
        private static FieldInfo _playerField;
        private static FieldInfo faField;

        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(WeaponStatSubclass), "Player_0");
            faField = AccessTools.Field(typeof(WeaponStatSubclass), "FirearmsAnimator_0");
            return typeof(MagReloadClass).GetMethod("Start", new Type[] { typeof(WeaponEventHandlerClass), typeof(Callback) });
        }

        [PatchPostfix]
        private static void PatchPostfix(MagReloadClass __instance)
        {
            Player player = (Player)_playerField.GetValue(__instance);
            if (player.IsYourPlayer)
            {
                if (PluginConfig.EnableReloadLogging.Value == true) Logger.LogWarning("===SetMagTypeNew===");
            }
        }
    }

    //speed of putting mag in weapon
    //called twice, straight after the last call of this patch
    public class SetMagInWeaponPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("SetMagInWeapon", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(FirearmsAnimator __instance)
        {
            Player player = PlayerStateInstance.Player;
            if (player == null) return;
            Player.FirearmController fc = player.HandsController as Player.FirearmController;
            if (fc == null) return;
            if (fc.FirearmsAnimator == __instance)
            {
                if (!ReloadStateInstance.IsMagReloading) return;

                if (PluginConfig.EnableReloadLogging.Value) Logger.LogWarning("===SetMagInWeapon===");

                ReloadEvents.RaiseOnInsertingNewMag(); // will be called multiple times
                ReloadStateInstance.MagReloadState = EMagReloadState.InsertingNewMag;
            }
        }
    }

    //reset mag reload speed
    public class OnMagInsertedPatch : ModulePatch
    {
        private static FieldInfo _playerField;

        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(FirearmController), "_player");
            return typeof(Player.FirearmController).GetMethod("method_50", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(ref Player.FirearmController __instance)
        {
            //to find this again, look for private void method_47(){ this.CurrentOperation.OnMagInsertedToWeapon(); }
            Player player = (Player)_playerField.GetValue(__instance);
            if (player.IsYourPlayer == true)
            {
                if (PluginConfig.EnableReloadLogging.Value == true) Logger.LogWarning("===OnMagInsertedPatch/method_50===");

                ReloadStateInstance.ResetMagazineReloadState();
            }
        }
    }

    public class ReloadCylinderMagazinePatch : ModulePatch
    {
        private static FieldInfo _playerField;

        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(FirearmController), "_player");
            return typeof(Player.FirearmController).GetMethod("ReloadCylinderMagazine", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(Player.FirearmController __instance)
        {
            Player player = (Player)_playerField.GetValue(__instance);
            if (player.IsYourPlayer == true)
            {
                ReloadStateInstance.IsAttemptingToReloadInternalMag = true;
                ReloadStateInstance.IsAttemptingRevolverReload = true;

                ReloadEvents.RaiseOnInternalMagReload();

                if (PluginConfig.EnableReloadLogging.Value) Logger.LogWarning("===ReloadCylinderMagazine===");
            }
        }
    }

    public class ReloadWithAmmoPatch : ModulePatch
    {
        private static FieldInfo _playerField;

        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(FirearmController), "_player");
            return typeof(Player.FirearmController).GetMethod("ReloadWithAmmo", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(Player.FirearmController __instance)
        {
            Player player = (Player)_playerField.GetValue(__instance);
            if (player.IsYourPlayer == true)
            {
                ReloadStateInstance.IsAttemptingToReloadInternalMag = true;

                ReloadEvents.RaiseOnInternalMagReload();

                if (PluginConfig.EnableReloadLogging.Value) Logger.LogWarning("===ReloadWithAmmo===");
            }
        }
    }

    public class ReloadBarrelsPatch : ModulePatch
    {
        private static FieldInfo _playerField;

        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(FirearmController), "_player");
            return typeof(Player.FirearmController).GetMethod("ReloadBarrels", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(Player.FirearmController __instance)
        {
            Player player = (Player)_playerField.GetValue(__instance);
            if (player.IsYourPlayer == true)
            {
                ReloadStateInstance.IsAttemptingToReloadInternalMag = true;

                if (PluginConfig.EnableReloadLogging.Value == true) Logger.LogWarning("===ReloadBarrels===");
            }
        }
    }

    //Reset reload speed 
    public class SetSpeedParametersPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("SetSpeedParameters", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(FirearmsAnimator __instance, float reload)
        {
            Player player = PlayerStateInstance.Player;
            if (player == null) return;
            Player.FirearmController fc = player.HandsController as Player.FirearmController;
            if (fc == null) return;
            if (fc.FirearmsAnimator == __instance)
            {
                ReloadStateInstance.ResetMagazineReloadState();
                ReloadStateInstance.BaseGameAnimationSpeed = reload;

                if (PluginConfig.EnableReloadLogging.Value == true) Logger.LogWarning("===SetSpeedParameters===");
            }
        }
    }
}
