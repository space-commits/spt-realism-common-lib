using BepInEx;
using BepInEx.Logging;
using RealismCommonLib.Patches;
using UnityEngine;
using System.Text;
using RealismCommonLib.StateControllers.InstanceState;
using RealismCommonLib.StateControllers.GlobalState;
using EFT;
using RealismCommonLib.Events;
using EFT.InputSystem;

namespace RealismCommonLib
{
    [BepInPlugin(MOD_GUID, MOD_NAME, MOD_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string MOD_GUID = "com.fontaine.spt-realism-common-lib";
        public const string MOD_NAME = "Fontaine-SPTRealismCommon-Lib";
        public const string MOD_VERSION = "0.0.2";

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

            RealismCommonLib.Utils.CurveDrawer.HandleInput();


            if (Input.GetKeyDown(KeyCode.O))
            {
                Dump(PlayerStateInstance.Player.PlayerBones);
            }
        }

        static BodyPartCollider FindOwner(Transform t)
        {
            for (; t != null; t = t.parent)
            {
                var c = t.GetComponent<BodyPartCollider>();   // works on inactive objects too
                if (c != null) return c;
            }
            return null;
        }

        static string P(Transform t, Transform root)
        {
            var s = t.name;
            for (t = t.parent; t != null && t != root; t = t.parent) s = t.name + "/" + s;
            return s;
        }

        static void Dump(PlayerBones bones)
        {
            var root = bones.transform;
            var sb = new StringBuilder($"== PlayerBones {bones.name} ==\n");
            foreach (var c in bones.GetComponentsInChildren<Collider>(true))
            {
                var own = c.GetComponent<BodyPartCollider>();
                var up = FindOwner(c.transform);
                var plate = up as ArmorPlateCollider;
                sb.AppendLine(string.Join(" | ", new[] {
            P(c.transform, root), c.GetType().Name,
            $"layer={LayerMask.LayerToName(c.gameObject.layer)}{(c.gameObject.layer == LayersMaskController.HitColliderLayer ? "(HIT)" : "")}",
            $"enabled={c.enabled} self={c.gameObject.activeSelf} hier={c.gameObject.activeInHierarchy}",
            own != null ? $"OWN={own.GetType().Name}/{own.BodyPartColliderType}" : "OWN=none",
            up  != null ? $"UP={P(up.transform, root)}/{up.BodyPartColliderType}/{(plate != null ? plate.ArmorPlateColliderType.ToString() : "-")}/isMain={up.Collider == c}/bridge={up.playerBridge != null}" : "UP=none",
            $"inHashSet={bones.BodyPartCollidersHashSet.Contains(c)}",
            c is BoxCollider b ? $"center={b.center} size={b.size}" : "",
            $"lpos={c.transform.localPosition} lrot={c.transform.localEulerAngles} lscale={c.transform.localScale}"
        }));
            }
            ModLogger.LogWarning(sb.ToString());
        }

        void OnGUI()
        {
            RealismCommonLib.Utils.CurveDrawer.DrawGUI();
        }

        void EnablePatches()
        {
            //Input
            new KeyInputPatch1().Enable();
            new KeyInputPatch2().Enable();
            new NVGTogglePatch().Enable();
            new ThermalTogglePatch().Enable();
            new FaceShieldTogglePatch().Enable();
            //new TryProceedPatch().Enable();
            new HandsControllerChangedPatch().Enable();


            //Aiming
            new ToggleAimPatch().Enable();
            new AimingChangedPatch().Enable();

            //Shooting
            new ShootPatch().Enable();

            //Player
            new OnItemAddedOrRemovedPatch().Enable();
            new PlayerInitPatch().Enable();
            new ClampedSpeedPatch().Enable();
            new ApplyMotionPatch().Enable();
            new SetCharacterMovementSpeedPatch().Enable();
            new SetCharacterSprintSpeedPatch().Enable();
            new SetTiltPatch().Enable();
            new InventoryOpenPatch().Enable();

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
            new CanStartReloadPatch().Enable();
            new SetMagTypeCurrentPatch().Enable();
            new SetMagTypeNewPatch().Enable();
            new SetMagInWeaponPatch().Enable();
            new SetSpeedParametersPatch().Enable();

            //Weapon
            new WeaponConstructorPatch().Enable();
        }

        void InstantiateInstanceControllers()
        {
            //global state persists as long as EFT is running.
            InitGlobalComponents(this.gameObject);

            //this handles adding player-scoped components on player init.
            PlayerEvents.OnPlayerInitRef += InitPlayerComponents;
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



