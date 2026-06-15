# RealismCommonLib Project Instructions

## Purpose
Shared infrastructure library for BepInEx mods targeting SPT. Provides common framework for patches, events, state management, configuration, and plugin lifecycle. Reduces complexity, improves compatibility, enables shared functionality across mods.

## Key Responsibilities
- **Event infrastructure**: Static event classes for gameplay events (PlayerEvents, ReloadEvents, InputEvents, PWAEvents)
- **Patch infrastructure**: ModulePatch base class for Harmony patching with SPT convenience libraries
- **State management**: MonoBehaviour-based state controllers for player, weapon, health, reload, audio state
- **Configuration system**: PluginConfig.cs with ConfigFile bindings and modifier system
- **Plugin registry**: PluginRegistry detects and manages dependent mods
- **Modifier system**: StatModifiers apply config-based and skill-based adjustments to gameplay values

## Project Structure
```
RealismCommonLib/
├── Plugin.cs                    # BepInEx plugin entry point, patch initialization
├── PluginConfig.cs              # Configuration bindings and defaults
├── PluginRegistry.cs            # Plugin detection and management
├── ConfigurationManagerAttributes.cs  # Config manager UI hints
├── Events/
│   ├── BaseEventHandler.cs      # Generic event raising with error handling
│   ├── PlayerEvents.cs          # Player lifecycle events
│   ├── InputEvents.cs           # Input-related events
│   ├── ReloadEvents.cs          # Reload sequence events
│   └── PWAEvents.cs             # ProceduralWeaponAnimation events
├── Patches/                     # Harmony patches for game method interception
│   ├── *Patch.cs                # Individual patch implementations
│   └── ...                      # 20+ patch files
├── ModifierHandlers/
│   ├── BoolHandlers.cs          # Boolean stat modifiers
│   └── FloatHandlers.cs         # Float stat modifiers
├── StateControllers/
│   ├── InstanceState/           # Player/weapon/health state classes
│   └── ...                      # State management components
├── PatchPipeline/               # Patch orchestration utilities
└── Utils/                       # Utility functions
```

## Core Components

### Plugin.cs (BepInEx Entry Point)
- Initializes on game start via Awake()
- Calls EnablePatches() to register all Harmony patches
- Calls InstantiateInstanceControllers() to create game-lifetime components
- Stores static singleton references (GameStateInstance, PlayerStateInstance, etc.)
- Subscribes PlayerEvents.OnPlayerInitArgs to create player-lifetime components

### Event System
**Static event classes** provide loose coupling between patches and listeners:
- `PlayerEvents`: OnPlayerInitArgs, OnPlayerInit, AimStateChanged, OnShotFired, OnItemAddedOrRemoved
- `ReloadEvents`: ChamberCheck, CheckAmmo, BoltActionPump, MagReload, QuickMagReload, WeaponStateReset, MalfFix
- `InputEvents`: WeaponSwapInput, ToggleBipodInput, ChangeStanceInput, ToggleBreathingInput
- `PWAEvents`: ProceduralWeaponAnimation-related events

**Event raising pattern**:
- Event declared as static Action in event class
- Internal Raise* method calls BaseEventHandler.RaiseEvent(event)
- Patches call Raise* methods to notify listeners
- Listeners subscribe via += operator

### Patch Pattern
All patches inherit from `ModulePatch` and follow this structure:
1. Override `GetTargetMethod()` to specify game method to intercept
2. Add [PatchPrefix] or [PatchPostfix] method
3. Call `Enable()` from Plugin.EnablePatches()

**Typical patch flow**:
- Extract player via _playerField.GetValue(__instance)
- Check if player.IsYourPlayer (skip if not)
- Raise event to notify listeners
- Read config value and apply modifier
- Call __instance methods to modify behavior
- Use Mathf.Clamp() to keep values in valid game ranges

### State Management
**Game-lifetime** (Plugin.gameObject):
- `GameState`: Global game state
- `AudioState`: Audio system state

**Player-lifetime** (player.gameObject, created on OnPlayerInitArgs):
- `PlayerState`: Per-player state (Player reference)
- `WeaponState`: Current weapon state (IsPistol, IsShotgun, IsManuallyOperated)
- `HealthState`: Player health state
- `ReloadState`: Reload system state
- `PWAState`: ProceduralWeaponAnimation state

**Access state via static references**:
```csharp
Plugin.PlayerStateInstance  // Current player state
Plugin.WeaponStateInstance  // Current weapon state
Plugin.HealthStateInstance  // Current health state
```

### Configuration System
**PluginConfig.cs** defines all mod settings:
- ConfigEntry<T> for each setting (bool, float, string, etc.)
- InitConfigBindings(ConfigFile) called from Plugin.Awake()
- Each entry has category, name, default, and description
- Accessed via PluginConfig.ConfigName.Value

### Modifier System
StatModifiers apply adjustments to gameplay values:
- Float modifiers: CheckAmmoSpeed, CheckChamberSpeed, etc.
- Applied via `Mathf.Clamp()` to ensure valid game ranges
- Modifiers can be from config or skill-based

## Public API

### For Dependent Mods
- **Events**: Subscribe to static events in PlayerEvents, ReloadEvents, InputEvents, PWAEvents
- **State access**: Via Plugin static singleton references (PlayerStateInstance, WeaponStateInstance, etc.)
- **Config**: Access via PluginConfig.ConfigName.Value
- **Modifiers**: Apply via StatModifiers.{SystemName}.Apply(baseValue)

### Event Subscription Example
```csharp
// In dependent mod Awake()
RealismCommonLib.Events.ReloadEvents.ChamberCheck += HandleChamberCheck;

private static void HandleChamberCheck()
{
    // Custom mod logic
}
```

### Accessing Player State
```csharp
var player = RealismCommonLib.Plugin.PlayerStateInstance.Player;
var weapon = RealismCommonLib.Plugin.WeaponStateInstance;
bool isPistol = weapon.IsPistol;
```

## Dependencies
- **External**: BepInEx, Harmony, SPT convenience libraries (Aki.Common, Aki.Reflection, etc.)
- **Internal**: None (provides infrastructure for other mods)

## Plugin Loading
- Load order: RealismCommonLib MUST load FIRST (other mods depend on it)
- Plugin.cs [BepInPlugin] GUID: "com.fontaine.realismCommonlib"
- Dependent mods should add RealismCommonLib.dll as project reference

## Common Development Tasks for This Library
1. **Add new event**: Create event Action in event class, add internal Raise* method
2. **Add new patch**: Create class inheriting ModulePatch, implement GetTargetMethod(), add [PatchPrefix/Postfix]
3. **Add new config**: Add ConfigEntry in PluginConfig.InitConfigBindings(), document in UI text
4. **Add new state**: Create MonoBehaviour class in StateControllers/, instantiate in Plugin.InstantiateInstanceControllers()
5. **Add modifier**: Create handler in ModifierHandlers/, expose via StatModifiers
