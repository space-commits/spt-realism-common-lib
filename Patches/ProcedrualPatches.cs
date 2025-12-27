using EFT;
using EFT.Animations;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;
using static EFT.Player;

namespace RealismCommonLib.CommonPatches
{
    internal class ProcedrualPatches
    {
        //used for applying first person proc animations
        public class ApplyComplexRotationPatch : ModulePatch
        {
            private static FieldInfo _aimSpeedField;
            private static FieldInfo _compensatoryField;
            private static FieldInfo _displacementStrField;
            private static FieldInfo _scopeRotationField;
            private static FieldInfo _weapTempRotationField;
            private static FieldInfo _weapTempPositionField;
            private static FieldInfo _isAimingField;
            private static FieldInfo _firearmControllerField;
            private static FieldInfo _playerField;

            protected override MethodBase GetTargetMethod()
            {
                _aimSpeedField = AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "_aimingSpeed");
                _compensatoryField = AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "_compensatoryScale");
                _displacementStrField = AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "_displacementStr");
                _scopeRotationField = AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "_targetScopeRotation");
                _weapTempPositionField = AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "_temporaryPosition");
                _weapTempRotationField = AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "_temporaryRotation");
                _isAimingField = AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "_isAiming");
                _firearmControllerField = AccessTools.Field(typeof(ProceduralWeaponAnimation), "_firearmController");
                _playerField = AccessTools.Field(typeof(FirearmController), "_player");

                return typeof(EFT.Animations.ProceduralWeaponAnimation).GetMethod("ApplyComplexRotation", BindingFlags.Instance | BindingFlags.Public);
            }

            [PatchPostfix]
            private static void Postfix(EFT.Animations.ProceduralWeaponAnimation __instance, float dt, Vector3 ____vCameraTarget)
            {
                FirearmController firearmController = (FirearmController)_firearmControllerField.GetValue(__instance);
                if (firearmController == null)
                {
                    return;
                }
                Player player = (Player)_playerField.GetValue(firearmController);
                if (player != null && player.IsYourPlayer && player.MovementContext.CurrentState.Name != EPlayerState.Stationary)
                {
                    FirearmController fc = player.HandsController as FirearmController;

                    Plugin.StanceControllerInstance.IsInThirdPerson = false;

                    float aimSpeed = (float)_aimSpeedField.GetValue(__instance);
                    float compensatoryScale = (float)_compensatoryField.GetValue(__instance);
                    float displacementStr = (float)_displacementStrField.GetValue(__instance);
                    Quaternion scopeRotation = (Quaternion)_scopeRotationField.GetValue(__instance);
                    Vector3 weapTempPosition = (Vector3)_weapTempPositionField.GetValue(__instance);
                    Quaternion weapTempRotation = (Quaternion)_weapTempRotationField.GetValue(__instance);
                    bool isAiming = (bool)_isAimingField.GetValue(__instance);

                    Vector3 handsRotation = __instance.HandsContainer.HandsRotation.Get();
                    Vector3 sway = __instance.HandsContainer.SwaySpring.Value;
                    handsRotation += displacementStr * (isAiming ? __instance.AimingDisplacementStr : 1f) * new Vector3(sway.x, 0f, sway.z);
                    handsRotation += sway;
                    Vector3 rotationCenter = __instance._shouldMoveWeaponCloser ? __instance.HandsContainer.RotationCenterWoStock : __instance.HandsContainer.RotationCenter;
                    Vector3 weapRootPivot = __instance.HandsContainer.WeaponRootAnim.TransformPoint(rotationCenter);

                    Plugin.StanceControllerInstance.CurrentRotation = Quaternion.Slerp(Plugin.StanceControllerInstance.CurrentRotation, __instance.IsAiming && Plugin.StanceControllerInstance.AllStancesReset ? Quaternion.identity : doStanceRotation ? Plugin.StanceControllerInstance.StanceRotation : Quaternion.identity, doStanceRotation ? Plugin.StanceControllerInstance.StanceRotationSpeed * PluginConfig.StanceRotationSpeedMulti.Value : __instance.IsAiming ? 7f * aimSpeed * dt : 8f * dt); //__instance.IsAiming ? 8f * aimSpeed * dt

                    //this needs to be done by firing overhaul
                    FiringStateInstance.DoVisualRecoil(ref weapTempRotation);

                    __instance.HandsContainer.WeaponRootAnim.SetPositionAndRotation(weapTempPosition, weapTempRotation * Plugin.StanceControllerInstance.CurrentRotation);
                }
            }
        }
    }
}
