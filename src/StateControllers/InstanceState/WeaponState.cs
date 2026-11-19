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
using System.Linq;

namespace RealismCommonLib.StateControllers.InstanceState
{
    public class WeaponState : MonoBehaviour
    {
        //state
        private float _totalWeaponWeight;
        private bool _hasShoulderContact;
        private float _length;

        //class
        private bool _isPistol;
        private bool _isShotgun;
        private bool _treatAsPistol; //pistol or machine pistol with no shoulder contact

        public Action? OnWeaponStateChanged;
        public Action<FirearmController>? OnWeaponStateChangedFc;

        /// <summary>
        /// Fires when TreatAsPistol flips: a pistol/non-pistol swap, or a pistol gaining or losing shoulder contact.
        /// Raised after the other weapon state events, so the rest of the state is already up to date.
        /// </summary>
        public Action? OnTreatAsPistolChanged;

        // last value sent through OnTreatAsPistolChanged
        private bool _lastTreatAsPistol;

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

        //Custom stats, these need attachment overhaul or similar to be set.
        //Not decided yet whether the lib should pull them from the modules or the modules should set them.
        //Pulling means less module code but more coupling.

        public bool IsMachinePistol { get; set; } = false;


        public bool IsBoltAction
        {
            get
            {
                return WeaponInstance != null && WeaponInstance.BoltAction;
            }
        }

        //TODO: find a mod agnostic way to detect this
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

        public Weapon.EReloadMode WeaponReloadType { get ; internal set; }

        //TODO: should be BSG's aim speed only without attachment overhaul, and should still factor in
        //adrenaline, stamina, injuries, weapon stats, sight etc.
        public float TotalFinalAimSpeed
        {
            get
            {
                return PlayerStateInstance.Player.ProceduralWeaponAnimation.AimingSpeed;
            }
        }

        //TODO: should be the ergo-based aim speed only, no health or sight. Attachment overhaul calcs this differently
        //and stance speed is balanced around its value, so it should set this when present.
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

        void Update()
        {
        }

        void Awake()
        {
            PWAEvents.OnUpdateWeaponVariablesWithFc += UpdateWeaponStateOnStateChange;
        }

        void OnDestroy()
        {
            PWAEvents.OnUpdateWeaponVariablesWithFc -= UpdateWeaponStateOnStateChange;
        }

        private void RaiseOnWeaponStateChanged(FirearmController fc)
        {
            //Fc first: its subscribers work out weapon stats (e.g. effective weight) that the parameterless ones read
            BaseEventHandler.RaiseEvent(OnWeaponStateChangedFc, fc);
            BaseEventHandler.RaiseEvent(OnWeaponStateChanged);
        }

        public void CheckForShoulderContact(Weapon weapon)
        {
            bool hasShoulderContact = false;
            IEnumerable<ProtrudableComponent> protrudableComponents = Enumerable.Empty<ProtrudableComponent>();
            if (ItemManipulator.CanFold(weapon, out var foldable))
            {
                if (foldable.FoldedSlot == null)
                {
                    hasShoulderContact |= !foldable.Folded;
                }
                else if (foldable.FoldedSlot.ContainedItem != null)
                {
                    protrudableComponents = ItemExtensions.GetItemComponentsInChildren<ProtrudableComponent>(foldable.FoldedSlot.ContainedItem).ToArray();
                    hasShoulderContact |= !foldable.Folded && protrudableComponents.Any((ProtrudableComponent x) => x.IsProtruding());
                }
            }

            IEnumerable<ProtrudableComponent> remainingProtrudableComponents = ItemExtensions.GetItemComponentsInChildren<ProtrudableComponent>(weapon).Except(protrudableComponents);
            hasShoulderContact |= remainingProtrudableComponents.Any((ProtrudableComponent x) => x.IsProtruding());

            _hasShoulderContact = hasShoulderContact;
        }

        /// <summary>
        /// Runs on any weapon state change (reload, stock fold, fire mode etc).
        /// Use fc here, WeaponInstance can be null or stale at this point.
        /// </summary>
        public void UpdateWeaponStateOnStateChange(FirearmController fc)
        {
            TotalWeaponWeight = fc.Item.TotalWeight;
            WeaponLength = fc.Item.CalculateCellSize().X;

            if (!PluginRegistry.AttachmentOverhaulEnabled)
            {
                TotalErgo = fc.Item.ErgonomicsTotal;
                CheckForShoulderContact(fc.Item);
            }

            //before the events, otherwise subscribers read the previous weapon's class (pistol, shotgun, reload mode)
            SetWeaponImmutableState(fc);

            RaiseOnWeaponStateChanged(fc);

            bool treatAsPistol = TreatAsPistol;
            if (treatAsPistol != _lastTreatAsPistol)
            {
                _lastTreatAsPistol = treatAsPistol;
                BaseEventHandler.RaiseEvent(OnTreatAsPistolChanged);
            }
        }

        /// <summary>
        /// Stuff that never changes for a given weapon (class, reload mode).
        /// Use fc here, WeaponInstance can be null or stale at this point.
        /// </summary>
        public void SetWeaponImmutableState(FirearmController fc)
        {
            _isPistol = fc.Weapon.WeapClass.ToLower() == "pistol";
            _isShotgun = fc.Weapon.WeapClass.ToLower() == "shotgun";
            WeaponReloadType = fc.Weapon.ReloadMode;
        }
    }
}
