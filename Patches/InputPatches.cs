using EFT.InputSystem;
using RealismCommonLib.PatchPipeline;
using SPT.Reflection.Patching;
using System.Reflection;
using InputClass1 = Class1730;
using InputClass2 = Class1728;

namespace RealismCommonLib.Patches
{
    internal class InputContextExtensions
    {
        public static bool Process(ECommand command)
        {
            var context = new InputContext
            {
                Command = command
            };

            return Pipelines.InputPipeline.Process(context);
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
            return InputContextExtensions.Process(command);
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
            return InputContextExtensions.Process(command); 
        }
    }
}
