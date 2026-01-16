using Comfort.Common;
using EFT;
using RealismCommonLib.StateControllers;
using UnityEngine;

namespace RealismCommonLib.Controllers
{
    public class RealismAudioController : MonoBehaviour
    {
        private float _globalAudioVolumeModifier = 1f;

        void Start()
        {

        }

        void Update()
        {

        }

        public void PlayADSSound(float vol, bool useMovementSound)
        {
            Plugin.PlayerStateInstance.Player.method_60(vol * _globalAudioVolumeModifier, useMovementSound);
        }

        public void PlayKnifeAttackSound(int vol)
        {
            Player player = Plugin.PlayerStateInstance.Player;

            int rndNum = UnityEngine.Random.Range(1, 11);
            string track = rndNum <= 5 ? "knife_1.wav" : "knife_2.wav";
            Singleton<BetterAudio>.Instance.PlayAtPoint(player.ProceduralWeaponAnimation.HandsContainer.WeaponRootAnim.position, HitAudioClips[track], 2, BetterAudio.AudioSourceGroupType.Distant, 100, vol, EOcclusionTest.Continuous);
        }
    }
}