using BepInEx.Bootstrap;

namespace RealismCommonLib
{
    public static class PluginRegistry
    {
        //Mod detection
        public static bool AttachmentOverhaulEnabled { get; private set; } = false;
        public static bool MedicalOverhaulEnabled { get; private set; } = false;
        public static bool StanceOverhaulEnabled { get; private set; } = false;
        public static bool RecoilOverhaulEnabled { get; private set; } = false;
        public static bool BallisticsOverhaulEnabled { get; private set; } = false;
        public static bool GearOverhaulIsEnabled { get; private set; } = false;
        public static bool FOVFixEnabled { get; private set; } = false;
        public static bool FikaIsPresent { get; private set; } = false;

        internal static void DetectPlugins()
        {
/*            AttachmentOverhaulEnabled = Chainloader.PluginInfos.ContainsKey("com.fontaine.attachmentoverhaul");
            MedicalOverhaulEnabled = Chainloader.PluginInfos.ContainsKey("com.fontaine.medicaloverhaul");
            StanceOverhaulEnabled = Chainloader.PluginInfos.ContainsKey("com.fontaine.stanceoverhaul");
            RecoilOverhaulEnabled = Chainloader.PluginInfos.ContainsKey("com.fontaine.recoiloverhaul");
            BallisticsOverhaulEnabled = Chainloader.PluginInfos.ContainsKey("com.fontaine.ballisticsoverhaul");
            GearOverhaulIsEnabled = Chainloader.PluginInfos.ContainsKey("com.fontaine.gearoverhaul");*/
            FOVFixEnabled = Chainloader.PluginInfos.ContainsKey("com.fontaine.fovfix");
            //FikaIsPresent = Chainloader.PluginInfos.ContainsKey("com.fika.fika");

            Plugin.ModLogger.LogWarning(FOVFixEnabled
                 ? "FOVFixEnabled detected."
                 : "FOVFixEnabled not detected.");
        }
    }
}
