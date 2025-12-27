using EFT.Animations;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using static EFT.Player;

namespace RealismCommonLib.Patches
{
    public class ShootPatch : ModulePatch
    {
        private FieldInfo _fcField;
        private FieldInfo _playerField;

        protected override MethodBase GetTargetMethod()
        {
            _fcField = AccessTools.Field(typeof(ProceduralWeaponAnimation), "_firearmController");
            _playerField = AccessTools.Field(typeof(FirearmController), "_player");
            return typeof(ProceduralWeaponAnimation).GetMethod("Shoot");
        }

        [PatchPostfix]
        public void PatchPostfix(ProceduralWeaponAnimation __instance, float str)
        {
            FirearmController firearmController = (FirearmController)_fcField.GetValue(__instance);
            Player player = (Player)_playerField.GetValue(firearmController);

            if (player != null && player.IsYourPlayer)
            {
                Plugin.FiringStateInstance.SetFiringState();
            }
        }
    }
}
