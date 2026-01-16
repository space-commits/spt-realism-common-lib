using EFT;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using EFT.InventoryLogic;
using EFT.Animations;
using static EFT.Player;

namespace RealismCommonLib.StateControllers
{
    public class WeaponState : MonoBehaviour
    {
        //Ergo factor in Realism was used and balanced around 80 being the best possible ergo
        private const float BASE_REALISM_ERGO_FACTOR = 80f;

        private Weapon _weapon;

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

        public Weapon Weapon
        {
            get { return _weapon; }
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
                return _fc != null && _fc.HasBipod && _fc.BipodState;
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
                return !HasShoulderContact &&  (IsMachinePistol || IsPistol);
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
                return _weapon.BoltAction;
            }
        }

        //need to find a mod agnostic way to determine this
        //if shotgunn and firemode is single should work
        public bool IsManuallyOperated
        {
            get
            {
                return IsBoltAction || (IsShotgun && _weapon.SelectedFireMode == Weapon.EFireMode.single);
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
                return _player.ProceduralWeaponAnimation.AimingSpeed;
            }
        }

        //this should be just the base weapon aim speed based on ergonomics, not health state or weapon sight
        //this was calculated differnetly by attachment overhaul, stance speed was balanced around it. If attachment overhaul is present, should set this.
        public float BaseAimSpeed
        {
            get
            {
                return _player.ProceduralWeaponAnimation.AimingSpeed;
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

        public void CheckForShoulderContact()
        {
            _hasShoulderContact = _player.ProceduralWeaponAnimation._shouldMoveWeaponCloser && IsPistol;
        }

        public void UpdateErgoFactor() 
        {
            _ergoFactor = Mathf.Clamp(BASE_REALISM_ERGO_FACTOR - TotalErgo, 1f, BASE_REALISM_ERGO_FACTOR);
        }

        //this needs to be called in an appropriate method call hook like UpdateWeaponVariables or Method_23()
        /// <summary>
        /// When weapon state changes (reload, fold stock, change firemode etc), update relevant weapon state variables.
        /// Used for EFT weapon stats, not custom weapon stats.
        /// </summary>
        public void UpdateWeaponStateOnStateChange()
        {
            TotalWeaponWeight = _weapon.TotalWeight;
            WeaponLength = _fc.Item.CalculateCellSize().X;

            if (!Plugin.AttachmentOverhaulEnabled) 
            {          
                //stats
                TotalErgo = Weapon.ErgonomicsTotal;
                UpdateErgoFactor();

                //state
                CheckForShoulderContact();
            }
        }

        /// <summary>
        /// For immutable weapon properties
        /// </summary>
        public void SetWeaponStateOnAwake() 
        {
            _isPistol = Weapon.WeapClass.ToLower() == "pistol";
            _isShotgun = Weapon.WeapClass.ToLower() == "shotgun";
        }

        void Awake() 
        {
            SetWeaponStateOnAwake();
        }

    }
}
