using EFT.InputSystem;
using RealismCommonLib.Events;
using RealismCommonLib.PatchPipeline;
using SPT.Reflection.Patching;
using System;
using System.Linq;
using System.Reflection;
using InputClass1 = EFT.FirearmHandsInputTranslator;
using InputClass2 = EFT.MoveInputTranslator;
using EFT;
using HarmonyLib;
using EFT.InventoryLogic;
using static RealismCommonLib.Plugin;

namespace RealismCommonLib.Patches
{
    internal static class InputChecker
    {
        public static ECommand[] Input1PrefixCommands = new ECommand[]
        {
            ECommand.SelectFirstPrimaryWeapon,
            ECommand.SelectSecondPrimaryWeapon,
            ECommand.QuickSelectSecondaryWeapon,
            ECommand.ToggleStepLeft,
            ECommand.ToggleStepRight,
            ECommand.ReturnFromRightStep,
            ECommand.ReturnFromLeftStep,
            ECommand.ChamberUnload,
            ECommand.ChangeScope,
            ECommand.ChangeScopeMagnification,
            ECommand.LeftStanceToggle,
            ECommand.ToggleBreathing,
            ECommand.ToggleBipods,
            ECommand.ToggleShooting
        };

        //commands that won't be vetoed and need the game's logic to run before the event is raised
        public static ECommand[] Input1PostfixCommands = new ECommand[]
        {
        };

        public static ECommand[] Input2Commands = new ECommand[]
        {
            ECommand.ToggleProne,
            ECommand.ToggleDuck,
            ECommand.ScrollNext,
            ECommand.ScrollPrevious,
            ECommand.WeaponMounting,
        };

        public static void RaiseEvents(ECommand command)
        {
            switch (command)
            {
                case ECommand.SelectFirstPrimaryWeapon:
                case ECommand.SelectSecondPrimaryWeapon:
                case ECommand.QuickSelectSecondaryWeapon:
                case ECommand.SelectSecondaryWeapon:
                    InputEvents.RaiseWeaponSwapInput();
                    break;
                case ECommand.ToggleStepLeft:
                case ECommand.ToggleStepRight:
                case ECommand.ReturnFromRightStep:
                case ECommand.ReturnFromLeftStep:
                    InputEvents.RaiseToggleStepOutInput();
                    break;
                case ECommand.ToggleProne:
                case ECommand.ToggleDuck:
                    InputEvents.RaiseChangeStanceInput();
                    break;
                case ECommand.LeftStanceToggle:
                    InputEvents.RaiseToggleLeftStanceInput();
                    break;
                case ECommand.ToggleBreathing:
                    InputEvents.RaiseToggleBreathingInput();
                    break;
                case ECommand.ToggleShooting:
                    InputEvents.RaiseToggleShootingInput();
                    break;
                case ECommand.ScrollNext:
                case ECommand.ScrollPrevious:
                    InputEvents.RaiseScrollInput();
                    break;
                case ECommand.WeaponMounting:
                    InputEvents.RaiseWeaponMountingInput();
                    break;
                case ECommand.ToggleGoggles:
                    InputEvents.RaiseToggleHeadDevice();
                    break;
                case ECommand.ChangeScope:
                    InputEvents.RaiseChangeScopeInput();
                    break;
                case ECommand.ChangeScopeMagnification:
                    InputEvents.RaiseChangeScopeMagnificationInput();
                    break;
            }
        }

        public static bool RunPrefixInputPipeline(ECommand command)
        {
            var inputCtx = new InputContext(command);

            //vetoed input does nothing at all, not even raise events
            if (Pipelines.InputVetoPipeline.ShouldVeto(inputCtx))
                return false;

            RaiseEvents(command);

            //as far as listeners know the input went through, but our logic runs instead of the game's
            if (Pipelines.InputOverridePipeline.ShouldOverride(inputCtx))
            {
                Pipelines.InputOverrideHandler.RunOverrides(inputCtx);
                return false;
            }

            return true;
        }
    }

    public class KeyInputPatch1 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(InputClass1).GetMethod("TranslateCommand", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static bool PatchPrefix(ECommand command)
        {
            if (InputChecker.Input1PrefixCommands.Contains(command))
                return InputChecker.RunPrefixInputPipeline(command);

            return true;
        }

        //raises events after the game logic, for Input1PostfixCommands
        // [PatchPostfix]
        // private static void PatchPostfix(ECommand command)
        // {
        //     if (InputChecker.Input1PostfixCommands.Contains(command))
        //         InputChecker.RaiseEvents(command);
        // }
    }

    public class KeyInputPatch2 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(InputClass2).GetMethod("TranslateCommand", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static bool PatchPrefix(ECommand command)
        {
            if (InputChecker.Input2Commands.Contains(command))
                return InputChecker.RunPrefixInputPipeline(command);

            return true;
        }
    }


    /// <summary>
    /// Head device toggles are async, so the toggle sound is the reliable sign the NVG actually switched.
    /// Same idea for the thermal and face shield patches below.
    /// </summary>
    public class NVGTogglePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.PlayNightVisionSound));

        }

        [PatchPostfix]
        private static void PatchPostfix(Player __instance)
        {
            if (!__instance.IsYourPlayer || GameStateInstance == null || !GameStateInstance.PlayerIsInRaidOrHideout)
                return;

            PlayerEvents.RaiseToggleHeadDevice();
        }
    }

    public class ThermalTogglePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.PlayThermalVisionSound));
        }

        [PatchPostfix]
        private static void PatchPostfix(Player __instance)
        {
            if (!__instance.IsYourPlayer || GameStateInstance == null || !GameStateInstance.PlayerIsInRaidOrHideout)
                return;

            PlayerEvents.RaiseToggleHeadDevice();
        }
    }

    public class FaceShieldTogglePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.PlayFaceShieldSound));
        }

        [PatchPostfix]
        private static void PatchPostfix(Player __instance)
        {
            if (!__instance.IsYourPlayer || GameStateInstance == null || !GameStateInstance.PlayerIsInRaidOrHideout)
                return;

            PlayerEvents.RaiseToggleHeadDevice();
        }
    }

    
    public class HandsControllerChangedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.PropertySetter(typeof(Player), nameof(Player.HandsController));
        }

        [PatchPrefix]
        private static void Prefix(Player __instance, Player.AbstractHandsController value)
        {
            if (!__instance.IsYourPlayer) return;

            Item? newItem = value?.Item;
            if (newItem == null) return;

            PlayerEvents.RaiseOnAnyItemEquipped();

            if (newItem is Weapon weapon)
            {
                //item id, not template id: two guns of the same model are still a swap
                if (Plugin.PlayerStateInstance.PreviouslyEquippedGunId != null && weapon.Id != Plugin.PlayerStateInstance.PreviouslyEquippedGunId)
                {
                    PlayerEvents.RaiseOnWeaponSwap();
                }
                else if (Plugin.PlayerStateInstance.PreviouslyEquippedItemId != null && Plugin.PlayerStateInstance.PreviouslyEquippedGunId == null)
                {
                    PlayerEvents.RaiseOnSwappedFromItemToGun();
                }

                PlayerEvents.RaiseEquippedWeapon();

                Plugin.PlayerStateInstance.PreviouslyEquippedGunId = weapon.Id;
                Plugin.PlayerStateInstance.PreviouslyEquippedItemId = null;
            }
            else
            {
                if (Plugin.PlayerStateInstance.PreviouslyEquippedGunId != null)
                {
                    PlayerEvents.RaiseOnSwappedFromGunToItem();
                }

                PlayerEvents.RaiseEquippedNonWeaponItem();

                Plugin.PlayerStateInstance.PreviouslyEquippedGunId = null;
                Plugin.PlayerStateInstance.PreviouslyEquippedItemId = newItem?.TemplateId;
            }
        }

        // The prefix events run before the new controller is assigned, when HandsController is still null
        // (the old one is already dropped). This runs after, for anything that needs the new controller.
        [PatchPostfix]
        private static void Postfix(Player __instance, Player.AbstractHandsController value)
        {
            if (!__instance.IsYourPlayer || value?.Item == null) return;

            PlayerEvents.RaiseOnHandsControllerAssigned();
        }
    }
}
