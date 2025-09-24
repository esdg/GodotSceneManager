# API Reference

Concise technical reference for classes, resources, and editor/runtime integration used by the Scene Manager add-on.

## Overview

- Visual editor defines a schema (.tres) with nodes, connections, and per-connection transition settings
- At runtime, an autoload singleton reads the schema, wires scene signals to transitions, and switches scenes
- You define and emit C# signals in your scenes; the add-on connects them based on the schema

## Runtime singleton

Class: `MoF.Addons.ScenesManager.ScenesManager : Node`

Members (selected):
- `[Export] SceneManagerSchema SceneManagerSchema { get; set; }` — schema used at runtime
- `static string PathToPlugin { get; set; }` — resolved plugin base path
- `public void SetSignals(Node nodeSource)` — connects the current scene’s signals to the manager based on the schema
- `public static void SignalEmitted(Node sourceNode, SceneManagerOutSlotSignal message)` — static handler invoked when a configured signal fires; performs the transition (or quits)

Lifecycle:
- On `_Ready`, loads settings, then loads the schema referenced by the settings file; enters the first scene defined by the Start node

## Data model (resources)

All types below are Godot `Resource` classes serialized into your schema `.tres`.

### `SceneManagerSchema`
- `Array<SceneManagerBaseItem> Items` — all nodes (scene, start, quit) and their connections

### `SceneManagerBaseItem`
- `string Name` — graph node name (used for wiring and display)
- `Array<SceneManagerOutSlotSignal> OutSignals` — configured outgoing connections (one per out slot)
- `Vector2 Position` — editor layout position

### `SceneManagerItem : SceneManagerBaseItem`
- `PackedScene Scene` — the `.tscn` represented by this node

### `StartAppSceneManagerItem : SceneManagerBaseItem`
Marker item for the “Game start” node.

### `QuitAppSceneManagerItem : SceneManagerBaseItem`
Marker item for the “Game quit” node.

### `SceneManagerOutSlotSignal`
- `int Index` — out slot index
- `string OutSlotSignalName` — C# signal name from the scene’s root script (must exist)
- `TargetScene TargetScene` — target scene descriptor
- `TargetSceneType TargetSceneType` — `StartAppGraphNode | SceneGraphNode | QuitGraphNode`
- `string TransitionFileName` — transition `.tscn` file name from `TransitionsLibrary` (empty/none = Jump Cut)
- `TransitionModifier TransitionModifier` — per-connection modifiers

### `TargetScene`
- `PackedScene PackedScene` — target scene resource
- `string graphNodeName` — target node name (for runtime naming)

### `TransitionModifier`
- `float Speed = 1.0f` — animation speed multiplier
- `Color Color = Colors.Black` — used by color-capable transitions

### `SceneManagerSettings` (resource file)
File: `res://addons/ScenesManager/Settings/SceneManagerSettings.tres`
- `string SceneManagerSchemaPath` — resource path of the active schema; updated automatically when saving/opening schemas in the editor

## Transitions API

### `TransitionNodeBase : Node`
- Signal: `TransitionFinished(Node currentScene)` — emitted when transition completes
- Properties:
  - `string TargetNodeName { get; set; }` — name to assign to the instantiated target scene
  - `virtual PackedScene TargetPackedScene { set; }` — set by the manager before starting
- Hooks:
  - `sealed override void _Ready()` — calls `_TransitionReady()`
  - `virtual void _TransitionReady()` — override to start your transition logic
- Helper:
  - `protected void SendTransitionFinishedSignal()` — cleans up and emits `TransitionFinished`

### `TransitionNode : TransitionNodeBase`
- [Export] `AnimationPlayer AnimationPlayer`
- Property `Node CurrentSceneRoot { set; }`
- Property `float TransitionSpeed { get; set; }` — sets `AnimationPlayer.SpeedScale`
- Default implementation:
  - Creates missing `AnimationPlayer`
  - Ensures a `TRANSITION` animation exists with visibility toggling
  - Wraps current/target scenes in isolated SubViewports
  - Plays the `TRANSITION` animation

### `TransitionNodeWithColor : TransitionNode`
- [Export] `ColorRect TransitionColorRect { get; set; }`
- Property `Color TransitionColor { get; set; }` — sets color (and shader parameter when present)
- Adds a color overlay layer before playing the transition

### `JumpCutTransitionNode : TransitionNodeBase`
- Instant switch; instantiates the target scene and immediately calls `SendTransitionFinishedSignal()`

## Editor integration

### `Plugin : EditorPlugin`
- Registers custom type `TransitionNode`
- Adds the `Scene Manager` main-screen UI (`ScenesManagerEditor`)
- Manages AutoLoad: adds/removes `ScenesManagerController` pointing to `ScenesManager.cs`

### `ScenesManagerEditor : Control`
- Graph editor UI to create, open, and save schemas
- On save/open, updates `SceneManagerSettings.tres` with the schema path

### `ScenesManagerBaseGraphNode : GraphNode` (tools)
- Base class for graph nodes; signals `GraphNodeReady`
- Abstract `Array<string> OutSignalsNames` — implemented by scene nodes

### `SceneGraphNode` (tools)
- Allows picking a `.tscn` and creating “Out” slots
- Populates signal dropdown from the selected scene’s root node via `GetSignalList()`
- Stores per-connection transition filename and modifiers

## Autoload and settings

Autoload (managed by the plugin):
```ini
[autoload]
ScenesManagerController="*res://addons/ScenesManager/ScenesManager.cs"
```

Settings resource (`SceneManagerSettings.tres`) holds:
- `SceneManagerSchemaPath` — points to the active `.tres` schema

## Usage notes

- Define C# signals on the root script of each `.tscn` used; emit them to trigger transitions
- After adding signals, build the solution so the editor can list them in the dropdown
- Transition filenames come from `res://addons/ScenesManager/TransitionsLibrary/`
- Empty transition = Jump Cut

## Custom transition example

```csharp
using Godot;

[Tool, GlobalClass]
public partial class MyCustomTransition : MoF.Addons.ScenesManager.TransitionNode
{
    [Export] public float FlashTime { get; set; } = 0.2f;

    public override void _TransitionReady()
    {
        // Ensure base setup (AnimationPlayer, containers, scenes)
        // Then customize the animation or play your own
        // Base class: SetupAnimationPlayer(); SetupTargetSceneRoot(); SetupCurrentSceneRoot();
        base._TransitionReady();
        // Optionally adjust speed or manipulate AnimationPlayer
        TransitionSpeed = 1.25f;
    }
}
```

## Logging and errors

Typical output messages:
```
[SceneManager] Init first scene: res://Scenes/MainMenu.tscn
[SceneManager] connecting signal: StartGame
[SceneManager] Switching scene: res://Scenes/GameLevel.tscn
```

Common issues:
- Settings file missing or schema path invalid → fix `SceneManagerSettings.tres`
- Transition file not found → ensure filename exists in `TransitionsLibrary`
- Signal not listed → define `[Signal]` on the scene root script and rebuild

See also: [Installation](installation.md), [Configuration](configuration.md), [Quick Start](quick-start.md), and [Troubleshooting](troubleshooting.md).