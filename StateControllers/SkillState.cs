using Comfort.Common;
using EFT;
using EFT.Animations;
using System.Numerics;
using UnityEngine;
using WeaponSkills = EFT.SkillManager.GClass2250;

namespace RealismCommonLib.StateControllers
{
    public class SkillState: MonoBehaviour
    {

        Player _player;

        Player.FirearmController _fc;

        public WeaponSkills WeaponSkillBuffs 
        { 
            get
            {
                return _player.Skills.GetWeaponInfo(_fc.Item);
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
                return _player.Skills.StrengthBuffAimFatigue.Value;
            }
        }

    }
}
