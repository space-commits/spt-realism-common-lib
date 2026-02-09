using System;
using System.Collections.Generic;
using System.Text;

namespace RealismCommonLib.Events
{
    public static class BaseEventHandler
    {
        public static void RaiseEvent(Action? eventHandler)
        {
            if (eventHandler == null) return;

            foreach (Action handler in eventHandler.GetInvocationList())
            {
                try
                {
                    handler();
                }
                catch (Exception ex)
                {
                    Plugin.ModLogger.LogError($"Error invoking {nameof(eventHandler)}: {ex}");
                }
            }
        }
    }
}
