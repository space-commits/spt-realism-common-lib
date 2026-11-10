using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace RealismCommonLib.Patches
{
    // Dev tool: keeps bots in EBotState.PreActive, the same state a bot spawned without a firearm is stuck in.
    // BotOwner.UpdateManual only activates a PreActive bot (method_10: brain, mover, sensors...) once WeaponManager.IsReady,
    // which only BotWeaponManager.UpdateFirearmsController sets. Skipping UpdateManual while PreActive does the same thing:
    // the bot body stands where it spawned, has no AI, and still takes hits/damage as a normal Player.
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
