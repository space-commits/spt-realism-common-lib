using Comfort.Common;
using EFT;
using EFT.Animations;
using EFT.InventoryLogic;
using RealismCommonLib.Utils;
using UnityEngine;

namespace RealismCommonLib.StateControllers.InstanceState
{
    public class GameState : MonoBehaviour
    {
        private TimeInterval _checkGate = new TimeInterval(0.25f);

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

        //TODO: replace with events rather than doing these inefficent checks
        //may not be reliable enough if not called per frame, may leak exceptions 
        public void CheckIsReady()
        {
            GameWorld gameWorld = Singleton<GameWorld>.Instance;
            SessionResultPanel sessionResultPanel = Singleton<SessionResultPanel>.Instance;

            Player player = gameWorld?.MainPlayer;

            PlayerIsReady = player != null && player.IsYourPlayer;
            IsInHideout = player != null || player is HideoutPlayer ? true : false;

            if (gameWorld == null || gameWorld.AllAlivePlayersList == null || player == null || sessionResultPanel != null)
            {
                PlayerIsReady = false;
                IsInHideout = false;
            }
        }
    }
}
