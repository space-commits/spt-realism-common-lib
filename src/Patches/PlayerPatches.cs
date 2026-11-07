using RealismCommonLib.ModifierHandlers;
using UnityEngine;
using EFT;
using HarmonyLib;
using RealismCommonLib.Events;
using SPT.Reflection.Patching;
using System.Reflection;
using static EFT.Player;
using static RealismCommonLib.Plugin;



namespace RealismCommonLib.Patches
{
    public class InventoryOpenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player).GetMethod("SetInventoryOpened", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void PatchPrefix(Player __instance, bool opened)
        {
            if (__instance.IsYourPlayer)
            {
                AbstractHandsController controller = (AbstractHandsController)AccessTools.Field(typeof(Player), "_handsController").GetValue(__instance);

                if (controller != null)
                {
                    if (opened)
                    {
                        PlayerEvents.RaiseOnInventoryOpened();
                        PlayerStateInstance.IsInInventory = true;
                    }
                    else
                    {
                        PlayerEvents.RaiseOnInventoryClosed();
                        PlayerStateInstance.IsInInventory = false;
                    }
                }
                else
                {
                  PlayerStateInstance.IsInInventory = false;
                }
            }
        }
    }

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
                motion *= multiplier;
            }
            else
            {
                float multiplier = StatModifiers.WalkSpeedModifier.Apply(1f);
                motion *= multiplier;
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
            // else
            //     __result *= StatModifiers.WalkSpeedModifier.Apply(1f);
        }
    }

    public class SetCharacterMovementSpeedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(PlayerAnimator), nameof(PlayerAnimator.SetCharacterMovementSpeed));
        }

        [PatchPrefix]
        private static void Postfix(PlayerAnimator __instance, ref float charaterMovementSpeed)
        {
            if (Plugin.PlayerStateInstance == null) return;
            Player player = Plugin.PlayerStateInstance.Player;
            if (player == null) return;
            if (player.MovementContext.PlayerAnimator != __instance) return;

            charaterMovementSpeed = StatModifiers.WalkSpeedModifier.Apply(charaterMovementSpeed);
        }
    }

    /// <summary>
    /// MovementContext.SetTilt is the single place the player's real tilt is written (clamped to [-5, 5], left is negative).
    /// The lean key (Player.ToggleLean) requests +-5, the incremental lean (Player.SlowLean) creeps towards it in 0.1 steps,
    /// and MovementState.SetTilt may reduce the request (e.g. leaning into a wall) before it gets here, so this sees the
    /// lean that actually happened rather than the input.
    /// </summary>
    public class SetTiltPatch : ModulePatch
    {
        private const float FullLeftTilt = -5f;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MovementContext), nameof(MovementContext.SetTilt));
        }

        [PatchPostfix]
        private static void Postfix(MovementContext __instance)
        {
            if (Plugin.PlayerStateInstance == null) return;
            Player player = Plugin.PlayerStateInstance.Player;
            if (player == null || player.MovementContext != __instance) return;

            bool atFullLeftTilt = Mathf.Approximately(__instance.Tilt, FullLeftTilt);

            if (!Plugin.PlayerStateInstance.IsFullyLeaningLeft)
            {
                // Only the lean key counts, an incremental lean that creeps all the way to -5 does not
                if (!atFullLeftTilt || player.CurrentLeanType != LeanType.NormalLean) return;

                Plugin.PlayerStateInstance.IsFullyLeaningLeft = true;
                PlayerEvents.RaiseOnFullLeftLeanChanged(true);
            }
            else if (!atFullLeftTilt)
            {
                // Anything that moves the tilt off full left ends it: lean key released, lean right, sprint, jump, ...
                Plugin.PlayerStateInstance.IsFullyLeaningLeft = false;
                PlayerEvents.RaiseOnFullLeftLeanChanged(false);
            }
        }
    }

    /// <summary>
    /// MovementContext.EnterMountedState is the single entry into vanilla mounting (called by
    /// WeaponMountingComponent.OnFoundPoint once a point is found), so IsInMountedState is true by the postfix.
    /// </summary>
    public class EnterMountedStatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MovementContext), nameof(MovementContext.EnterMountedState));
        }

        [PatchPostfix]
        private static void Postfix(MovementContext __instance)
        {
            if (Plugin.PlayerStateInstance == null) return;
            Player player = Plugin.PlayerStateInstance.Player;
            if (player == null || player.MovementContext != __instance) return;

            PlayerEvents.RaiseOnMountedStateChanged(true);
        }
    }

    /// <summary>
    /// MovementContext.ExitMountedState returns early when not mounted, and clears the mount point data
    /// (IsInMountedState) when it does exit, so only a true -> false change is reported.
    /// </summary>
    public class ExitMountedStatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MovementContext), nameof(MovementContext.ExitMountedState));
        }

        [PatchPrefix]
        private static void Prefix(MovementContext __instance, out bool __state)
        {
            __state = __instance.IsInMountedState;
        }

        [PatchPostfix]
        private static void Postfix(MovementContext __instance, bool __state)
        {
            if (!__state || __instance.IsInMountedState) return;
            if (Plugin.PlayerStateInstance == null) return;
            Player player = Plugin.PlayerStateInstance.Player;
            if (player == null || player.MovementContext != __instance) return;

            PlayerEvents.RaiseOnMountedStateChanged(false);
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
