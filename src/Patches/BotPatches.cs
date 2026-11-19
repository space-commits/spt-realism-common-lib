using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace RealismCommonLib.Patches
{
    // Dev tool: keeps bots stuck in EBotState.PreActive, like a bot that spawns without a gun.
    // UpdateManual only activates a PreActive bot (brain, mover, sensors...) once WeaponManager.IsReady, so skipping it
    // leaves a body standing at its spawn with no AI that still takes hits like a normal Player.
    // See Realism/docs/Assembly/Bot-Activation.md.
    public class FreezeBotsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotOwner), nameof(BotOwner.UpdateManual));
        }

        [PatchPrefix]
        private static bool PatchPrefix(BotOwner __instance)
        {
            return !(PluginConfig.FreezeBots.Value && __instance.BotState == EBotState.PreActive);
        }
    }
}
