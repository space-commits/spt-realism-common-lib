using EFT;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RealismCommonLib.Patches
{
    public class OnItemAddedOrRemovedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player).GetMethod("OnItemAddedOrRemoved", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(Player __instance)
        {

            if (__instance.IsYourPlayer)
            {
               Plugin.PlayerStateInstance.CalcPlayerWeightStats(__instance);
            }
        }
    }

    public class PlayerInitPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player).GetMethod("Init", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(Player __instance)
        {
            if (__instance.IsYourPlayer)
            {
                Plugin.PlayerStateInstance.CalcPlayerWeightStats(__instance);
            }
        }
    }
}
