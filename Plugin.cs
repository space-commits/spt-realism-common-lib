using BepInEx;
using BepInEx.Logging;
using RealismCommonLib.Patches;
using UnityEngine;
using RealismCommonLib.StateControllers.InstanceState;
using RealismCommonLib.StateControllers.GlobalState;
using EFT;
using RealismCommonLib.Events;

namespace RealismCommonLib
{
    [BepInPlugin(MOD_GUID, MOD_NAME, MOD_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string MOD_GUID = "com.fontaine.realismCommonlib";
        public const string MOD_NAME = "Realism-CommonLib";
        public const string MOD_VERSION = "1.0.0";

        //common lib's state and controller instances
        public static GameState GameStateInstance;
        public static PlayerState PlayerStateInstance;
        public static WeaponState WeaponStateInstance;
        public static HealthState HealthStateInstance;
        public static HazardsState HazardsStateInstance;
        public static GearState GearStateInstance;
        public static FiringState FiringStateInstance;
        public static AudioState AudioControllerInstance;
        public static SkillState SkillStateInstance;
        public static ReloadState ReloadStateInstance;
        public static AimingState AimStateInstance;

        //FPS
        private const float FPS_BASELINE = 144f;
        public static float FPS = 0f;
        public static float FPSFactor = 1f;
        private static float _averageFPS = 0f;

        public static ManualLogSource ModLogger;

        void Awake()
        {
            ModLogger = Logger;
            PluginConfig.InitConfigBindings(Config);
            PluginConfig.InitTempBindings(Config);
            PluginRegistry.DetectPlugins();
            EnablePatches();
            InstantiateInstanceControllers();
        }

        void Update()
        {
            GetFps();
            FPSFactor = GetFPSFactor();
        }

        void EnablePatches() 
        {
            //Input
            new KeyInputPatch1().Enable();
            new KeyInputPatch2().Enable();

            //Aiming
            new ToggleAimPatch().Enable();

            //Shooting
            new ShootPatch().Enable();

            //Player
            new OnItemAddedOrRemovedPatch().Enable();
            new PlayerInitPatch().Enable();
            new PlayerWalkSpeedLimitPatch().Enable();
            new PlayerSprintSpeedLimitPatch().Enable();
            new OnWeightUpdatedPatch().Enable();

            //PWA Patches
            new UpdateWeaponVariablesPatch().Enable();
            new PwaWeaponParamsPatch().Enable();

            //Reload
            new SetAnimatorAndProceduralValuesPatch().Enable();
            new CheckAmmoPatch().Enable();
            new CheckChamberPatch().Enable();
            new BoltActionReloadPatch().Enable();
            new SetMalfRepairSpeedPatch().Enable();
            new RechamberPatch().Enable();
            new ReloadMagPatch().Enable();
            new QuickReloadMagPatch().Enable();
            new ReloadCylinderMagazinePatch().Enable();
            new ReloadWithAmmoPatch().Enable();
            new ReloadBarrelsPatch().Enable();
            new OnMagInsertedPatch().Enable();
        }

        void InstantiateInstanceControllers()
        {
            //global state persists as long as EFT is running.
            InitGlobalComponents(this.gameObject);

            //this handles adding player-scoped components on player init.
            PlayerEvents.OnPlayerInitArgs += InitPlayerComponents;
        }

        private void InitGlobalComponents(GameObject go) 
        {
            GameStateInstance = go.gameObject.AddComponent<GameState>();
            AudioControllerInstance = go.gameObject.AddComponent<AudioState>();
        }

        private void InitPlayerComponents(Player player)
        {
            PlayerStateInstance = player.gameObject.AddComponent<PlayerState>();
            PlayerStateInstance.Player = player;

            WeaponStateInstance = player.gameObject.AddComponent<WeaponState>();
            AimStateInstance = player.gameObject.AddComponent<AimingState>();
            HealthStateInstance = player.gameObject.AddComponent<HealthState>();
            HazardsStateInstance = player.gameObject.AddComponent<HazardsState>();
            GearStateInstance = player.gameObject.AddComponent<GearState>();
            FiringStateInstance = player.gameObject.AddComponent<FiringState>();
            SkillStateInstance = player.gameObject.AddComponent<SkillState>();
            ReloadStateInstance = player.gameObject.AddComponent<ReloadState>();
        }

        void GetFps()
        {
            _averageFPS += ((Time.deltaTime / Time.timeScale) - _averageFPS) * 0.035f;
            FPS = (1f / _averageFPS);
            if (float.IsNaN(FPS) || FPS <= 1f) FPS = 144f;
            FPS = Mathf.Clamp(FPS, 30f, 200f);
        }

        float GetFPSFactor()
        {
            float fpsFactor = Plugin.FPS >= 1f ? FPS_BASELINE / Plugin.FPS : 1f;
            fpsFactor = Mathf.Clamp(fpsFactor, 0.05f, 5f);
            return fpsFactor;
        }
    }
}



