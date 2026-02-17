using RealismCommonLib.Utils;
using UnityEngine;
using EFT;
using static EFT.Player;
using static RealismCommonLib.Plugin;
using RealismCommonLib.Events;

namespace RealismCommonLib.StateControllers.InstanceState
{
    /// <summary>
    /// Manages components that are instance (raid/hideout) specific
    /// </summary>
    public class InstanceStateController : MonoBehaviour
    {
        void Awake()
        {
            PlayerEvents.OnPlayerInit += InitPlayerComponents;
        }

        void Destroy()
        {
            PlayerEvents.OnPlayerInit -= InitPlayerComponents;
        }

        private void InitPlayerComponents(Player player)
        {
            PlayerStateInstance = player.gameObject.AddComponent<PlayerState>();
            PlayerStateInstance.Player = player;

            //TODO: weapon state instance needs to update on weapon swap or if player unequps weapon...
            WeaponStateInstance = player.gameObject.AddComponent<WeaponState>();

            HealthStateInstance = player.gameObject.AddComponent<HealthState>();
            HazardsStateInstance = player.gameObject.AddComponent<HazardsState>();
            GearStateInstance = player.gameObject.AddComponent<GearState>();
            FiringStateInstance = player.gameObject.AddComponent<FiringState>();
            SkillStateInstance = player.gameObject.AddComponent<SkillState>();
            ReloadStateInstance = player.gameObject.AddComponent<ReloadState>();
        }
    }
}
