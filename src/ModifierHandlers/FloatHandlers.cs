using System;
using System.Collections.Generic;
using System.Text;

namespace RealismCommonLib.ModifierHandlers
{
    public sealed class FloatMultiplierHandle
    {
        public float Multiplier;
    }

    public sealed class FloatMultiplierSet
    {
        private readonly List<FloatMultiplierHandle> _mods = new();

        public FloatMultiplierHandle Add(float multiplier)
        {
            var handle = new FloatMultiplierHandle
            {
                Multiplier = multiplier
            };

            _mods.Add(handle);
            return handle;
        }

        public void Remove(FloatMultiplierHandle handle)
        {
            _mods.Remove(handle);
        }

        public float Apply(float baseValue)
        {
            float value = baseValue;

            for (int i = 0; i < _mods.Count; i++)
                value *= _mods[i].Multiplier;

            return value;
        }
    }

    public static class StatModifiers
    {
        //Reloads
        public static readonly FloatMultiplierSet MagReloadSpeed = new();
        public static readonly FloatMultiplierSet QuickMagReloadSpeed = new();
        public static readonly FloatMultiplierSet CheckAmmoSpeed = new();
        public static readonly FloatMultiplierSet CheckChamberSpeed = new();
        public static readonly FloatMultiplierSet PumpBoltSpeed = new();
        public static readonly FloatMultiplierSet MalfFixSpeed = new();
        public static readonly FloatMultiplierSet RechamberSpeed = new();
        public static readonly FloatMultiplierSet NoMagReloadSpeed = new();
        public static readonly FloatMultiplierSet NewMagReloadSpeed = new();
        public static readonly FloatMultiplierSet CurrentMagRemoveSpeed = new();
        public static readonly FloatMultiplierSet InternalReloadModifier = new();
        public static readonly FloatMultiplierSet RevolverReloadModifier = new();

        //Movement speed
        public static readonly FloatMultiplierSet WalkSpeedModifier = new();
        public static readonly FloatMultiplierSet SprintSpeedModifier = new();
        public static readonly FloatMultiplierSet SprintAccelModifier = new();
        public static readonly FloatMultiplierSet PreSprintAccelModifier = new();

    }
}
