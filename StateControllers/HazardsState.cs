using Comfort.Common;
using EFT;
using EFT.Animations;
using EFT.HealthSystem;
using UnityEngine;

namespace RealismCommonLib.StateControllers
{
    public class HazardsState : MonoBehaviour
    {
        public float TotalToxicity { get; set; } = 0f;

        public float TotalRadiation { get; set; } = 0f;

        public bool IsCoughingInGas { get; set; } = false;
    }
}
