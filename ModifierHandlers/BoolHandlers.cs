using System;
using System.Collections.Generic;
using System.Text;

namespace RealismCommonLib.ModifierHandlers
{
    public sealed class BoolGateHandle
    {
        public bool Allowed = true;
    }

    public sealed class BoolGateSet
    {
        private readonly List<BoolGateHandle> _handles = new();

        public BoolGateHandle Add(bool allowed = true)
        {
            var h = new BoolGateHandle { Allowed = allowed };
            _handles.Add(h);
            return h;
        }

        public void Remove(BoolGateHandle handle)
        {
            _handles.Remove(handle);
        }

        /// <summary>
        /// Returns false if any handle disallows.
        /// </summary>
        public bool Evaluate(bool baseValue = true)
        {
            bool result = baseValue;

            for (int i = 0; i < _handles.Count; i++)
            {
                if (!_handles[i].Allowed)
                    return false;
            }

            return result;
        }
    }

    public static class BoolHandlers
    {
        public static readonly BoolGateSet CanAim = new();
    }
}
