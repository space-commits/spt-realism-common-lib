using EFT;
using HarmonyLib;
using RealismCommonLib.Events;
using SPT.Reflection.Patching;
using System.Reflection;

namespace RealismCommonLib.Patches
{
    public class ToggleAimPatch : ModulePatch
    {
        private static FieldInfo _playerField;
        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(Player.ItemHandsController), "_player");
            return typeof(Player.FirearmController).GetMethod("ToggleAim", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPrefix]
        private static bool Prefix(Player.FirearmController __instance)
        {
            Player player = (Player)_playerField.GetValue(__instance);

            if (player.IsYourPlayer)
            {
                // Always allow exiting ADS; only block entering ADS
                if (__instance.IsAiming) return true;

                return Plugin.AimStateInstance.PlayerCanAim;
            }
            return true;
        }
    }

    // ToggleAim isn't called if using continous/hold for ADS, but is better place to block aiming
    public class SetAimPatch : ModulePatch
    {
        private static FieldInfo _playerField;
        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(EFT.Player.ItemHandsController), "_player");
            return typeof(Player.FirearmController).GetMethod("SetAim", [typeof(bool)]);
        }

        [PatchPostfix]
        private static void Postfix(Player.FirearmController __instance)
        {
            Player player = (Player)_playerField.GetValue(__instance);

            if (player.IsYourPlayer)
            {
                Logger.LogInfo($"SetAimPatch called, IsAiming={__instance.IsAiming}, PlayerCanAim={Plugin.AimStateInstance.PlayerCanAim}");
                PlayerEvents.RaiseOnAimStateChanged();
            }
        }
    }
}
