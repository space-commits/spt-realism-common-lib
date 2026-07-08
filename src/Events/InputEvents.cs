using System;
using static RealismCommonLib.Plugin;

namespace RealismCommonLib.Events
{
    public static class InputEvents
    {
        public static event Action? WeaponSwapInput;
        public static event Action? ToggleStepOutInput;

        /// <summary>
        /// Player toggled duck or prone
        /// </summary>
        public static event Action? ChangeStanceInput;

        public static event Action? ToggleBipodInput;
        public static event Action? ToggleLeftStanceInput;
        public static event Action? ToggleBreathingInput;
        public static event Action? ToggleShootingInput;
        public static event Action? ScrollInput;
        public static event Action? WeaponMountingInput;
        public static event Action? ChangeScopeInput;
        public static event Action? ChangeScopeMagnificationInput;
        public static event Action? ToggleHeadDevice;

        internal static void RaiseWeaponSwapInput()
        {
            BaseEventHandler.RaiseEvent(WeaponSwapInput);
        }

        internal static void RaiseToggleStepOutInput()
        {
            BaseEventHandler.RaiseEvent(ToggleStepOutInput);
        }

        internal static void RaiseChangeStanceInput()
        {
            BaseEventHandler.RaiseEvent(ChangeStanceInput);
        }

        internal static void RaiseToggleBipodInput()
        {
            BaseEventHandler.RaiseEvent(ToggleBipodInput);
        }

        internal static void RaiseToggleLeftStanceInput()
        {
            BaseEventHandler.RaiseEvent(ToggleLeftStanceInput);
        }

        internal static void RaiseToggleBreathingInput()
        {
            BaseEventHandler.RaiseEvent(ToggleBreathingInput);
        }

        internal static void RaiseToggleShootingInput()
        {
            BaseEventHandler.RaiseEvent(ToggleShootingInput);
        }

        internal static void RaiseScrollInput()
        {
            BaseEventHandler.RaiseEvent(ScrollInput);
        }

        internal static void RaiseWeaponMountingInput()
        {
            BaseEventHandler.RaiseEvent(WeaponMountingInput);
        }

        internal static void RaiseChangeScopeInput()
        {
            BaseEventHandler.RaiseEvent(ChangeScopeInput);
        }

        internal static void RaiseChangeScopeMagnificationInput()
        {
            BaseEventHandler.RaiseEvent(ChangeScopeMagnificationInput);
        }
        
        internal static void RaiseToggleHeadDevice()
        {
            BaseEventHandler.RaiseEvent(ToggleHeadDevice);
        }

    }
}
