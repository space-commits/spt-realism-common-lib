using EFT;
using HarmonyLib;
using RealismCommonLib.Events;
using RealismCommonLib.ModifierHandlers;
using SPT.Reflection.Patching;
using System.Reflection;

namespace RealismCommonLib.Patches
{
    public class ToggleAimPatch : ModulePatch
    {
        private static FieldInfo _playerField;
        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(EFT.Player.ItemHandsController), "_player");
            return typeof(EFT.Player.FirearmController).GetMethod("ToggleAim", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPrefix]
        private static bool Prefix(EFT.Player.FirearmController __instance)
        {
            Player player = (Player)_playerField.GetValue(__instance);

            if (player.IsYourPlayer)
            {
                // Always allow exiting ADS; only block entering ADS
                if (__instance.IsAiming) return true;
                Logger.LogWarning($"Aim blocked: {!Plugin.AimStateInstance.PlayerCanAim}");

                return Plugin.AimStateInstance.PlayerCanAim;
            }
            return true;
        }

        [PatchPostfix]
        private static void Postfix(EFT.Player.FirearmController __instance)
        {
            Player player = (Player)_playerField.GetValue(__instance);

            if (player.IsYourPlayer)
            {
                PlayerEvents.RaiseOnAimStateChanged();
            }
        }
    }
}
