using BepInEx.Bootstrap;

namespace RealismCommonLib
{
    public static class PluginRegistry
    {
        //Mod detection
        //Lazy, since plugins that depend on the lib aren't in PluginInfos yet during its Awake.
        //Don't read these from any plugin's Awake.
        public static bool AttachmentOverhaulEnabled => false; // Chainloader.PluginInfos.ContainsKey("com.fontaine.attachmentoverhaul");
        public static bool MedicalOverhaulEnabled => false; // Chainloader.PluginInfos.ContainsKey("com.fontaine.medicaloverhaul");
        public static bool StanceOverhaulEnabled => Chainloader.PluginInfos.ContainsKey("com.fontaine.spt-stance-overhaul");
        public static bool RecoilOverhaulEnabled => false; // Chainloader.PluginInfos.ContainsKey("com.fontaine.recoiloverhaul");
        public static bool BallisticsOverhaulEnabled => false; // Chainloader.PluginInfos.ContainsKey("com.fontaine.ballisticsoverhaul");
        public static bool GearOverhaulIsEnabled => false; // Chainloader.PluginInfos.ContainsKey("com.fontaine.gearoverhaul");
        public static bool FOVFixEnabled => Chainloader.PluginInfos.ContainsKey("com.fontaine.fovfix");
        public static bool FikaIsPresent => false; // Chainloader.PluginInfos.ContainsKey("com.fika.fika");
    }
}
