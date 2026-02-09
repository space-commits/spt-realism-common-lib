using System;
using System.Collections.Generic;
using System.Text;

namespace RealismCommonLib.Events
{
    public static class PlayerEvents
    {
        public static event Action? OnPlayerInit;

        public static event Action? OnItemAddedOrRemoved;

        public static event Action? AimStateChanged;

        public static event Action? OnShotFired;

        internal static void RaiseOnAimStateChanged()
        {
            BaseEventHandler.RaiseEvent(AimStateChanged);
        }

        internal static void RaiseOnPlayerInit()
        {
            BaseEventHandler.RaiseEvent(OnPlayerInit);
        }

        internal static void RaiseOnItemAddedOrRemoved()
        {
            BaseEventHandler.RaiseEvent(OnItemAddedOrRemoved);
        }

        internal static void RaiseOnShotFired()
        {
            BaseEventHandler.RaiseEvent(OnShotFired);
        }
    }
}
