using System;
using System.Collections.Generic;
using System.Text;

namespace RealismCommonLib.ModifierHandlers
{
    public sealed class FloatMultiplierHandle
    {
        internal float Multiplier;
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
        public static readonly FloatMultiplierSet AllReloadSpeeds = new();
        public static readonly FloatMultiplierSet MagReloadSpeed = new();
        public static readonly FloatMultiplierSet QuickMagReloadSpeed = new();
        public static readonly FloatMultiplierSet CheckAmmoSpeed = new();
        public static readonly FloatMultiplierSet CheckChamberSpeed = new();
        public static readonly FloatMultiplierSet PumpBoltSpeed = new();
        public static readonly FloatMultiplierSet MalfFixSpeed = new();
        public static readonly FloatMultiplierSet RechamberSpeed = new();
        public static readonly FloatMultiplierSet NoMagReloadSpeed = new();
    }

}
