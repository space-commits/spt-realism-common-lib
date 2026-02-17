using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using RealismCommonLib.Events;
using RealismCommonLib.ModifierHandlers;
using SPT.Reflection.Patching;
using System.Reflection;
using static EFT.Player.FirearmController;


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
               PlayerEvents.RaiseOnItemAddedOrRemoved();
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
                PlayerEvents.RaiseOnPlayerInit(__instance);
            }
            else 
            {
                //TODO: raise on bot init
            }
        }
    }

    //trigger weapon swap
    public class OnWeaponDrawPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GClass2055).GetMethod("Start", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(GClass2055 __instance)
        {
            if (__instance.Player_0.IsYourPlayer)
            {
                PWAEvents.RaiseOnPWAInitialize(__instance.Player_0);
            }
        }
    }


    public class PlayerWalkSpeedLimitPatch : ModulePatch
    {
        private static FieldInfo playerField;

        protected override MethodBase GetTargetMethod()
        {
            playerField = AccessTools.Field(typeof(MovementContext), "_player");
            return typeof(MovementContext).GetMethod("StateSpeedLimit", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(MovementContext __instance, ref float __result)
        {
            Player player = (Player)playerField.GetValue(__instance);
            if (player.IsYourPlayer)
            {
                __result = StatModifiers.MaxWalkSpeedModifier.Apply(__result);
            }
        }
    }

    public class PlayerSprintSpeedLimitPatch : ModulePatch
    {
        private static FieldInfo playerField;

        protected override MethodBase GetTargetMethod()
        {
            playerField = AccessTools.Field(typeof(MovementContext), "_player");
            return typeof(MovementContext).GetMethod("StateSprintSpeedLimit", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(MovementContext __instance, ref float __result)
        {
            Player player = (Player)playerField.GetValue(__instance);
            if (player.IsYourPlayer)
            {
                __result = StatModifiers.MaxSprintSpeedModifier.Apply(__result);
            }
        }
    }

    public class OnWeightUpdatedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BasePhysicalClass).GetMethod("OnWeightUpdated", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(BasePhysicalClass __instance)
        {
            if (__instance.IobserverToPlayerBridge_0.iPlayer.IsYourPlayer)
            {
                BackendConfigSettingsClass.InertiaSettings inertia = Singleton<BackendConfigSettingsClass>.Instance.Inertia;

                __instance.SprintAcceleration = StatModifiers.SprintAccelModifier.Apply(__instance.SprintAcceleration);
                __instance.PreSprintAcceleration = StatModifiers.PreSprintAccelModifier.Apply(__instance.PreSprintAcceleration);
            }
        }
    }
}
