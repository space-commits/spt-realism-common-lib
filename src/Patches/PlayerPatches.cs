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

    public class OnWeightUpdatedPatch : ModulePatch
    {
        private static FieldInfo _playerField;

        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(global::Physical), "_player");
            return AccessTools.Method(typeof(global::Physical), nameof(global::Physical.OnWeightUpdated));
        }

        [PatchPostfix]
        private static void PatchPostfix(global::Physical __instance)
        {
            Player player = (Player)_playerField.GetValue(__instance);
            if (player != null && player.IsYourPlayer)
            {
                PlayerEvents.RaiseOnWeightUpdated();
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
    /// MovementContext.SetTilt is where the real tilt gets written (clamped to [-5, 5], left is negative).
    /// MovementState.SetTilt can cut the request down first (e.g. leaning into a wall), so this sees the lean that
    /// actually happened, not the input. Lean key (ToggleLean) asks for +-5, SlowLean creeps there in 0.1 steps.
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
                // lean key only, a slow lean that creeps all the way to -5 doesn't count
                if (!atFullLeftTilt || player.CurrentLeanType != LeanType.NormalLean) return;

                Plugin.PlayerStateInstance.IsFullyLeaningLeft = true;
                PlayerEvents.RaiseOnFullLeftLeanChanged(true);
            }
            else if (!atFullLeftTilt)
            {
                // anything that moves tilt off full left ends it (key released, lean right, sprint, jump...)
                Plugin.PlayerStateInstance.IsFullyLeaningLeft = false;
                PlayerEvents.RaiseOnFullLeftLeanChanged(false);
            }
        }
    }

    /// <summary>
    /// The only way into vanilla mounting (called from WeaponMountingComponent.OnFoundPoint),
    /// so IsInMountedState is already true in the postfix.
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
    /// Returns early when not mounted and clears IsInMountedState when it does exit,
    /// so only a true -> false change gets reported.
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
