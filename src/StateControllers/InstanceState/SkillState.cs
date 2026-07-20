using Comfort.Common;
using EFT;
using EFT.Animations;
using System.Numerics;
using UnityEngine;
using static RealismCommonLib.Plugin;
using WeaponSkills = EFT.SkillManager.GClass2250;

namespace RealismCommonLib.StateControllers.InstanceState
{
    public class SkillState : MonoBehaviour
    {


        public WeaponSkills WeaponSkillBuffs
        {
            get
            {
                return PlayerStateInstance.Player.Skills.GetWeaponInfo(WeaponStateInstance.WeaponInstance);
            }

        }

        public float AimSkillADSBuff
        {
            get
            {
                return WeaponSkillBuffs.AimSpeed;
            }
        }

        public float StrengthSkillAimBuff
        {
            get
            {
                return PlayerStateInstance.Player.Skills.StrengthBuffAimFatigue.Value;
            }
        }

        public float StrengthWeightBuff
        {
            get
            {
                return PlayerStateInstance.Player.Skills.StrengthBuffLiftWeightInc.Value;
            }
        }

        public float ReloadSkillMulti
        {
            get
            {
                return WeaponSkillBuffs.ReloadSpeed;
            }
        }

        public float FixSkillMulti
        {
            get
            {
                return WeaponSkillBuffs.FixSpeed;
            }
        }

        public float WeaponSkillErgo
        {
            get
            {
                return WeaponSkillBuffs.DeltaErgonomics;
            }
        }
    }
}
