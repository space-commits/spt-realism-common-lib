using SPT.Reflection.Patching;
using System.Reflection;
using EFT.InventoryLogic;
using System;
using RealismCommonLib.Events;


public class WeaponConstructorPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(Weapon).GetConstructor(new Type[] { typeof(string), typeof(WeaponTemplate) });
    }


    [PatchPostfix]
    private static void PatchPostfix(Weapon __instance)
    {

    }
}