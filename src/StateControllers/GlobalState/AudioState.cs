using EFT;
using CommonAssets.Scripts.Audio;
using UnityEngine;
using HarmonyLib;
using System.Reflection;


namespace RealismCommonLib.StateControllers.GlobalState
{
    public class AudioState : MonoBehaviour
    {
        void Start()
        {

        }

        public void PlayADSSound(Player player, float volume = 1f)
        {
            player.UpdateMuffledState();
            float rolloff = player.CalculateMovementRolloff(EAudioMovementState.Gear, false);

            FieldInfo sourceField = AccessTools.Field(typeof(Player), "_gearSource");
            BetterSource audioSource = (BetterSource)sourceField.GetValue(player);

            FieldInfo bankField = AccessTools.Field(typeof(Player), "_gearSoundBank");
            SoundBank bank = (SoundBank)bankField.GetValue(player);

            audioSource.SetActive(true);
            player.SetRolloff(audioSource, rolloff);

            AudioClip audioClip = null;
            AudioClip audioClip2 = null;
            float balance = 1f;

            bank.PickClipsByDistance(ref audioClip, ref audioClip2, ref balance, (int)EnvironmentType.Outdoor, player.Distance);
           
            if (audioClip == null && audioClip2 == null)
            {
                return;
            }

            float maxDistance = audioSource.MaxDistance;

            if (player.Distance > maxDistance)
            {
                return;
            }

            audioSource.SetPitch(bank.RandomPitch);
            audioSource.Play(audioClip, audioClip2, balance, volume * 2f, true, true);
        }

        public void PlayKnifeAttackSound(int vol)
        {
            Player player = Plugin.PlayerStateInstance.Player;

            int rndNum = UnityEngine.Random.Range(1, 11);
            string track = rndNum <= 5 ? "knife_1.wav" : "knife_2.wav";
            //Singleton<BetterAudio>.Instance.PlayAtPoint(player.ProceduralWeaponAnimation.HandsContainer.WeaponRootAnim.position, HitAudioClips[track], 2, BetterAudio.AudioSourceGroupType.Distant, 100, vol, EOcclusionTest.Continuous);
        }
    }
}