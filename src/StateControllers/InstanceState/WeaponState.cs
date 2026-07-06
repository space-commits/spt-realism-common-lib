using EFT;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using EFT.InventoryLogic;
using EFT.Animations;
using static EFT.Player;
using static RealismCommonLib.Plugin;
using RealismCommonLib.Events;

namespace RealismCommonLib.StateControllers.InstanceState
{
    public class WeaponState : MonoBehaviour
    {
        //TODO: replace, refactor stance module to figure this out itself
        //Ergo factor in Realism was used and balanced around 80 being the best possible ergo
        private const float BASE_REALISM_ERGO_FACTOR = 80f;
        //stats
        private float _ergoFactor;

        //state
        private float _totalWeaponWeight;
        private bool _hasShoulderContact;
        private float _length;

        //class
        private bool _isPistol;
        private bool _isShotgun;
        private bool _treatAsPistol; //is pistol class, or related, and does not have stock     

        public Weapon? WeaponInstance
        {
            get 
            {
                if (PlayerStateInstance.FirearmController == null) return null;
                return PlayerStateInstance.FirearmController.Item; 
            }
        }

        public float TotalErgo { get; set; }

        public float TotalWeaponWeight
        {
            get
            {
                return _totalWeaponWeight;
            }
            private set
            {
                _totalWeaponWeight = value;
            }
        }

        public bool BipodIsDeployed
        {
            get
            {
                return 
                    PlayerStateInstance.FirearmController != null && 
                    PlayerStateInstance.FirearmController.HasBipod && 
                    PlayerStateInstance.FirearmController.BipodState;
            }
        }

        public float WeaponLength
        {
            get
            {
                return _length;
            }
            private set
            {
                _length = value;
            }
        }

        public bool IsPistol
        {
            get
            {
                return _isPistol;
            }
            private set
            {
                _isPistol = value;
            }
        }

        public bool IsShotgun
        {
            get
            {
                return _isShotgun;
            }
            private set
            {
                _isShotgun = value;
            }
        }

        public bool TreatAsPistol
        {
            get
            {
                return !HasShoulderContact && (IsMachinePistol || IsPistol);
            }
        }

        public bool HasOptic
        {
            get
            {
                return
                    PlayerStateInstance.Player.ProceduralWeaponAnimation.CurrentScope != null &&
                    PlayerStateInstance.Player.ProceduralWeaponAnimation.CurrentScope.IsOptic;
            }
        }

        //Custom Stats (requires attachment overhaul or others)
        //Should this common lib pull data from the modules, or should the modules set these?
        //Having the lib do it means less code in the modules, but more coupling

        public bool IsMachinePistol { get; set; } = false;


        public bool IsBoltAction
        {
            get
            {
                return WeaponInstance != null && WeaponInstance.BoltAction;
            }
        }

        //need to find a mod agnostic way to determine this
        //if shotgunn and firemode is single should work
        public bool IsManuallyOperated
        {
            get
            {
                return WeaponInstance != null && (IsBoltAction || IsShotgun && WeaponInstance.SelectedFireMode == Weapon.EFireMode.single);
            }
        }

        public bool IsBullpup { get; set; } = false;

        public float StockPosition { get; set; } = 0f;

        public bool HasBayonet { get; set; } = false;

        public bool HasLongMag { get; set; } = false;

        public bool WeaponCanFSADS { get; set; } = false;

        public float TotalAimStabilityModi { get; set; } = 1f;

        public float TotalWeaponHandlingModi { get; set; } = 1f;

        public float BaseMeleeDamage { get; set; } = 0f;

        public float BaseMeleePen { get; set; } = 0f;

        //this should only return BSG aim speed if attahcment overhaul is not present, but should still be factored by health state
        //it needs to factor adrenaline, stamina, injury, weapon stats, weapon sight, etc. as calculated previously
        public float TotalFinalAimSpeed
        {
            get
            {
                return PlayerStateInstance.Player.ProceduralWeaponAnimation.AimingSpeed;
            }
        }

        //this should be just the base weapon aim speed based on ergonomics, not health state or weapon sight
        //this was calculated differnetly by attachment overhaul, stance speed was balanced around it. If attachment overhaul is present, should set this.
        public float BaseAimSpeed
        {
            get
            {
                return PlayerStateInstance.Player.ProceduralWeaponAnimation.AimingSpeed;
            }
        }

        public bool HasShoulderContact
        {
            get
            {
                return _hasShoulderContact;
            }
            set
            {
                _hasShoulderContact = value;
            }
        }

        //TODO: replace, refactor stance module to figure this out itself
        public float ErgoFactor
        {
            get
            {
                return _ergoFactor;
            }
            private set
            {
                _ergoFactor = Mathf.Clamp(value, 1f, BASE_REALISM_ERGO_FACTOR);
            }
        }

        void Update() 
        {
        }

        void Awake()
        {
            PWAEvents.OnUpdateWeaponVariables += UpdateWeaponStateOnStateChange;
            PWAEvents.OnPWAInitialize += UpdateWeaponStateOnStateChange;
            PWAEvents.OnPWAInitialize += SetWeaponImmutableState;
        }

        void Destroy()
        {
            PWAEvents.OnUpdateWeaponVariables -= UpdateWeaponStateOnStateChange;
            PWAEvents.OnPWAInitialize -= SetWeaponImmutableState;
        }

        //TODO: attachment overhaul needs a way to override
        public void CheckForShoulderContact()
        {
            // _shouldMoveWeaponCloser=true means the weapon is compact (folded/no stock) — inverted relative to shoulder contact
            _hasShoulderContact = !PlayerStateInstance.Player.ProceduralWeaponAnimation._shouldMoveWeaponCloser && !IsPistol;
        }

        //TODO: replace, refactor stance module to figure this out itself
        public void UpdateErgoFactor()
        {
            _ergoFactor = Mathf.Clamp(BASE_REALISM_ERGO_FACTOR - TotalErgo, 1f, BASE_REALISM_ERGO_FACTOR);
        }

        /// <summary>
        /// When weapon state changes (reload, fold stock, change firemode etc), update relevant weapon state variables.
        /// Don't use componenet's weapon property as it may be null or not updated when this is called.
        /// </summary>
        public void UpdateWeaponStateOnStateChange(FirearmController fc)
        {  
            TotalWeaponWeight = fc.Item.TotalWeight;
            WeaponLength = fc.Item.CalculateCellSize().X;

            if (!PluginRegistry.AttachmentOverhaulEnabled)
            {
                //stats
                TotalErgo = fc.Item.ErgonomicsTotal;
                UpdateErgoFactor();

                //state
                CheckForShoulderContact();
            }
        }

        /// <summary>
        /// For immutable weapon properties.
        /// Don't use componenet's weapon property as it may be null or not updated when this is called.
        /// </summary>
        public void SetWeaponImmutableState(FirearmController fc)
        {
            _isPistol = fc.Item.WeapClass.ToLower() == "pistol";
            _isShotgun = fc.Item.WeapClass.ToLower() == "shotgun";
        }
    }
}
