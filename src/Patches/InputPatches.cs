using EFT.InputSystem;
using RealismCommonLib.Events;
using RealismCommonLib.PatchPipeline;
using SPT.Reflection.Patching;
using System;
using System.Linq;
using System.Reflection;
using InputClass1 = Class1730;
using InputClass2 = Class1728;
using EFT;
using static RealismCommonLib.Plugin;
using HarmonyLib;


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

        //for commands that aren't going to be vetoed, and need to game's logic to run before raising event.
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

            //Don't allow input to do anything at all
            if (Pipelines.InputVetoPipeline.ShouldVeto(inputCtx))
                return false;

            RaiseEvents(command);

            //From user POV original input ran above, but the executed logic is overriden
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

        //for raising events after game logic runs
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
    /// Patch to detect when the player toggles NVG on or off. Internally it's an asynchronous call when toggling head device,
    /// so need to use these patches to know that they were actually toggled.
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
}
