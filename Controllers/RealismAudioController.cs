using Comfort.Common;
using EFT;
using UnityEngine;

namespace RealismCommonLib.Controllers
{
    public class RealismAudioController : MonoBehaviour
    {
        private Player _player;

        private float _globalAudioVolumeModifier = 1f;

        void Start()
        {

        }

        void Update()
        {

        }

        public void PlayADSSound(float vol, bool useMovementSound)
        {
            _player.method_58(vol * _globalAudioVolumeModifier, useMovementSound);
        }

        public void PlayKnifeAttackSound(int vol)
        {
            int rndNum = UnityEngine.Random.Range(1, 11);
            string track = rndNum <= 5 ? "knife_1.wav" : "knife_2.wav";
            Singleton<BetterAudio>.Instance.PlayAtPoint(_player.ProceduralWeaponAnimation.HandsContainer.WeaponRootAnim.position, HitAudioClips[track], 2, BetterAudio.AudioSourceGroupType.Distant, 100, vol, EOcclusionTest.Continuous);
        }
    }
}