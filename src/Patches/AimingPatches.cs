using EFT;
using HarmonyLib;
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

    // The IsAiming setter raises AimingChanged on every change. Firearms update PWA.IsAiming first,
    // usable items (rangefinder) after, which ReportAimState handles.
    // Patching SetAim(bool) would miss forced cancels (sprint, jump, inventory, reloads, malfs),
    // since those go through CurrentOperation.SetAiming/IsAiming and never call SetAim.
    public class AimingChangedPatch : ModulePatch
    {
        private static FieldInfo _playerField;
        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(Player.ItemHandsController), "_player");
            return typeof(Player.AbstractHandsController).GetMethod("AimingChanged", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        private static void Postfix(Player.AbstractHandsController __instance, bool newValue)
        {
            if (__instance is not Player.ItemHandsController) return;

            Player player = (Player)_playerField.GetValue(__instance);

            if (player != null && player.IsYourPlayer)
            {
                Plugin.AimStateInstance?.ReportAimState(newValue);
            }
        }
    }
}
