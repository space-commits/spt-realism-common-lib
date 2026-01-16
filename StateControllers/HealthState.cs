using RealismCommonLib.Utils
using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using UnityEngine;
using EffectClass = EFT.HealthSystem.ActiveHealthController.GClass3008; //DisplayableVariations, OverallDuration, Existing
using ContusionInterface = GInterface352;
using DehydrationInterface = GInterface343;
using ExhaustionInterface = GInterface344;
using FractureInterface = GInterface342;
using HeavyBleedingInterface = GInterface340;
using IntoxicationInterface = GInterface346;
using LethalToxinInterface = GInterface347;
using LightBleedingInterface = GInterface339;
using PainKillerInterface = GInterface358;
using TremorInterface = GInterface361;
using TunnelVisionInterface = GInterface363;

namespace RealismCommonLib.StateControllers
{
    public static class MedProperties
    {
        public static readonly Dictionary<string, Type> EffectTypes = new Dictionary<string, Type>
        {
            { "PainKiller", typeof(PainKillerInterface) },
            { "Tremor", typeof(TremorInterface) },
            { "BrokenBone", typeof(FractureInterface) },
            { "TunnelVision", typeof(TunnelVisionInterface) },
            { "Contusion", typeof(ContusionInterface)  },
            { "HeavyBleeding", typeof(HeavyBleedingInterface) },
            { "LightBleeding", typeof(LightBleedingInterface) },
            { "Dehydration", typeof(DehydrationInterface) },
            { "Exhaustion", typeof(ExhaustionInterface) },
            { "LethalToxin", typeof(IntoxicationInterface) },
            { "Intoxication", typeof(LethalToxinInterface) }
        };
    }


    public class HealthState: MonoBehaviour
    {
        internal ActiveHealthController _activeHealthController;
        private TimeInterval _baseTimeGate = new TimeInterval(5f);

        public List<EBodyPart> PossibleBodyParts = new List<EBodyPart>
        {
            EBodyPart.Head, EBodyPart.Chest, EBodyPart.Stomach, EBodyPart.RightLeg, EBodyPart.LeftLeg, EBodyPart.RightArm, EBodyPart.LeftArm
        };

        public Dictionary<EBodyPart, bool> PartsWithFractures = new Dictionary<EBodyPart, bool>
        {
            { EBodyPart.Head, false },
            { EBodyPart.Chest, false },
            { EBodyPart.Stomach, false },
            { EBodyPart.RightLeg, false },
            { EBodyPart.LeftLeg, false },
            { EBodyPart.RightArm, false },
            { EBodyPart.LeftArm, false }
        };

        public Dictionary<EBodyPart, float> PartsHPPerc = new Dictionary<EBodyPart, float>
        {
            { EBodyPart.Head, 1f },
            { EBodyPart.Chest, 1f },
            { EBodyPart.Stomach, 1f },
            { EBodyPart.RightLeg, 1f },
            { EBodyPart.LeftLeg, 1f },
            { EBodyPart.RightArm, 1f },
            { EBodyPart.LeftArm, 1f }
        };

        //Base State
        public bool HasBlackedArmLeft { get; private set; } = false;

        public bool HasBlackedArmRight { get; private set; } = false;

        public bool HasBlackedArm { get; private set; } = false;

        public bool HasFracturedArmLeft { get; private set; } = false;

        public bool HasFracturedArmRight { get; private set; } = false;

        public bool HasFracturedArm
        {
            get
            {
                return HasFracturedArmLeft || HasFracturedArmRight;
            }
        }

        public bool HasRuinedArm
        {
            get
            {
                return HasBlackedArm || HasFracturedArm;
            }
        }

        public bool IsOnPainKiller { get; private set; } = false;

        public bool ArmsAreIncapacitated
        {
            get
            {
                return (HasRuinedArm || HasFracturedArm) && !IsOnPainKiller;
            }
        }
        public bool IsDehydrated { get; set; } = false;

        public bool IsExhausted { get; set; } = false;

        //Custom State

        public float AdrenalineStanceBonus { get; set; } = 1f;

        public bool HasOverdosed { get; set; } = false;

        public bool IsPoisoned { get; set; } = false;

        public float HealthStamRegenFactor { get; set; } = 1f;

        public float StanceInjuryMulti  { get; set; } = 1f;

        void Update() 
        {
            if (_baseTimeGate.Ready())
            {
                CheckHealthAndStatusEffects();

            }
        }

        private Type GetStatusEffectType (string effectName)
        {
            Type effectType;
            MedProperties.EffectTypes.TryGetValue(effectName, out effectType);
            return effectType;
        }

        private void CheckHealthAndStatusEffects()
        {
            Type fractureType = GetStatusEffectType("BrokenBone");
            
            foreach (EBodyPart part in PossibleBodyParts) 
            {
                IEnumerable<IEffect> effects = _player.ActiveHealthController.GetAllActiveEffects(part);
                PartsWithFractures[part] = effects.Any(e => e.Type == fractureType);
                PartsHPPerc[part] = _player.ActiveHealthController.GetBodyPartHealth(part).Current / _player.ActiveHealthController.GetBodyPartHealth(part).Maximum;
            }

            HasBlackedArmLeft = PartsHPPerc[EBodyPart.LeftArm] <= 0f;
            HasBlackedArmRight = PartsHPPerc[EBodyPart.RightArm] <= 0f;
            HasFracturedArmLeft = PartsWithFractures[EBodyPart.LeftArm];
            HasFracturedArmRight = PartsWithFractures[EBodyPart.RightArm];
            IsOnPainKiller = HasBaseEFTEffect(_player, "PainKiller");
            IsDehydrated = HasBaseEFTEffect(_player, "Dehydration");
            IsExhausted = HasBaseEFTEffect(_player, "Exhaustion");
        }

        public bool HasBaseEFTEffect(Player player, string targetEffect)
        {
            IEnumerable<IEffect> commonEffects = player.ActiveHealthController.GetAllEffects();
            IReadOnlyList<EffectClass> effectsList = player.ActiveHealthController.IReadOnlyList_0;
            Type targetType = null;
            if (MedProperties.EffectTypes.TryGetValue(targetEffect, out targetType))
            {
                for (int i = effectsList.Count - 1; i >= 0; i--)
                {
                    EffectClass effect = effectsList[i];
                    Type effectType = effect.Type;
                    if (effectType == targetType)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
