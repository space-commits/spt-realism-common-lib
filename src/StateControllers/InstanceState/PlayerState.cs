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
        //TODO move these to modules, controller should just emit what the stamina % is.
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
                return IsSprinting || (Player.ProceduralWeaponAnimation.Mask & EProceduralAnimationMask.Walking) != 0;
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

        public bool IsInventoryOpen
        {
            get
            {
                return Player.IsInventoryOpened;
            }
        }

        internal MongoID? PreviouslyEquippedItemId { get; set; }
        internal MongoID? PreviouslyEquippedGunId { get; set; }
        public float BaseStaminaPerc { get; private set; }

        public float RemainingArmStamFactor { get; private set; }

        public float RemainingArmStamReloadFactor { get; private set; }

        public float CombinedStaminaFactor { get; private set; }

        //this is just set to current player total weight, so it assumes it's been modified elsewhere by comfort stat
        //renamed it to reflect above, in future create dedicated property that factors comfort
        public float TotalWeightMinusWeapon { get; private set; }

        //this is just set to current player total weight, so it assumes it's been modified elsewhere by comfort stat
        //renamed it to reflect above, in future create dedicated property that factors comfort
        public float TotalWeight { get; private set; }

        //this is a config option, probably should be from movement or gear overhaul
        //probably has a server component, setting gear mouse penalties to 0 as it's basd on weight
        //may be a redundant feature if doing dead zone instead
        public bool EnableMouseSensPenalty { get; private set; }

        void Awake()
        {
            IsScav = Singleton<GameWorld>.Instance?.MainPlayer?.Profile?.Info?.Side == EPlayerSide.Savage;

            PreviouslyEquippedItemId = null; //ensures on new player instance, weapon swap is detected and events are raised
            PreviouslyEquippedGunId = null; //ensures on new player instance, weapon swap is detected and events are raised
 
            //component will be initialized after events fired, so do initial calc here
            CalcPlayerWeightStats();

            PlayerEvents.OnItemAddedOrRemoved += CalcPlayerWeightStats;
            PlayerEvents.OnPlayerInit += CalcPlayerWeightStats;
        }

        void Update()
        {
            if (_staminaCalcTimer.Ready()) CalcStaminaFactors();
        }

        void onDestroy()
        {
            PlayerEvents.OnItemAddedOrRemoved -= CalcPlayerWeightStats;
            PlayerEvents.OnPlayerInit -= CalcPlayerWeightStats;
        }

        /// <summary>
        /// Resets the player's procedural weapon animation state, useful to reset sway.
        /// </summary>
        public void ResetProceduralState()
        {
            Player.ProceduralWeaponAnimation.OnAimOrPoseChanged();
        }

        //TODO this should not be opionated, should only calculate the raw % stamina
        //this hould be handled by modules, stances should calculate this itself, common lib just needs to give the raw values or percentages
        //modules can work out factors based on this.
        private void CalcStaminaFactors()
        {
            if (Player || Player.Physical == null) return;

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
            float playerWeight = Player.Inventory.TotalWeight;
            var weapon = Player.HandsController != null && Player?.HandsController?.Item != null ? Player.HandsController.Item as Weapon : null;
            float weaponWeight = weapon != null ? weapon.TotalWeight : 0f;
            TotalWeightMinusWeapon = playerWeight - weaponWeight;
            TotalWeight = playerWeight;
        }

        /// <summary>
        /// Sets MovementContext.SpeedLimitIsDirty to true, which forces speed limit to update. Do not call in update.
        /// </summary>
        // public void UpdateWalkSpeedLimits()
        // {
        //     Player.MovementContext.SpeedLimitIsDirty = true;
        // }
    }
}
