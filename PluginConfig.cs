using BepInEx.Configuration;
using System;
using UnityEngine;


namespace RealismCommonLib
{
    internal static class PluginConfig
    {
        //dev config options
        public static ConfigEntry<bool> DevMode { get; set; }
        public static ConfigEntry<bool> ZoneDebug { get; set; }
        public static ConfigEntry<String> TargetZone { get; set; }
        public static ConfigEntry<bool> EnableGeneralLogging { get; set; }
        public static ConfigEntry<bool> EnableMedicalLogging { get; set; }
        public static ConfigEntry<bool> EnableReloadLogging { get; set; }
        public static ConfigEntry<bool> EnablePWALogging { get; set; }
        public static ConfigEntry<bool> EnableRecoilLogging { get; set; }
        public static ConfigEntry<bool> EnableBallisticsLogging { get; set; }
        public static ConfigEntry<float> test1 { get; set; }
        public static ConfigEntry<float> test2 { get; set; }
        public static ConfigEntry<float> test3 { get; set; }
        public static ConfigEntry<float> test4 { get; set; }
        public static ConfigEntry<float> test5 { get; set; }
        public static ConfigEntry<float> test6 { get; set; }
        public static ConfigEntry<float> test7 { get; set; }
        public static ConfigEntry<float> test8 { get; set; }
        public static ConfigEntry<float> test9 { get; set; }
        public static ConfigEntry<float> test10 { get; set; }
        public static ConfigEntry<KeyboardShortcut> AddEffectKeybind { get; set; }
        public static ConfigEntry<KeyboardShortcut> AddZone { get; set; }
        public static ConfigEntry<int> AddEffectBodyPart { get; set; }
        public static ConfigEntry<String> AddEffectType { get; set; }

        //TODO: use firing start event, and have modules make their own timers
        public static ConfigEntry<float> FiringResetDelay { get; set; }
        public static ConfigEntry<float> FiringDeafenResetDelay { get; set; }
        public static ConfigEntry<float> FiringVisualResetDelay { get; set; }
        public static ConfigEntry<float> FiringFromStanceResetDelay { get; set; }
        public static ConfigEntry<float> FiringMovementResetDelay { get; set; }

        //reloading
        public static ConfigEntry<float> PistolGlobalAimSpeedModifier { get; set; }
        public static ConfigEntry<float> GlobalAimSpeedModifier { get; set; }
        public static ConfigEntry<float> GlobalReloadSpeedMulti { get; set; }
        public static ConfigEntry<float> GlobalFixSpeedMulti { get; set; }
        public static ConfigEntry<float> GlobalUBGLReloadMulti { get; set; }
        public static ConfigEntry<float> GlobalRechamberSpeedMulti { get; set; }
        public static ConfigEntry<float> GlobalShotgunRackSpeedFactor { get; set; }
        public static ConfigEntry<float> GlobalCheckChamberSpeedMulti { get; set; }
        public static ConfigEntry<float> GlobalCheckChamberShotgunSpeedMulti { get; set; }
        public static ConfigEntry<float> GlobalCheckChamberPistolSpeedMulti { get; set; }
        public static ConfigEntry<float> GlobalCheckAmmoPistolSpeedMulti { get; set; }
        public static ConfigEntry<float> GlobalCheckAmmoMulti { get; set; }
        public static ConfigEntry<float> QuickReloadSpeedMulti { get; set; }
        public static ConfigEntry<float> CurrentMagSpeedMulti { get; set; }
        public static ConfigEntry<float> MagInsertSpeedMulti { get; set; }
        public static ConfigEntry<float> InternalMagReloadMulti { get; set; }
        public static ConfigEntry<float> GlobalBoltSpeedMulti { get; set; }
        public static ConfigEntry<float> RechamberPistolSpeedMulti { get; set; }

        public static void InitConfigBindings(ConfigFile config)
        {
            string dev = "1. Debug and Dev Tools";
            string firingState = "2. Fire State Controller";
            string weaponSpeed = "3. Weaponn Speed Modifiers.";

            test1 = config.Bind<float>(dev, "test 1", 1f, new ConfigDescription("", new AcceptableValueRange<float>(-5000f, 5000f), new ConfigurationManagerAttributes { Order = 170, IsAdvanced = true, Browsable = true }));
            test2 = config.Bind<float>(dev, "test 2", 1f, new ConfigDescription("", new AcceptableValueRange<float>(-5000f, 5000f), new ConfigurationManagerAttributes { Order = 160, IsAdvanced = true, Browsable = true }));
            test3 = config.Bind<float>(dev, "test 3", 1f, new ConfigDescription("", new AcceptableValueRange<float>(-5000f, 5000f), new ConfigurationManagerAttributes { Order = 150, IsAdvanced = true, Browsable = true }));
            test4 = config.Bind<float>(dev, "test 4", 1f, new ConfigDescription("", new AcceptableValueRange<float>(-5000f, 5000f), new ConfigurationManagerAttributes { Order = 140, IsAdvanced = true, Browsable = true }));
            test5 = config.Bind<float>(dev, "test 5", 1f, new ConfigDescription("", new AcceptableValueRange<float>(-5000f, 5000f), new ConfigurationManagerAttributes { Order = 130, IsAdvanced = true, Browsable = true }));
            test6 = config.Bind<float>(dev, "test 6", 1f, new ConfigDescription("", new AcceptableValueRange<float>(-5000f, 5000f), new ConfigurationManagerAttributes { Order = 120, IsAdvanced = true, Browsable = true }));
            test7 = config.Bind<float>(dev, "test 7", 1f, new ConfigDescription("", new AcceptableValueRange<float>(-5000f, 5000f), new ConfigurationManagerAttributes { Order = 110, IsAdvanced = true, Browsable = true }));
            test8 = config.Bind<float>(dev, "test 8", 1f, new ConfigDescription("", new AcceptableValueRange<float>(-5000f, 5000f), new ConfigurationManagerAttributes { Order = 100, IsAdvanced = true, Browsable = true }));
            test9 = config.Bind<float>(dev, "test 9", 1f, new ConfigDescription("", new AcceptableValueRange<float>(-5000f, 5000f), new ConfigurationManagerAttributes { Order = 90, IsAdvanced = true, Browsable = true }));
            test10 = config.Bind<float>(dev, "test 10", 1f, new ConfigDescription("", new AcceptableValueRange<float>(-5000f, 5000f), new ConfigurationManagerAttributes { Order = 80, IsAdvanced = true, Browsable = true }));
            AddZone = config.Bind(dev, "Create Debug Zone", new KeyboardShortcut(KeyCode.None), new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 70, IsAdvanced = true, Browsable = true }));
            TargetZone = config.Bind<string>(dev, "TargetZone", "", new ConfigDescription("DebugZone", null, new ConfigurationManagerAttributes { Order = 65, IsAdvanced = true, Browsable = true }));
            AddEffectType = config.Bind<string>(dev, "Effect Type", "", new ConfigDescription("HeavyBleeding, LightBleeding, Fracture, removeHP, addHP.", null, new ConfigurationManagerAttributes { Order = 60, IsAdvanced = true, Browsable = true }));
            AddEffectBodyPart = config.Bind<int>(dev, "Body Part Index", 1, new ConfigDescription("Head = 0, Chest = 1, Stomach = 2, Letft Arm, Right Arm, Left Leg, Right Leg, Common (whole body)", null, new ConfigurationManagerAttributes { Order = 50, IsAdvanced = true, Browsable = true }));
            AddEffectKeybind = config.Bind(dev, "Add Effect Keybind", new KeyboardShortcut(KeyCode.None), new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 40, IsAdvanced = true, Browsable = true }));
            ZoneDebug = config.Bind<bool>(dev, "Enable Zone Debug", false, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 30, IsAdvanced = true, Browsable = true }));
            DevMode = config.Bind<bool>(dev, "Enable Dev Mode", false, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 29, IsAdvanced = true, Browsable = true }));
            EnableBallisticsLogging = config.Bind<bool>(dev, "Enable Ballistics Logging", false, new ConfigDescription("Enables Logging For Debug And Dev", null, new ConfigurationManagerAttributes { Order = 20, IsAdvanced = true, Browsable = true }));
            EnableGeneralLogging = config.Bind<bool>(dev, "Enable General Logging", false, new ConfigDescription("Enables Logging For Debug And Dev", null, new ConfigurationManagerAttributes { Order = 10, IsAdvanced = true, Browsable = true }));
            EnableMedicalLogging = config.Bind<bool>(dev, "Enable Medical Logging", false, new ConfigDescription("Enables Logging For Debug And Dev", null, new ConfigurationManagerAttributes { Order = 10, IsAdvanced = true, Browsable = true }));
            EnableReloadLogging = config.Bind<bool>(dev, "Enable Reload Logging", false, new ConfigDescription("Enables Logging For Debug And Dev", null, new ConfigurationManagerAttributes { Order = 10, IsAdvanced = true, Browsable = true }));
            EnablePWALogging = config.Bind<bool>(dev, "Enable PWA Logging", false, new ConfigDescription("Enables Logging For Debug And Dev", null, new ConfigurationManagerAttributes { Order = 10, IsAdvanced = true, Browsable = true }));
            EnableRecoilLogging = config.Bind<bool>(dev, "Enable Recoil Logging", false, new ConfigDescription("Enables Logging For Debug And Dev", null, new ConfigurationManagerAttributes { Order = 10, IsAdvanced = true, Browsable = true }));

            FiringResetDelay = config.Bind<float>(firingState, "Firing Reset Delay", 0.14f, new ConfigDescription("The Time In Seconds That Has To Be Elapsed Before Firing Is Considered Over, Used By Different Modules To Time Resets Related To Firing,", new AcceptableValueRange<float>(-0.1f, 0.5f), new ConfigurationManagerAttributes { IsAdvanced = true, Order = 4 }));
            FiringDeafenResetDelay = config.Bind<float>(firingState, "Firing Deafen Reset Delay", 1f, new ConfigDescription("The Time In Seconds That Has To Be Elapsed Before Firing Is Considered Over, Used By Different Modules To Time Resets Related To Firing,", new AcceptableValueRange<float>(-0.1f, 0.5f), new ConfigurationManagerAttributes { IsAdvanced = true, Order = 4 }));
            FiringVisualResetDelay = config.Bind<float>(firingState, "Firing Visual Recoil Reset Delay", 0.12f, new ConfigDescription("The Time In Seconds That Has To Be Elapsed Before Firing Is Considered Over, Used By Different Modules To Time Resets Related To Firing,", new AcceptableValueRange<float>(-0.1f, 0.5f), new ConfigurationManagerAttributes { IsAdvanced = true, Order = 4 }));
            FiringFromStanceResetDelay = config.Bind<float>(firingState, "Firing Stance Reset Delay", 0.55f, new ConfigDescription("The Time In Seconds That Has To Be Elapsed Before Firing Is Considered Over, Used By Different Modules To Time Resets Related To Firing,", new AcceptableValueRange<float>(-0.1f, 0.5f), new ConfigurationManagerAttributes { IsAdvanced = true, Order = 4 }));
            FiringMovementResetDelay = config.Bind<float>(firingState, "Firing Movement Speed Reset Delay", 0.5f, new ConfigDescription("The Time In Seconds That Has To Be Elapsed Before Firing Is Considered Over, Used By Different Modules To Time Resets Related To Firing,", new AcceptableValueRange<float>(-0.1f, 0.5f), new ConfigurationManagerAttributes { IsAdvanced = true, Order = 4 }));

            GlobalAimSpeedModifier = config.Bind<float>(weaponSpeed, "Aim Speed Multi.", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 10.0f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 200 }));
            PistolGlobalAimSpeedModifier = config.Bind<float>(weaponSpeed, "Pistol Aim Speed Multi.", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 10.0f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 100}));
            GlobalReloadSpeedMulti = config.Bind<float>(weaponSpeed, "Magazine Reload Speed Multi", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 10.0f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 15}));
            GlobalFixSpeedMulti = config.Bind<float>(weaponSpeed, "Malfunction Fix Speed Multi", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 10.0f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 14}));
            GlobalUBGLReloadMulti = config.Bind<float>(weaponSpeed, "UBGL Reload Speed Multi", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 10.0f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 13, IsAdvanced = true}));
            RechamberPistolSpeedMulti = config.Bind<float>(weaponSpeed, "Pistol Rechamber Speed Multi", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 10.0f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 12, IsAdvanced = true}));
            GlobalRechamberSpeedMulti = config.Bind<float>(weaponSpeed, "Rechamber Speed Multi", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 10.0f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 11}));
            GlobalBoltSpeedMulti = config.Bind<float>(weaponSpeed, "Bolt Speed Multi", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 10.0f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 10}));
            GlobalShotgunRackSpeedFactor = config.Bind<float>(weaponSpeed, "Shotgun Rack Speed Multi", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 10.0f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 9}));
            GlobalCheckChamberSpeedMulti = config.Bind<float>(weaponSpeed, "Chamber Check Speed Multi", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 10.0f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 8}));
            GlobalCheckChamberShotgunSpeedMulti = config.Bind<float>(weaponSpeed, "Shotgun Chamber Check Speed Multi", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 10.0f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 7, IsAdvanced = true}));
            GlobalCheckChamberPistolSpeedMulti = config.Bind<float>(weaponSpeed, "Pistol Chamber Check Speed Multi", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 10.0f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 6, IsAdvanced = true}));
            GlobalCheckAmmoPistolSpeedMulti = config.Bind<float>(weaponSpeed, "Pistol Check Ammo Speed Multi", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 10.0f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 5, IsAdvanced = true}));
            GlobalCheckAmmoMulti = config.Bind<float>(weaponSpeed, "Check Ammo Speed Multi.", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 10.0f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 4}));
            QuickReloadSpeedMulti = config.Bind<float>(weaponSpeed, "Quick Reload Speed Multi", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 10.0f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 2}));
            InternalMagReloadMulti = config.Bind<float>(weaponSpeed, "Internal Magazine Reload Speed Multi", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 10.0f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 1}));
            CurrentMagSpeedMulti = config.Bind<float>(weaponSpeed, "Remove Magazine Reload Speed Multi", 1f, new ConfigDescription("Speed of removing magazine during reload sequence.", new AcceptableValueRange<float>(0.1f, 10.0f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 1 }));
            MagInsertSpeedMulti = config.Bind<float>(weaponSpeed, "Insert Magazine Reload Speed Multi", 1f, new ConfigDescription("Speed of inserting magazine during reload sequance.", new AcceptableValueRange<float>(0.1f, 10.0f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 1 }));
        }
    }
}
