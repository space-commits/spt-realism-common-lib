using EFT;
using EFT.Animations;
using EFT.InventoryLogic;
using HarmonyLib;
using RealismCommonLib.Events;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;
using static EFT.Player;

namespace RealismCommonLib.Patches
{
    public class UpdateWeaponVariablesPatch : ModulePatch
    {
        private static FieldInfo playerField;
        private static FieldInfo fcField;

        protected override MethodBase GetTargetMethod()
        {
            playerField = AccessTools.Field(typeof(FirearmController), "_player");
            fcField = AccessTools.Field(typeof(ProceduralWeaponAnimation), "_firearmController");
            return typeof(EFT.Animations.ProceduralWeaponAnimation).GetMethod("UpdateWeaponVariables", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(EFT.Animations.ProceduralWeaponAnimation __instance)
        {
            FirearmController firearmController = (FirearmController)fcField.GetValue(__instance);
            if (firearmController == null) return;
            Player player = (Player)playerField.GetValue(firearmController);
            if (player != null && player.IsYourPlayer && player.MovementContext.CurrentState.Name != EPlayerState.Stationary)
            {
                PWAEvents.RaiseOnUpdateWeaponVariables();
            }
        }
    }

    public class PwaWeaponParamsPatch : ModulePatch
    {
        private static FieldInfo playerField;
        private static FieldInfo fcField;
        private static FieldInfo float3Field;
        private static bool didAimWiggle = false;

        protected override MethodBase GetTargetMethod()
        {
            float3Field = AccessTools.Field(typeof(Player.FirearmController), "float_3");
            playerField = AccessTools.Field(typeof(FirearmController), "_player");
            fcField = AccessTools.Field(typeof(ProceduralWeaponAnimation), "_firearmController");
            return typeof(EFT.Animations.ProceduralWeaponAnimation).GetMethod("method_23", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(EFT.Animations.ProceduralWeaponAnimation __instance)
        {
            FirearmController firearmController = (FirearmController)fcField.GetValue(__instance);
            if (firearmController == null) return;
            Player player = (Player)playerField.GetValue(firearmController);

            if (player != null && player.IsYourPlayer && player.MovementContext.CurrentState.Name != EPlayerState.Stationary)
            {
                Weapon weapon = firearmController.Weapon;
                if (weapon != null)
                {
                    PWAEvents.RaiseOnPWAInitialize();
                }
            }
        }
    }
}
