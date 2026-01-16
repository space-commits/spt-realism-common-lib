using EFT;
using RealismCommonLib.StateControllers;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using RealismCommonLib.Controllers;
using EFT.HealthSystem;

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
               Plugin.PlayerStateInstance.CalcPlayerWeightStats();
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


                //method to add state controllers, or one component controller that manages others
                Plugin.GameStateInstance = __instance.gameObject.AddComponent<GameState>(); //should be global and not player specific?
                Plugin.AudioControllerInstance = __instance.gameObject.AddComponent<RealismAudioController>();  //should be global and not player specific?
               
                Plugin.PlayerStateInstance = __instance.gameObject.AddComponent<PlayerState>();
                Plugin.PlayerStateInstance.Player = __instance;

                //weapon state instance needs to update on weapon swap or if player unequps weapon...
                Plugin.WeaponStateInstance = __instance.gameObject.AddComponent<WeaponState>();

                Plugin.HealthStateInstance = __instance.gameObject.AddComponent<HealthState>();
                Plugin.HealthStateInstance._activeHealthController = __instance.ActiveHealthController;

                Plugin.HazardsStateInstance = __instance.gameObject.AddComponent<HazardsState>();
                Plugin.GearStateInstance = __instance.gameObject.AddComponent<GearState>();
                Plugin.FiringStateInstance = __instance.gameObject.AddComponent<FiringState>();
                Plugin.SkillStateInstance = __instance.gameObject.AddComponent<SkillState>();
                Plugin.ReloadStateInstance = __instance.gameObject.AddComponent<ReloadState>();

                //call subscribed mod methods here
            }

            //add components to bots if required
        }
    }
}
