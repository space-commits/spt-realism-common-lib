using Comfort.Common;
using EFT;
using EFT.Animations;
using EFT.InventoryLogic;
using RealismCommonLib.Utils;
using UnityEngine;

namespace RealismCommonLib.StateControllers
{
    public class GameState: MonoBehaviour
    {
        private TimeInterval _checkGate = new TimeInterval(1f);

        public bool WeaponIsReady { get; private set; } = false;

        public bool IsInHideout { get; private set; } = false;

        public bool PlayerIsReady { get; private set; } = false;

        public void Update() 
        {
            if (_checkGate.Ready()) CheckIsReady();
        }

        public Player GetYourPlayer()
        {
            GameWorld gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null) return null;
            return gameWorld.MainPlayer;
        }

        public Player GetPlayerByProfileId(string id)
        {
            GameWorld gameWorld = Singleton<GameWorld>.Instance;
            return gameWorld.GetAlivePlayerByProfileID(id);
        }

        public void CheckIsReady()
        {
            GameWorld gameWorld = Singleton<GameWorld>.Instance;
            SessionResultPanel sessionResultPanel = Singleton<SessionResultPanel>.Instance;

            Player player = gameWorld?.MainPlayer;
            if (player != null)
            {
                WeaponIsReady = player?.HandsController != null && player?.HandsController?.Item != null && player?.HandsController?.Item is Weapon ? true : false;
                IsInHideout = player is HideoutPlayer ? true : false;
            }
            else
            {
                WeaponIsReady = false;
                IsInHideout = false;
            }

            if (gameWorld == null || gameWorld.AllAlivePlayersList == null || gameWorld.MainPlayer == null || sessionResultPanel != null)
            {
                PlayerIsReady = false;
                WeaponIsReady = false;
                IsInHideout = false;
            }
        }
    }
}
