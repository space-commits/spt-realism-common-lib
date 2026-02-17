using EFT.InventoryLogic;
using EFT;
using RealismCommonLib.Utils;
using UnityEngine;
using static EFT.Player;
using static RealismCommonLib.Plugin;
using RealismCommonLib.ModifierHandlers;

namespace RealismCommonLib.StateControllers.InstanceState
{
    public class AimingState : MonoBehaviour
    {
        public bool PlayerCanAim
        {
            get
            {
                return BoolHandlers.CanAim.Evaluate();
            }
        }

        public bool IsAiming
        {
            get
            {
                return PlayerStateInstance.ProceduralWeaponAnimation.IsAiming;
            }
        }
    }
}
