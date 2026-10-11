# spt-realism-common-lib Project Instructions

## Purpose
Shared infrastructure library for BepInEx mods targeting SPT. Provides common framework for patches, events, state management, configuration, and plugin lifecycle. Reduces complexity, improves compatibility, enables shared functionality across mods.

## Key Responsibilities
- **Event infrastructure**: Static event classes for gameplay events (PlayerEvents, ReloadEvents, InputEvents, PWAEvents)
- **Patch infrastructure**: ModulePatch base class for Harmony patching with SPT convenience libraries
- **State management**: MonoBehaviour-based state controllers for player, weapon, health, reload, audio state
- **Configuration system**: PluginConfig.cs with ConfigFile bindings and modifier system
- **Plugin registry**: PluginRegistry detects and manages dependent mods via BepInEx GUID
- **Modifier system**: FloatMultiplierSet (stacked float multipliers) and BoolGateSet (compound boolean gates) for runtime gameplay value modification
- **PatchPipeline**: Veto/Override pattern for coordinating behavior across mods/handlers without tight coupling

## Project Structure
```
spt-realism-common-lib/
├── Plugin.cs                    # BepInEx plugin entry point, patch initialization, static singletons
├── PluginConfig.cs              # Configuration bindings and defaults
├── PluginRegistry.cs            # Detects other loaded mods by BepInEx GUID
├── ConfigurationManagerAttributes.cs  # Config manager UI hints
├── Events/
│   ├── BaseEventHandler.cs      # Generic event raising with exception catching/logging
│   ├── PlayerEvents.cs          # Player lifecycle + gameplay events
│   ├── InputEvents.cs           # Input-related events (keybinds, scroll, etc.)
│   ├── ReloadEvents.cs          # Reload sequence events (each stage of reload)
│   └── PWAEvents.cs             # ProceduralWeaponAnimation lifecycle events
├── Patches/
│   ├── AimingPatches.cs         # ToggleAimPatch
│   ├── FiringPatches.cs         # ShootPatch
│   ├── InputPatches.cs          # KeyInputPatch1/2
│   ├── PlayerPatches.cs         # PlayerInitPatch, OnItemAddedOrRemovedPatch, walk/sprint speed, weight
│   ├── PWAPatches.cs            # UpdateWeaponVariablesPatch, PwaWeaponParamsPatch
│   └── ReloadPatches.cs         # All reload animation patches (20+)
├── ModifierHandlers/
│   ├── BoolHandlers.cs          # BoolGateSet/Handle system + static BoolHandlers registry
│   └── FloatHandlers.cs         # FloatMultiplierSet/Handle system + static StatModifiers registry
├── StateControllers/
│   ├── GlobalState/
│   │   ├── GameState.cs         # Global game state (IsInHideout, PlayerIsReady)
│   │   └── AudioState.cs        # Audio system state
│   └── InstanceState/           # Player-lifetime state (created on PlayerInitPatch)
│       ├── PlayerState.cs       # Player reference, FirearmController, PWA, movement flags
│       ├── WeaponState.cs       # Weapon class, ergo, weight, IsPistol, TreatAsPistol
│       ├── AimingState.cs       # IsAiming, PlayerCanAim (via BoolHandlers.CanAim)
│       ├── HealthState.cs       # Health state, injuries, overdose, dehydration, etc.
│       ├── HazardsState.cs      # Radiation, toxicity totals
│       ├── GearState.cs         # NVG, thermal, face shield, gas mask state
│       ├── FiringState.cs       # Firing-related flags
│       ├── SkillState.cs        # Skill stat buffs
│       └── ReloadState.cs       # Reload operation type tracking
├── PatchPipeline/
│   ├── Pipelines.cs             # Static registry of all pipelines (InputVetoPipeline, etc.)
│   ├── PatchVetoPipeline.cs     # Registered handlers can return true to block a patch
│   ├── PatchOveridePipeline.cs  # Registered handlers can return true to signal an override
│   ├── PatchOverrideHandler.cs  # Registered handlers run custom override logic
│   └── Subscriber.cs            # IDisposable subscription token for unregistering
└── Utils/
    ├── Cloner.cs                # ShallowClone<T>() via MemberwiseClone reflection
    ├── CurveDrawer.cs           # Debug utility for visualizing AnimationCurves in-game
    ├── Gizmos.cs                # Debug gizmo drawing utilities
    ├── MathUtils.cs             # Vector clamping, float/vector comparison helpers
    └── TimerUtils.cs            # TimeInterval, TimeGate, DelayTimer
```

## Core Components

### Plugin.cs (BepInEx Entry Point)
- Initializes on game start via `Awake()`
- Calls `EnablePatches()` to register all Harmony patches via `.Enable()`
- Calls `InstantiateInstanceControllers()` → adds game-lifetime MonoBehaviours to `this.gameObject`, subscribes to `PlayerEvents.OnPlayerInitArgs` for player-lifetime components
- Static singleton references updated on player init:
  - `GameStateInstance`, `AudioControllerInstance` — game-lifetime, on Plugin.gameObject
  - `PlayerStateInstance`, `WeaponStateInstance`, `HealthStateInstance`, `HazardsStateInstance`, `GearStateInstance`, `FiringStateInstance`, `SkillStateInstance`, `ReloadStateInstance`, `AimStateInstance` — player-lifetime, on player.gameObject

### Event System
**Static event classes** provide loose coupling between patches and listeners:

| Class | Events |
|-------|--------|
| `PlayerEvents` | `OnPlayerInitArgs(Player)`, `OnPlayerInit`, `AimStateChanged`, `OnShotFired`, `OnWeaponDraw`, `OnItemAddedOrRemoved` |
| `ReloadEvents` | `WeaponStateReset`, `ChamberCheck`, `CheckAmmo`, `BoltActionPump`, `MalfFix`, `Rechamber`, `MagReload`, `QuickMagReload`, `NewMagInserted`, `CurrentMagRemoved`, `InternalMagReload` |
| `InputEvents` | `WeaponSwapInput`, `ToggleStepOutInput`, `ChangeStanceInput`, `ToggleBipodInput`, `ToggleLeftStanceInput`, `ToggleBreathingInput`, `ToggleShootingInput`, `ScrollInput`, `WeaponMountingInput`, `ToggleGogglesInput`, `ChangeScopeInput`, `ChangeScopeMagnificationInput` |
| `PWAEvents` | `OnUpdateWeaponVariables(FirearmController)`, `OnPWAInitialize(FirearmController)` |

**Event raising pattern**:
- Each event class has `internal static void Raise*()` methods
- These call `BaseEventHandler.RaiseEvent(event)` which catches and logs exceptions
- Patches call `Raise*` methods; listeners subscribe via `+=` / unsubscribe via `-=`
- Dependent mods subscribe in their own Awake() or component OnAwake()

### Patch Pattern
All patches inherit from `ModulePatch` (SPT) and follow this structure:
1. Override `GetTargetMethod()` — use `AccessTools.Method(typeof(TargetClass), "MethodName")`
2. Add `[PatchPrefix]` or `[PatchPostfix]` static method
3. Call `new MyPatch().Enable()` from `Plugin.EnablePatches()`

**Common conventions**:
- Use `__instance` for the patched object
- Check `player.IsYourPlayer` — ignore AI/other players
- Access `____privateField` (four underscores) for private fields via Harmony injection
- Raise events via `Events.XyzEvents.RaiseXyz()`
- Apply modifiers: `StatModifiers.XyzSpeed.Apply(baseValue)` then clamp

### State Management
**Game-lifetime** (added to `Plugin.gameObject` in `InitGlobalComponents()`):
- `GameState`: Tracks `IsInHideout`, `PlayerIsReady` by polling `Singleton<GameWorld>.Instance` on a timer
- `AudioState`: Audio system state

**Player-lifetime** (added to `player.gameObject` in `InitPlayerComponents()`, triggered by `PlayerEvents.OnPlayerInitArgs`):
- `PlayerState`: Holds `Player` reference; exposes `FirearmController`, `PWA`, movement state (`IsMoving`, `IsSprinting`), `IsScav`, `WeaponIsReady`, `IsUsingStationaryWeapon`
- `WeaponState`: Weapon class flags (`IsPistol`, `TreatAsPistol`), ergo, weight, length, `BipodIsDeployed`
- `AimingState`: `IsAiming` (from PWA), `PlayerCanAim` (evaluated from `BoolHandlers.CanAim` gate set)
- `HealthState`: Health/injury state flags
- `HazardsState`: `TotalRadiation`, `TotalToxicity`
- `GearState`: `NVGIsActive`, `ThermalIsActive`, `FaceShieldIsActive`, `HasGasMask`
- `FiringState`: Firing state flags
- `SkillState`: Skill stat buffs (e.g., `AimSkillADSBuff`)
- `ReloadState`: Tracks reload operation type (`IsMagReloading`, `IsQuickReloading`, `ReloadAnimationSupportsActiveAim`, etc.)

**Access pattern** (from any mod):
```csharp
using static RealismCommonLib.Plugin;

var player = PlayerStateInstance.Player;
bool isPistol = WeaponStateInstance.TreatAsPistol;
bool isAiming = AimStateInstance.IsAiming;
```

### Configuration System
**PluginConfig.cs** defines all mod settings as `ConfigEntry<T>`:
- `InitConfigBindings(ConfigFile cfg)` — called from `Plugin.Awake()`
- Each entry: `cfg.Bind(category, name, defaultValue, description)`
- Access: `PluginConfig.ConfigName.Value`

### Modifier System
Two modifier types for runtime value composition:

**`FloatMultiplierSet` / `FloatMultiplierHandle`** — stacks float multipliers:
```csharp
// Register a handle (on awake):
_myHandle = StatModifiers.MagReloadSpeed.Add(1f);

// Update the multiplier value each frame:
_myHandle.Multiplier = 1.25f;

// Apply all stacked modifiers (in a patch):
float result = StatModifiers.MagReloadSpeed.Apply(baseValue);

// Unregister (on destroy):
StatModifiers.MagReloadSpeed.Remove(_myHandle);
```

Static `StatModifiers` registry:
- Reload: `MagReloadSpeed`, `QuickMagReloadSpeed`, `CheckAmmoSpeed`, `CheckChamberSpeed`, `PumpBoltSpeed`, `MalfFixSpeed`, `RechamberSpeed`, `NoMagReloadSpeed`, `NewMagReloadSpeed`, `CurrentMagRemoveSpeed`, `InternalReloadModifier`
- Movement: `MaxWalkSpeedModifier`, `MaxSprintSpeedModifier`, `SprintAccelModifier`, `PreSprintAccelModifier`

**`BoolGateSet` / `BoolGateHandle`** — compound boolean gate (all handles must allow):
```csharp
// Register a gate handle (on awake):
_myGate = BoolHandlers.CanAim.Add(true);

// Block or allow:
_myGate.Allowed = false;

// Evaluate (returns false if any handle disallows):
bool canAim = BoolHandlers.CanAim.Evaluate();

// Unregister (on destroy):
BoolHandlers.CanAim.Remove(_myGate);
```

Static `BoolHandlers` registry:
- `CanAim` — gates whether ADS is permitted; checked by `AimingState.PlayerCanAim`

### PatchPipeline System
Used to coordinate patch behavior across handlers/mods without tight coupling.  
All pipelines are registered in the static `Pipelines` class.

**`PatchVetoPipeline<TContext>`**: Any registered subscriber returning `true` blocks the patch.
**`PatchOveridePipeline<TContext>`**: Any registered subscriber returning `true` signals an override is needed (only one override allowed — multiple overrides log an error).
**`PatchOverrideHandler<TContext>`**: Registered subscribers run custom override logic when an override is signaled.

Currently defined pipelines (in `Pipelines.cs`):
- `Pipelines.InputVetoPipeline` — veto input commands (e.g., suppress fire key)
- `Pipelines.InputOverridePipeline` — signal that an input command should be overridden
- `Pipelines.InputOverrideHandler` — run override logic for an input command

`InputContext` carries `ECommand` to identify which input event is being processed.

Registration pattern (subscribe in `RunOnAwake`, dispose in `RunOnDestroy`):
```csharp
private IDisposable _subscription;

public void RunOnAwake() =>
    _subscription = Pipelines.InputVetoPipeline.Register(ctx => ctx.Command == ECommand.ToggleShooting && ShouldVeto());

public void RunOnDestroy() =>
    _subscription?.Dispose();
```

### Utils
- **`Cloner.ShallowClone<T>(obj)`**: Invokes `MemberwiseClone` via reflection — used to clone game objects (e.g., Spring instances)
- **`TimeInterval`**: Throttle-timer; `Ready()` returns true once per interval
- **`TimeGate`**: One-shot timer that deactivates after duration; has `IsActive` flag
- **`DelayTimer`**: Tracks elapsed time with Start/Stop
- **`MathUtils`**: `ClampVector`, `AreFloatsEqual`, `IsGreaterThan`, `IsLessThan` with epsilon tolerance
- **`CurveDrawer`**: In-game debug overlay for visualizing AnimationCurve shapes

## Public API

### For Dependent Mods
- **Events**: Subscribe to static events in `PlayerEvents`, `ReloadEvents`, `InputEvents`, `PWAEvents`
- **State**: Via `Plugin` static singletons (`PlayerStateInstance`, `WeaponStateInstance`, etc.)
- **Config**: `PluginConfig.ConfigName.Value`
- **Float modifiers**: `StatModifiers.XyzSpeed.Add(1f)` → `FloatMultiplierHandle`, update `.Multiplier`, remove on destroy
- **Bool gates**: `BoolHandlers.CanAim.Add(true)` → `BoolGateHandle`, set `.Allowed`, remove on destroy
- **Input pipelines**: `Pipelines.InputVetoPipeline.Register(...)` returns `IDisposable` subscription

### Event Subscription Example
```csharp
// Subscribe in mod Awake() or component RunOnAwake()
RealismCommonLib.Events.ReloadEvents.MagReload += OnMagReload;

private void OnMagReload()
{
    // Custom logic
}

// Unsubscribe in OnDestroy() or RunOnDestroy()
RealismCommonLib.Events.ReloadEvents.MagReload -= OnMagReload;
```

### Accessing Player State
```csharp
using static RealismCommonLib.Plugin;

var player = PlayerStateInstance.Player;
bool isPistol = WeaponStateInstance.TreatAsPistol;
bool canAim = AimStateInstance.PlayerCanAim;
bool isAiming = AimStateInstance.IsAiming;
```

## Dependencies
- **External**: BepInEx, Harmony, SPT reflection/patching libraries (`SPT.Reflection`)
- **Internal**: None (base library; provides infrastructure for all other mods)

## Plugin Loading
- RealismCommonLib MUST load before dependent mods (declare `[BepInDependency(RealismCommonLib.Plugin.MOD_GUID)]`)
- Plugin GUID: `"com.fontaine.realismCommonlib"`
- Dependent mods reference `RealismCommonLib.dll` as a project reference

## Common Development Tasks for This Library
1. **Add new event**: Add `static event Action?` in an event class, add `internal static void Raise*()` calling `BaseEventHandler.RaiseEvent()`
2. **Add new patch**: Create class inheriting `ModulePatch`, implement `GetTargetMethod()`, add `[PatchPrefix]`/`[PatchPostfix]`, call `new MyPatch().Enable()` in `Plugin.EnablePatches()`
3. **Add new config**: Add `ConfigEntry<T>` in `PluginConfig.InitConfigBindings()`, access via `PluginConfig.Name.Value`
4. **Add new state**: Create `MonoBehaviour` in `StateControllers/InstanceState/`, add static reference in `Plugin.cs`, instantiate in `InitPlayerComponents()`
5. **Add float modifier**: Add `FloatMultiplierSet` to `StatModifiers` in `FloatHandlers.cs`; consumers use `Add()`/`Remove()`/`Apply()`
6. **Add bool gate**: Add `BoolGateSet` to `BoolHandlers` in `BoolHandlers.cs`; consumers use `Add()`/`Remove()`/`Evaluate()`
7. **Add pipeline**: Add typed pipeline instance to `Pipelines.cs`; patches query it before executing logic
