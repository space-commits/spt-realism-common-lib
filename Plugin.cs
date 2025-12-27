using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using RealismCommonLib.Controllers;
using RealismCommonLib.StateControllers;
using SPT.Common;
using SPT.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace RealismCommonLib
{
    [BepInPlugin("com.fontaine.commonlib", "Realism-CommonLib", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        //common lib's state and controller instances
        public static GameState GameStateInstance;
        public static PlayerState PlayerStateInstance;
        public static WeaponState WeaponStateInstance;
        public static HealthState HealthStateInstance;
        public static HazardsState HazardsStateInstance;
        public static GearState GearStateInstance;
        public static FiringState FiringStateInstance;
        public static RealismAudioController AudioControllerInstance;
        public static SkillState SkillStateInstance;
        public static ReloadState ReloadStateInstance;

        //Mod detection
        public static bool AttachmentOverhaulEnabled = false;
        public static bool MedicalOverhaulEnabled = false;
        public static bool StanceOverhaulEnabled = false;
        public static bool RecoilOverhaulEnabled = false;
        public static bool BallisticsOverhaulEnabled = false;
        public static bool FOVFixEnabled = false;
        public static bool GearOverhaulIsEnabled = false;
        public static bool FikaIsPresent = false;

        //FPS
        private const float FPS_BASELINE = 144f;
        public static float FPS = 0f;
        public static float FPSFactor = 1f;
        private static float _averageFPS = 0f;

        public static ManualLogSource ModLogger;

        void Awake()
        {
            ModLogger = Logger;
        }

        void Update()
        {
            GetFps();
            FPSFactor = GetFPSFactor();
        }

        private void GetFps()
        {
            _averageFPS += ((Time.deltaTime / Time.timeScale) - _averageFPS) * 0.035f;
            FPS = (1f / _averageFPS);
            if (float.IsNaN(FPS) || FPS <= 1f) FPS = 144f;
            FPS = Mathf.Clamp(FPS, 30f, 200f);
        }

        public float GetFPSFactor()
        {
            float fpsFactor = Plugin.FPS >= 1f ? FPS_BASELINE / Plugin.FPS : 1f;
            fpsFactor = Mathf.Clamp(fpsFactor, 0.05f, 5f);
            return fpsFactor;
        }
    }
}



