using Comfort.Common;
using EFT;
using HarmonyLib;
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
                ReloadEvents.RaiseWeaponStateReset();
                if (PluginConfig.EnableReloadLogging.Value) Logger.LogWarning("SetAnimatorAndProceduralValues");
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
                ReloadEvents.RaiseCheckAmmo();

                float config = 
                    WeaponStateInstance.IsPistol ? PluginConfig.GlobalCheckAmmoPistolSpeedMulti.Value : 
                    PluginConfig.GlobalCheckAmmoMulti.Value;

                float modifier = Mathf.Clamp(StatModifiers.CheckAmmoSpeed.Apply(config), 0.7f, 1.35f);

                __instance.FirearmsAnimator.SetAnimationSpeed(modifier);

                if (PluginConfig.EnableReloadLogging.Value) Logger.LogWarning("===CheckAmmo===");
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
                ReloadEvents.RaiseChamberCheck();

                float config = 
                    WeaponStateInstance.IsPistol ? PluginConfig.GlobalCheckChamberPistolSpeedMulti.Value :
                    WeaponStateInstance.IsShotgun ? PluginConfig.GlobalCheckChamberShotgunSpeedMulti.Value : 
                    PluginConfig.GlobalCheckChamberSpeedMulti.Value;

                float modifier = Mathf.Clamp(StatModifiers.CheckChamberSpeed.Apply(config), 0.6f, 1.8f);

                __instance.FirearmsAnimator.SetAnimationSpeed(modifier);

                if (PluginConfig.EnableReloadLogging.Value == true) Logger.LogWarning("===CheckChamber===");
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
            if (WeaponStateInstance.IsManuallyOperated || (isUBGL))
            {
                ReloadEvents.RaiseBoltActionReload();

                float config =
                    WeaponStateInstance.IsShotgun ? PluginConfig.GlobalCheckChamberShotgunSpeedMulti.Value :
                    isUBGL ? PluginConfig.GlobalUBGLReloadMulti.Value :
                    WeaponStateInstance.IsBoltAction ? PluginConfig.GlobalBoltSpeedMulti.Value :
                    1f;

                float maxSpeed =
                    WeaponStateInstance.IsShotgun ? PluginConfig.GlobalCheckChamberShotgunSpeedMulti.Value :
                    isUBGL ? PluginConfig.GlobalUBGLReloadMulti.Value :
                    WeaponStateInstance.IsBoltAction ? PluginConfig.GlobalBoltSpeedMulti.Value :
                    1.4f;

                float modifier = Mathf.Clamp(StatModifiers.CheckChamberSpeed.Apply(config), 0.77f, maxSpeed);

                __instance.FirearmsAnimator.SetAnimationSpeed(modifier);

                if (PluginConfig.EnableReloadLogging.Value) Logger.LogWarning("===SetBoltActionReload===");
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
                ReloadEvents.RaiseOnSetMalfFix();

                float baseModi = fix * PluginConfig.GlobalFixSpeedMulti.Value;

                float modifier = Mathf.Clamp(StatModifiers.MalfFixSpeed.Apply(baseModi), 0.75f, 1.18f);

                __instance.SetAnimationSpeed(modifier);

                if (PluginConfig.EnableReloadLogging.Value) Logger.LogWarning("===SetMalfRepairSpeed===");
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
                ReloadEvents.RaiseOnRechamber();

                float config =
                    WeaponStateInstance.IsPistol ? PluginConfig.RechamberPistolSpeedMulti.Value :
                    PluginConfig.GlobalRechamberSpeedMulti.Value; 

                float modifier = Mathf.Clamp(StatModifiers.RechamberSpeed.Apply(config), 0.75f, 1.75f);

                fc.FirearmsAnimator.SetAnimationSpeed(modifier);

                if (PluginConfig.EnableReloadLogging.Value == true) Logger.LogWarning("===Rechamber===");
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
                    if (__instance.Item.GetCurrentMagazine() == null) ReloadStateInstance.NoCurrentMagazineReload = true;
                    else ReloadStateInstance.NoCurrentMagazineReload = false;
                }
            }
        }
    }

    public class ReloadMagPatch : ModulePatch
    {
        private static FieldInfo _playerField;

        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(FirearmController), "_player");
            return typeof(Player.FirearmController).GetMethod("ReloadMag", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(Player.FirearmController __instance, MagazineItemClass magazine)
        {
            Player player = (Player)_playerField.GetValue(__instance);
            if (player.IsYourPlayer)
            {
                Plugin.ReloadStateInstance.IsMagReloading = true;
                Plugin.ReloadStateInstance.CurrentMagazine = magazine;
                ReloadEvents.RaiseOnMagReload();

                if (PluginConfig.EnableReloadLogging.Value)
                {
                    Logger.LogWarning("ReloadMag Patch");
                    Logger.LogWarning("magazine = " + magazine.LocalizedName());
                    Logger.LogWarning("magazine weight = " + magazine.TotalWeight);
                }
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

        [PatchPostfix]
        private static void PatchPostfix(Player.FirearmController __instance, MagazineItemClass magazine)
        {
            Player player = (Player)_playerField.GetValue(__instance);
            if (player.IsYourPlayer)
            {
                Plugin.ReloadStateInstance.IsQuickReloading = true;
                Plugin.ReloadStateInstance.IsMagReloading = true;
                Plugin.ReloadStateInstance.CurrentMagazine = magazine;
                ReloadEvents.RaiseOnMagReload();
                ReloadEvents.RaiseOnQuickMagReload();

                if (PluginConfig.EnableReloadLogging.Value) Logger.LogWarning("===QuickReloadMag===");
            }
        }
    }

    //reload speed of current mag
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
                ReloadEvents.RaiseOnCurrentMagRemoved();

                float totalSpeed = ReloadStateInstance.GetMagReloadSpeed(PluginConfig.CurrentMagSpeed.Value, StatModifiers.CurrentMagRemoveSpeed);
                __instance.SetAnimationSpeed(totalSpeed);

                if (PluginConfig.EnableReloadLogging.Value) Logger.LogWarning("===SetMagTypeCurrent===");
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
            _playerField = AccessTools.Field(typeof(WeaponStatSubclass), "player_0");
            faField = AccessTools.Field(typeof(WeaponStatSubclass), "firearmsAnimator_0");
            return typeof(MagReloadClass).GetMethod("Start", new Type[] { typeof(WeaponEventHandlerClass), typeof(Callback) });
        }

        [PatchPostfix]
        private static void PatchPostfix(MagReloadClass __instance)
        {
            Player player = (Player)_playerField.GetValue(__instance);
            if (player.IsYourPlayer)
            {
                FirearmsAnimator fa = (FirearmsAnimator)faField.GetValue(__instance);

                //watch if calling this twice caused issues
                ReloadEvents.RaiseOnCurrentMagRemoved();

                float totalSpeed = ReloadStateInstance.GetMagReloadSpeed(PluginConfig.CurrentMagSpeed.Value, StatModifiers.CurrentMagRemoveSpeed);
                fa.SetAnimationSpeed(totalSpeed);

                if (PluginConfig.EnableReloadLogging.Value == true)
                {
                    Logger.LogWarning("===SetMagTypeNew===");
                }
            }

        }
    }

    //speed of putting mag in weapon
    public class SetMagInWeaponPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("SetMagInWeapon", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(FirearmsAnimator __instance)
        {
            if (ReloadStateInstance.IsMagReloading)
            {
                ReloadEvents.RaiseOnNewMagInserted();

                float totalSpeed = ReloadStateInstance.GetMagReloadSpeed(PluginConfig.MagInsertSpeed.Value, StatModifiers.NewMagReloadSpeed);               
                __instance.SetAnimationSpeed(totalSpeed);

                if (PluginConfig.EnableReloadLogging.Value) Logger.LogWarning("===SetMagInWeapon===");
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
        private static void PatchPostfix(FirearmsAnimator __instance)
        {
            Player player = PlayerStateInstance.Player;
            if (player == null) return;
            Player.FirearmController fc = player.HandsController as Player.FirearmController;
            if (fc == null) return;
            if (fc.FirearmsAnimator == __instance)
            {
                ReloadStateInstance.ResetReloadSpeed();

                if (PluginConfig.EnableReloadLogging.Value == true) Logger.LogWarning("===SetSpeedParameters===");
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
                ReloadStateInstance.IsMagReloading = false;
                ReloadStateInstance.IsQuickReloading = false;

                ReloadStateInstance.ResetReloadSpeed();

                if (PluginConfig.EnableReloadLogging.Value == true) Logger.LogWarning("===OnMagInsertedPatch/method_50===");
            }
        }
    }
}
