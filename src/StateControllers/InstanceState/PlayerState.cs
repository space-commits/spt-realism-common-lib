using Comfort.Common;
using EFT;
using EFT.Animations;
using EFT.InventoryLogic;
using RealismCommonLib.Events;
using RealismCommonLib.Utils;
using UnityEngine;
using static EFT.Player;
using static RealismCommonLib.Plugin;

namespace RealismCommonLib.StateControllers.InstanceState
{
    public static class PlayerConsts
    {
        //TODO: move these to the modules, the lib should only expose the stamina %
        public const float AIM_MOVE_SPEED_BASE = 0.5f;

        public const float REMAINING_ARM_STAM_FACTOR = 0.5f;

        public const float REMAINING_ARM_STAM_RELOAD_FACTOR = 0.25f;

        public const float REMAINING_ARM_STAM_RELOAD_FACTOR_MIN = 0.8f;

        public const float COMBINED_STAMINA_FACTOR = 0.35f;

    }

    public class PlayerState : MonoBehaviour
    {
        private bool _isScav;

        private TimeInterval _staminaCalcTimer = new TimeInterval(1f);

        public Player Player { get; set; }

        public bool IsInInventory { get; set; }

        public FirearmController? FirearmController
        {
            get
            {
                return Player.HandsController as FirearmController;
            }
        }

        public bool WeaponIsReady
        {
            get
            {
                return FirearmController != null && FirearmController?.Weapon != null;
            }
        }

        public ProceduralWeaponAnimation PWA
        {
            get
            {
                return Player.ProceduralWeaponAnimation;
            }
        }

        public bool IsMoving
        {
            get
            {
                return IsSprinting || IsWalking;
            }
        }

        public bool IsWalking
        {
            get
            {
                return (Player.ProceduralWeaponAnimation.Mask & EProceduralAnimationMask.Walking) != 0;
            }
        }

        public bool IsSprinting
        {
            get
            {
                return Player.IsSprintEnabled;
            }
        }

        public bool IsScav
        {
            get
            {
                return _isScav;
            }
            private set
            {
                _isScav = value;
            }
        }

        public bool IsUsingStationaryWeapon
        {
            get
            {
                return Player.MovementContext.StationaryWeapon != null;
            }
        }

        public bool IsMounting
        {
            get
            {
                return Player.MovementContext.IsInMountedState;
            }
        }

        /// <summary>
        /// True while the player is fully leaning left via the lean key (not the incremental lean). Set by SetTiltPatch.
        /// </summary>
        public bool IsFullyLeaningLeft { get; internal set; }

        public bool IsInventoryOpen
        {
            get
            {
                return Player.IsInventoryOpened;
            }
        }

        internal MongoID? PreviouslyEquippedItemId { get; set; }
        //instance id, not template id, so two guns of the same model still count as a swap
        internal string? PreviouslyEquippedGunId { get; set; }
        public float BaseStaminaPerc { get; private set; }

        public float RemainingArmStamFactor { get; private set; }

        public float RemainingArmStamReloadFactor { get; private set; }

        public float CombinedStaminaFactor { get; private set; }

        //weapon is read fresh each time so the old one's weight can't linger after a swap
        public float TotalWeightMinusWeapon
        {
            get
            {
                Weapon? weapon = Player != null ? FirearmController?.Item : null;
                return TotalWeight - (weapon != null ? weapon.TotalWeight : 0f);
            }
        }

        //raw inventory weight, assumes comfort has already been applied elsewhere
        //TODO: add a property that factors in comfort itself
        public float TotalWeight { get; private set; }

        //TODO: probably belongs in movement or gear overhaul. Likely needs the server to zero gear mouse penalties,
        //since this is weight based. Might not be needed at all with dead zone.
        public bool EnableMouseSensPenalty { get; private set; }

        void Awake()
        {
            IsScav = Singleton<GameWorld>.Instance?.MainPlayer?.Profile?.Info?.Side == EPlayerSide.Savage;

            //so a new player instance always counts as a weapon swap and raises the events
            PreviouslyEquippedItemId = null;
            PreviouslyEquippedGunId = null;

            //the component is added after the init events fire, so do the first calc here
            CalcPlayerWeightStats();

            PlayerEvents.OnItemAddedOrRemoved += CalcPlayerWeightStats;
            PlayerEvents.OnPlayerInit += CalcPlayerWeightStats;
        }

        void Update()
        {
            if (_staminaCalcTimer.Ready()) CalcStaminaFactors();
        }

        void LateUpdate()
        {
            if (!_hipInaccuracyRefreshRequested) return;
            _hipInaccuracyRefreshRequested = false;

            if (Player != null)
                FirearmController?.UpdateHipInaccuracy();
        }

        private bool _hipInaccuracyRefreshRequested;

        /// <summary>
        /// Queues one FirearmController.UpdateHipInaccuracy for the end of the frame, on whatever weapon is in hands by then.
        /// Use this rather than calling it directly, since one stance/aim change or pick-up hits several handlers that all want a refresh.
        /// </summary>
        public void RequestHipInaccuracyRefresh()
        {
            _hipInaccuracyRefreshRequested = true;
        }

        void OnDestroy()
        {
            PlayerEvents.OnItemAddedOrRemoved -= CalcPlayerWeightStats;
            PlayerEvents.OnPlayerInit -= CalcPlayerWeightStats;
        }

        /// <summary>
        /// Resets the PWA state, handy for killing sway.
        /// </summary>
        public void ResetProceduralState()
        {
            Player.ProceduralWeaponAnimation.OnAimOrPoseChanged();
            RequestHipInaccuracyRefresh();
        }

        //TODO: shouldn't be opinionated. Just expose the raw stamina %, and let modules (e.g. stances) work out their own factors.
        private void CalcStaminaFactors()
        {
            if (Player == null || Player.Physical == null || SkillStateInstance == null) return;

            float remainArmStamPercent = Mathf.Min(Player.Physical.HandsStamina.Current / Player.Physical.HandsStamina.TotalCapacity * (1f + SkillStateInstance.StrengthSkillAimBuff), 1f);
            BaseStaminaPerc = Player.Physical.Stamina.Current / Player.Physical.Stamina.TotalCapacity;

            RemainingArmStamFactor = Mathf.Pow(remainArmStamPercent, PlayerConsts.REMAINING_ARM_STAM_FACTOR);
            RemainingArmStamReloadFactor = Mathf.Clamp(Mathf.Pow(remainArmStamPercent, PlayerConsts.REMAINING_ARM_STAM_RELOAD_FACTOR), PlayerConsts.REMAINING_ARM_STAM_RELOAD_FACTOR_MIN, 1f);

            CombinedStaminaFactor = Mathf.Pow(remainArmStamPercent * BaseStaminaPerc, PlayerConsts.COMBINED_STAMINA_FACTOR);
        }

        private void CalcPlayerWeightStats()
        {
            if (Player?.Inventory == null) return;
            Player.Inventory.UpdateTotalWeight();
            TotalWeight = Player.Inventory.TotalWeight;
        }

        /// <summary>
        /// Forces the speed limit to recalc via MovementContext.SpeedLimitIsDirty. Don't call every frame.
        /// </summary>
        // public void UpdateWalkSpeedLimits()
        // {
        //     Player.MovementContext.SpeedLimitIsDirty = true;
        // }
    }
}
