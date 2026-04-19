using EFT.InputSystem;
using RealismCommonLib.Events;
using RealismCommonLib.PatchPipeline;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using InputClass1 = Class1730;
using InputClass2 = Class1728;

namespace RealismCommonLib.Patches
{
    internal static class InputChecker
    {
        private static ECommand? _lastCommand;
        private static long _lastCommandTicks;
        private const int DUPLICATE_THRESHOLD_MS = 15; //TODO: find lowest possible threshold

        public static bool RunInputPipeline(ECommand command)
        {
            var now = Environment.TickCount;

            //both input methods fire when any key is inputted, need to avoid invoking events twice for single input
            bool isDuplicate = _lastCommand == command && (now - _lastCommandTicks) < DUPLICATE_THRESHOLD_MS;
            
            _lastCommand = command;
            _lastCommandTicks = now;

            var inputCtx = new InputContext(command);

            //Don't allow input to do anything at all
            if (Pipelines.InputVetoPipeline.ShouldVeto(inputCtx))
                return false;

            if (!isDuplicate)
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
                        InputEvents.RaiseToggleGogglesInput();
                        break;
                    case ECommand.ChangeScope:
                        InputEvents.RaiseChangeScopeInput();
                        break;
                    case ECommand.ChangeScopeMagnification:
                        InputEvents.RaiseChangeScopeMagnificationInput();
                        break;
                }


            //From user POV original input ran above, but the executed logic is overriden
            if (Pipelines.InputOverridePipeline.ShouldOverride(inputCtx))
            {
                if (!isDuplicate)
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
        private static bool PatchPrefix(InputClass1 __instance, ECommand command)
        {
            return InputChecker.RunInputPipeline(command);
        }
    }
     
    public class KeyInputPatch2 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(InputClass2).GetMethod("TranslateCommand", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static bool PatchPrefix(InputClass2 __instance, ECommand command)
        {
            return InputChecker.RunInputPipeline(command);
        }
    }
}
