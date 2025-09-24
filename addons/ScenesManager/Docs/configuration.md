# Configuration Guide

This guide covers the actual configuration surface of the Scene Manager add-on: selecting which schema to use at runtime, customizing transitions, and recommended project structure.

## 🔧 Where settings live

- Plugin manifest: `res://addons/ScenesManager/plugin.cfg`
- Runtime settings resource: `res://addons/ScenesManager/Settings/SceneManagerSettings.tres`
- Your schemas: any `.tres` you save from the Scene Manager tab (e.g., `res://SceneManagerSchemas/MainFlow.tres`)

There is a single runtime setting today: the path to the schema used at runtime.

## 🗂️ Scene Manager settings resource

File: `res://addons/ScenesManager/Settings/SceneManagerSettings.tres`

Class: `SceneManagerSettings`

Properties:
- `SceneManagerSchemaPath: string` — absolute resource path to your schema `.tres`

Notes:
- When you save or open a schema from the editor’s `Scene Manager` tab, the plugin automatically updates this file for you.
- You can also edit the `SceneManagerSettings.tres` manually in the Inspector and set `SceneManagerSchemaPath` to a different schema.

Example content:

```ini
[resource]
script = ExtResource(".../SceneManagerSettings.cs")
SceneManagerSchemaPath = "res://SceneManagerSchemas/MainFlow.tres"
```

## 🎛️ Per-connection transition configuration

All transition configuration is set visually per connection inside the `Scene Manager` tab:
- Transition scene: choose from `res://addons/ScenesManager/TransitionsLibrary/` or select “none” for a jump cut
- Transition speed: slider (0.0–2.0, default 1.0) scales the animation speed
- Transition color: available on color-capable transitions (e.g., fade-to-color variants)

These values are stored inside your schema and applied at runtime automatically.

## 🎨 Transitions library

Built-in transitions live in:
- `res://addons/ScenesManager/TransitionsLibrary/`

Add your own transitions by dropping `.tscn` files into this folder. Requirements:
- The scene’s root must be `TransitionNode` (or a subclass), because the runtime instantiates it as `TransitionNode`
- Provide a `TRANSITION` animation on an `AnimationPlayer` or rely on the default one the node creates
- Keep filenames stable once referenced by a schema; renaming requires updating affected connections

Tip: Duplicate an existing transition scene from the library and modify its animation for a quick start.

## 🚀 Selecting a schema (switching flows)

To switch which flow runs at startup:
1. Open the `Scene Manager` tab
2. `Graph > Open Graph...` and pick a different `.tres` schema
3. Save (or simply opening will also update the setting)

The plugin updates `SceneManagerSettings.tres > SceneManagerSchemaPath` automatically.

## 🔁 Autoload configuration

The add-on manages its AutoLoad for you. When enabled, it adds:

```ini
[autoload]
ScenesManagerController="*res://addons/ScenesManager/ScenesManager.cs"
```

You normally do not need to edit this manually. If the entry is missing, toggle the plugin off/on and rebuild.

## 📁 Recommended project structure

```
res://
├── addons/
│   └── ScenesManager/            # The add-on
├── SceneManagerSchemas/          # Your schemas (.tres)
│   ├── MainFlow.tres
│   └── MenuFlow.tres
└── Scenes/                       # Your game scenes
    ├── UI/
    ├── Levels/
    └── Shared/
```

Naming tips:
- Use descriptive node names in the graph (e.g., `MainMenu`, `Settings`, `GameLevel1`)
- Keep schema filenames meaningful (e.g., `MainFlow.tres`, `OnboardingFlow.tres`)

## ⚙️ Performance tips

- Prefer Jump Cut (no transition) where animation isn’t needed
- Shorten transition duration via the per-connection speed modifier
- Keep transition scenes lightweight (smaller textures, fewer effects)
- Test on your target platform early

## 📝 Verifications

At a glance:
- Plugin enabled: `Project > Project Settings > Plugins > SceneManager` (Enabled)
- AutoLoad present: `ScenesManagerController`
- Settings file points to your schema: `SceneManagerSettings.tres > SceneManagerSchemaPath`

For logging, use Godot’s Output panel. The add-on prints messages such as:

```
[SceneManager] Init first scene: res://Scenes/MainMenu.tscn
[SceneManager] connecting signal: StartGame
[SceneManager] Switching scene: res://Scenes/GameLevel.tscn
```

If something’s off, see the [Troubleshooting](troubleshooting.md) guide.