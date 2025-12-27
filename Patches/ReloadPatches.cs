using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using static EFT.Player;
using UnityEngine;
using RealismCommonLib.Events;
using RealismCommonLib.StateControllers;

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
            if (player.IsYourPlayer && ((Plugin.WeaponStateInstance.IsManuallyOperated || __instance.Item.IsGrenadeLauncher || __instance.Item.IsUnderBarrelDeviceActive)))
            {
                ReloadEvents.RaiseBoltActionReload();
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
                if (PluginConfig.EnableReloadLogging.Value == true) Logger.LogWarning("===Rechamber===");
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
                ReloadEvents.RaiseOnQuickMagReload();

                if (PluginConfig.EnableReloadLogging.Value) Logger.LogWarning("===QuickReloadMag===");
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
                Plugin.ReloadStateInstance.IsAttemptingToReloadInternalMag = true;
                Plugin.ReloadStateInstance.IsAttemptingRevolverReload = true;

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
                Plugin.ReloadStateInstance.IsAttemptingToReloadInternalMag = true;

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
                Plugin.ReloadStateInstance.IsAttemptingToReloadInternalMag = true;

                if (PluginConfig.EnableReloadLogging.Value == true) Logger.LogWarning("===ReloadBarrels===");
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
                Plugin.ReloadStateInstance.IsMagReloading = false;
                Plugin.ReloadStateInstance.IsQuickReloading = false;

                if (PluginConfig.EnableReloadLogging.Value == true) Logger.LogWarning("===OnMagInsertedPatch/method_50===");
            }
        }
    }
}
