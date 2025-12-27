using Comfort.Common;
using EFT;
using EFT.Animations;
using EFT.InventoryLogic;
using RealismCommonLib.Utils;
using SPT.Reflection.Patching;
using System.Numerics;
using System.Reflection;
using UnityEngine;
using static EFT.Player;
using static RealismCommonLib.Plugin;

namespace RealismCommonLib.StateControllers
{
    public static class PlayerConsts 
    {
        public const float AIM_MOVE_SPEED_BASE = 0.5f;

        public const float REMAINING_ARM_STAM_FACTOR = 0.5f;

        public const float REMAINING_ARM_STAM_RELOAD_FACTOR = 0.25f;

        public const float REMAINING_ARM_STAM_RELOAD_FACTOR_MIN = 0.8f;

        public const float COMBINED_STAMINA_FACTOR = 0.35f;

    }

    public class PlayerState: MonoBehaviour
    {
        private Player _player;
        private bool _isScav;

        private TimeInterval _staminaCalcTimer = new TimeInterval(1f);

        public Player Player
        {
            get
            {
                return _player; 
            }
        }

        public Player.FirearmController FirearmController
        {
            get
            { 
                return Player.HandsController as FirearmController;
            }
        }

        public ProceduralWeaponAnimation ProceduralWeaponAnimation
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
              return IsSprinting || (_player.ProceduralWeaponAnimation.Mask & EProceduralAnimationMask.Walking) != (EProceduralAnimationMask)0;
            }
        }

        public bool IsSprinting
        {
            get
            {
                return _player.IsSprintEnabled;
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
                return _player.MovementContext.StationaryWeapon == null;
            }
        }

        public bool IsInventoryOpen 
        {
            get 
            {
                return _player.IsInventoryOpened;
            }
        }

        public float BaseStaminaPerc { get; private set; }

        public float RemainingArmStamFactor { get; private set; }

        public float RemainingArmStamReloadFactor { get; private set; }

        public float CombinedStaminaFactor { get; private set; }

        public float TotalMousePenalty { get; private set; }

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
        }

        void Update() 
        {
           if (_staminaCalcTimer.Ready()) CalcStaminaFactors();
        }

        /// <summary>
        /// Resets the player's procedural weapon animation state, useful to reset sway.
        /// </summary>
        public void ResetProceduralState() 
        {
            _player.ProceduralWeaponAnimation.method_23();
        }

        private void CalcStaminaFactors()
        {
            float remainArmStamPercent = Mathf.Min((_player.Physical.HandsStamina.Current / _player.Physical.HandsStamina.TotalCapacity) * (1f + SkillStateInstance.StrengthSkillAimBuff), 1f);
            BaseStaminaPerc = _player.Physical.Stamina.Current / _player.Physical.Stamina.TotalCapacity;

            RemainingArmStamFactor = Mathf.Pow(remainArmStamPercent, PlayerConsts.REMAINING_ARM_STAM_FACTOR);
            RemainingArmStamReloadFactor = Mathf.Clamp(Mathf.Pow(remainArmStamPercent, PlayerConsts.REMAINING_ARM_STAM_RELOAD_FACTOR), PlayerConsts.REMAINING_ARM_STAM_RELOAD_FACTOR_MIN, 1f);

            CombinedStaminaFactor = Mathf.Pow(remainArmStamPercent * BaseStaminaPerc, PlayerConsts.COMBINED_STAMINA_FACTOR);
        }

        public void CalcPlayerWeightStats(Player player)
        {
            player.Inventory.UpdateTotalWeight();
            float playerWeight = player.Inventory.TotalWeight;
            var weapon = player?.HandsController != null && player?.HandsController?.Item != null ? player.HandsController.Item as Weapon : null;
            float weaponWeight = weapon != null ? weapon.TotalWeight : 0f;
            TotalWeightMinusWeapon = playerWeight - weaponWeight;
            //TotalMousePenalty = (-playerWeight / 10f);
            TotalWeight = playerWeight;

            //This should be part of gear or movement overhaul mods
/*            if (EnableMouseSensPenalty)
            {
                player.RemoveMouseSensitivityModifier(Player.EMouseSensitivityModifier.Armor);
                if (TotalMousePenalty < 0f)
                {
                    player.AddMouseSensitivityModifier(Player.EMouseSensitivityModifier.Armor, TotalMousePenalty / 100f);
                }
            }*/
        }
    }
}
