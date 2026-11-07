using System;
using System.Collections.Generic;

namespace RealismCommonLib.Integration
{
    /// <summary>
    /// Lets modules read each other's data without referencing each other's assemblies.
    /// The module that owns the data registers an implementation in its Awake, consumers read.
    /// Typed accessors return a null object when the provider isn't installed.
    /// </summary>
    public static class ModuleServices
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private static readonly IStanceInfo _nullStance = new NullStanceInfo();

        public static void Register<T>(T impl) where T : class
        {
            _services[typeof(T)] = impl;
        }

        public static bool TryGet<T>(out T impl) where T : class
        {
            bool found = _services.TryGetValue(typeof(T), out var o);
            impl = found ? (T)o : null!;
            return found;
        }

        public static IStanceInfo Stance => TryGet<IStanceInfo>(out var s) ? s : _nullStance;
    }
}
