using RealismCommonLib.Utils;
using UnityEngine;
using EFT;
using static EFT.Player;
using static RealismCommonLib.Plugin;

namespace RealismCommonLib.StateControllers
{
    public class StateController: MonoBehaviour
    {
        void Awake()
        {
              
        }

        private void InitPlayerComponents(Player player)
        {
            //method to add state controllers, or one component controller that manages others
            Plugin.GameStateInstance = player.gameObject.AddComponent<GameState>(); //should be global and not player specific?
            Plugin.AudioControllerInstance = player.gameObject.AddComponent<RealismAudioController>();  //should be global and not player specific?

            Plugin.PlayerStateInstance = player.gameObject.AddComponent<PlayerState>();
            Plugin.PlayerStateInstance.Player = player;

            //weapon state instance needs to update on weapon swap or if player unequps weapon...
            Plugin.WeaponStateInstance = player.gameObject.AddComponent<WeaponState>();

            Plugin.HealthStateInstance = player.gameObject.AddComponent<HealthState>();
            Plugin.HealthStateInstance._activeHealthController = player.ActiveHealthController;

            Plugin.HazardsStateInstance = player.gameObject.AddComponent<HazardsState>();
            Plugin.GearStateInstance = player.gameObject.AddComponent<GearState>();
            Plugin.FiringStateInstance = player.gameObject.AddComponent<FiringState>();
            Plugin.SkillStateInstance = player.gameObject.AddComponent<SkillState>();
            Plugin.ReloadStateInstance = player.gameObject.AddComponent<ReloadState>();
        }
    }
}
