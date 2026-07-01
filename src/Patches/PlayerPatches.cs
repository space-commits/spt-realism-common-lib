using Comfort.Common;
using RealismCommonLib.ModifierHandlers;
using UnityEngine;
using EFT;
using HarmonyLib;
using RealismCommonLib.Events;
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
                PlayerEvents.RaiseOnPlayerInitArgs(__instance);
                PlayerEvents.RaiseOnPlayerInit();
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
                var fc = __instance.Player_0.HandsController as Player.FirearmController;
                if (fc == null) return;
                PWAEvents.RaiseOnPWAInitialize(fc);
            }
        }
    }

    //Affects speed cap, like from debuffs, weight and player selection
    // public class MaxSpeedPatch : ModulePatch
    // {
    //     private static FieldInfo playerField;

    //     protected override MethodBase GetTargetMethod()
    //     {
    //         playerField = AccessTools.Field(typeof(MovementContext), "_player");
    //         return typeof(MovementContext).GetMethod("get_MaxSpeed", BindingFlags.Instance | BindingFlags.Public);
    //     }

    //     [PatchPostfix]
    //     private static void PatchPostfix(MovementContext __instance, ref float __result)
    //     {
    //         Player player = (Player)playerField.GetValue(__instance);
    //         if (player.IsYourPlayer)
    //         {
    //             __result *= StatModifiers.MaxWalkSpeedModifier.Apply(__result);

    //             Logger.LogWarning($"walk speed {__result}, modifers: {StatModifiers.MaxWalkSpeedModifier.Apply(1f)}");
    //         }
    //     }
    // }


    //Doesn't really work
    // public class SprintingSpeedPatch : ModulePatch
    // {
    //     private static FieldInfo playerField;

    //     protected override MethodBase GetTargetMethod()
    //     {
    //         playerField = AccessTools.Field(typeof(MovementContext), "_player");
    //         return typeof(MovementContext).GetMethod("get_SprintingSpeed", BindingFlags.Instance | BindingFlags.Public);
    //     }

    //     [PatchPostfix]
    //     private static void PatchPostfix(MovementContext __instance, ref float __result)
    //     {
    //         Player player = (Player)playerField.GetValue(__instance);
    //         if (player.IsYourPlayer)
    //         {
    //             __result *= StatModifiers.MaxSprintSpeedModifier.Apply(__result);

    //             Logger.LogWarning($"sprint speed {__result}, modifers: {StatModifiers.MaxSprintSpeedModifier.Apply(1f)}");
    //         }
    //     }
    // }


    //Doesn't increase walk speed beyond cap, sprint speed boost is limited
    // public class MovementContextUpdateCharacterControllerSpeedLimitPatch : ModulePatch
    // {
    //     private static FieldInfo playerField;

    //     protected override MethodBase GetTargetMethod()
    //     {
    //         playerField = AccessTools.Field(typeof(MovementContext), "_player");
    //         return typeof(MovementContext).GetMethod(nameof(MovementContext.UpdateCharacterControllerSpeedLimit));
    //     }

    //     // Override only when sprinting
    //     [PatchPostfix]
    //     private static void PatchPostfix(ref MovementContext __instance)
    //     {
    //         Player player = (Player)playerField.GetValue(__instance);
    //         if (!player.IsYourPlayer || __instance.CurrentState is GClass2137) return;

    //         if (__instance.CurrentState.Name == EPlayerState.Sprint)
    //         {

    //         }
    //         else
    //         {

    //         }


    // 		GClass2175 speedLimiter = __instance.SpeedLimiter;
    // 		if (speedLimiter != null)
    // 		{
    // 			speedLimiter.Update(
    //                 __instance.CurrentState.Name, 
    //                 __instance.SkillManager.Strength.SummaryLevel, 
    //                 (__instance.CurrentState.Name == EPlayerState.Sprint) ? PluginConfig.test1.Value : player.Physical.WalkSpeedLimit  * PluginConfig.test2.Value);
    // 		}

    // 		__instance.SetCharacterMovementSpeed(__instance.RelativeSpeed * __instance.MaxSpeed * PluginConfig.test3.Value, false);
    //     }
    // }

    public class ApplyMotionPatch : ModulePatch
    {
        private static FieldInfo _playerField;

        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(MovementContext), "_player");
            return AccessTools.Method(typeof(MovementState), nameof(MovementState.ApplyMotion));
        }

        [PatchPrefix]
        private static void Prefix(MovementState __instance, ref Vector3 motion)
        {
            Player player = (Player)_playerField.GetValue(__instance.MovementContext);
            if (!player.IsYourPlayer) return;

            if (__instance.MovementContext.IsSprintEnabled)
            {
                float multiplier = StatModifiers.SprintSpeedModifier.Apply(1f);
                motion *= multiplier * PluginConfig.test2.Value;
            }
            else
            {
                float multiplier = StatModifiers.WalkSpeedModifier.Apply(1f);
                motion *= multiplier * PluginConfig.test1.Value;
            }

        }
    }

    public class ClampedSpeedPatch : ModulePatch
    {
        private static FieldInfo _playerField;

        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(MovementContext), "_player");
            return AccessTools.PropertyGetter(typeof(MovementContext), "ClampedSpeed");
        }

        [PatchPostfix]
        private static void Postfix(MovementContext __instance, ref float __result)
        {
            Player player = (Player)_playerField.GetValue(__instance);
            if (!player.IsYourPlayer) return;

            if (__instance.IsSprintEnabled)
                __result *= StatModifiers.SprintSpeedModifier.Apply(1f);
            else
                __result *= StatModifiers.WalkSpeedModifier.Apply(1f);
        }
    }

    public class SetCharacterMovementSpeedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(PlayerAnimator), nameof(PlayerAnimator.SetCharacterMovementSpeed));
        }

        [PatchPrefix]
        private static void Prefix(PlayerAnimator __instance, ref float charaterMovementSpeed)
        {
            Logger.LogWarning($">>>>SetCharacterMovementSpeedPatch called, speed: {charaterMovementSpeed}");
            if (Plugin.PlayerStateInstance == null) return;
            Player player = Plugin.PlayerStateInstance.Player;
            if (player == null) return;
            if (player.MovementContext.PlayerAnimator != __instance) return;

            charaterMovementSpeed *= StatModifiers.WalkSpeedModifier.Apply(1f);
        }
    }

    public class SetCharacterSprintSpeedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(PlayerAnimator), nameof(PlayerAnimator.SetCharacterSprintSpeed));
        }

        [PatchPrefix]
        private static void Prefix(PlayerAnimator __instance, ref float speed)
        {
            Logger.LogWarning($">>>>SetCharacterSprintSpeedPatch called, speed: {speed}");
            if (Plugin.PlayerStateInstance == null) return;
            Player player = Plugin.PlayerStateInstance.Player;
            if (player == null) return;
            if (player.MovementContext.PlayerAnimator != __instance) return;

            speed *= StatModifiers.SprintSpeedModifier.Apply(1f);
        }
    }
}
