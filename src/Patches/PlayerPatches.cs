using Comfort.Common;
using RealismCommonLib.ModifierHandlers;
using UnityEngine;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using RealismCommonLib.Events;
using SPT.Reflection.Patching;
using System.Reflection;

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

    public class TryProceedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.TryProceed));
        }

        [PatchPrefix]
        private static void Prefix(Player __instance, Item item)
        {
            if (!__instance.IsYourPlayer) return;

            PlayerEvents.RaiseOnAnyItemEquipped();
            Logger.LogWarning($"SetItemInHandsPatch: RaiseOnAnyItemEquipped");

            if (item is Weapon weapon)
            {
                if ( Plugin.PlayerStateInstance.PreviouslyEquippedGunId != null && weapon.TemplateId != Plugin.PlayerStateInstance.PreviouslyEquippedGunId)
                {
                    PlayerEvents.RaiseOnWeaponSwap();
                    Logger.LogWarning($"SetItemInHandsPatch: RaiseOnWeaponSwap");
                }      
                else if (Plugin.PlayerStateInstance.PreviouslyEquippedItemId != null && Plugin.PlayerStateInstance.PreviouslyEquippedGunId == null)
                {
                    PlayerEvents.RaiseOnSwappedFromItemToGun();
                    Logger.LogWarning($"SetItemInHandsPatch: RaiseOnSwappedFromItemToGun");
                }

                PlayerEvents.RaiseEquippedWeapon();
                Logger.LogWarning($"SetItemInHandsPatch: RaiseEquippedWeapon");

                Plugin.PlayerStateInstance.PreviouslyEquippedGunId = weapon.TemplateId;
                Plugin.PlayerStateInstance.PreviouslyEquippedItemId = null;
            }
            else
            {
                if (Plugin.PlayerStateInstance.PreviouslyEquippedGunId != null)
                {
                    PlayerEvents.RaiseOnSwappedFromGunToItem();
                    Logger.LogWarning($"SetItemInHandsPatch: RaiseOnSwappedFromGunToItem");
                }

                PlayerEvents.RaiseEquippedNonWeaponItem();
                Logger.LogWarning($"SetItemInHandsPatch: RaiseEquippedNonWeaponItem");

                Plugin.PlayerStateInstance.PreviouslyEquippedGunId = null;
                Plugin.PlayerStateInstance.PreviouslyEquippedItemId = item.TemplateId;
            }

            Logger.LogWarning($"SetItemInHandsPatch: item={item?.TemplateId}, previouslyEquippedItem={Plugin.PlayerStateInstance.PreviouslyEquippedItemId}, previouslyEquippedGun={Plugin.PlayerStateInstance.PreviouslyEquippedGunId}");
        }
    }

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
            if (Plugin.PlayerStateInstance == null) return;
            Player player = Plugin.PlayerStateInstance.Player;
            if (player == null) return;
            if (player.MovementContext.PlayerAnimator != __instance) return;

            speed *= StatModifiers.SprintSpeedModifier.Apply(1f);
        }
    }
}
