using EFT;
using HarmonyLib;
using RealismCommonLib.Events;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace RealismCommonLib.Patches
{
    public class ToggleAimPatch : ModulePatch
    {
        private static FieldInfo playerField;
        protected override MethodBase GetTargetMethod()
        {
            playerField = AccessTools.Field(typeof(EFT.Player.ItemHandsController), "_player");
            return typeof(EFT.Player.FirearmController).GetMethod("ToggleAim", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPrefix]
        private static bool Prefix(EFT.Player.FirearmController __instance)
        {
            Player player = (Player)playerField.GetValue(__instance);

            if (player.IsYourPlayer)
            {
                return Plugin.AimStateInstance.PlayerCanAim;
            }
            return true;
        }

        [PatchPostfix]
        private static void Postfix(EFT.Player.FirearmController __instance)
        {
            Player player = (Player)playerField.GetValue(__instance);

            if (player.IsYourPlayer)
            {
                PlayerEvents.RaiseOnAimStateChanged();
            }
        }
    }
}
