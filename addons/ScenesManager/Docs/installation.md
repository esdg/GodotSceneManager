# Installation

Get the Scene Manager add-on into your Godot project, enable it, and make a first run without surprises.

## ⚙️ Requirements

- Godot 4.5 (Mono/.NET build)
- .NET SDK 8.0
- Windows, macOS, or Linux

This add-on is written in C# and requires the .NET variant of Godot. GDScript-only projects are not supported.

## 📦 Install Options

Pick one of the following methods:

### Option A — Download Release (recommended)
1. Download the latest ZIP from the project’s [Releases](https://github.com/esdg/GodotSceneManager/releases)
2. Extract it and copy the `addons` folder into the root of your Godot project (so you get `res://addons/ScenesManager/...`)
3. Open the project in Godot 4.5 .NET
4. Build: menu `Build > Build Solution`
5. Enable the plugin: `Project > Project Settings > Plugins`
   - Find `SceneManager` and set it to `Enabled`

### Option B — Git (clone or submodule)
Place the plugin in `res://addons/ScenesManager`:

```powershell
# From your project root
mkdir -Force addons; Set-Location addons
git clone https://github.com/esdg/GodotSceneManager.git ScenesManager
```

Or as a submodule:

```powershell
git submodule add https://github.com/esdg/GodotSceneManager.git addons/ScenesManager
```

Then open your project in Godot, build the solution, and enable the plugin as in Option A.

### Option C — Manual copy
1. Download the repository as ZIP
2. Extract and rename the folder to `ScenesManager`
3. Place it at `res://addons/ScenesManager`
4. Build and enable as in Option A

## ▶️ Enable and First Run

1. Open your project in Godot 4.5 .NET
2. Build the C# solution: `Build > Build Solution`
3. Enable the plugin: `Project > Project Settings > Plugins > SceneManager > Enabled`
4. A new main-screen tab named `Scene Manager` appears (the plugin UI)
5. The plugin automatically registers an AutoLoad singleton named `ScenesManagerController`

## 🔧 Initial Configuration (Schema)

The add-on uses a schema resource that describes your scene flow.

- Default settings resource: `res://addons/ScenesManager/Settings/SceneManagerSettings.tres`
- Default schema path inside that file: `res://test.tres` (demo). You’ll likely want to change it.

Two easy ways to set it up:

1) From the Scene Manager tab
- Open the `Scene Manager` tab
- Create your graph (start node, scene nodes, connections, transitions)
- Save it as a `.tres` (for example: `res://SceneManagerSchemas/MainFlow.tres`)
- Open `SceneManagerSettings.tres` and set `SceneManagerSchemaPath` to your saved file

2) Point to an existing schema
- Create or copy a `.tres` schema into your project
- Open `SceneManagerSettings.tres`
- Set `SceneManagerSchemaPath` to that file’s path

Tip: Keep schemas under `res://SceneManagerSchemas/` for clarity (as in the demo).

## ✅ Verify Installation

After enabling and configuring, confirm the following:
- The `Scene Manager` tab is visible as a main-screen tab

![Scene Manager Editor](/imgs/screenshot-beta-tab.png)

- The plugin entry `SceneManager` is `Enabled` in Project Settings > Plugins
- `ScenesManagerController` appears in `Project > Project Settings > AutoLoad` (added automatically)
- Building the solution completes without errors

## ⬆️ Upgrading

1. Disable the plugin (Project Settings > Plugins)
2. Replace the folder `res://addons/ScenesManager` with the new version (or update your submodule)
3. Build the solution
4. Re-enable the plugin

## 🧹 Uninstalling

1. Disable the plugin
2. Close the project (optional but recommended)
3. Delete `res://addons/ScenesManager`

The AutoLoad entry `ScenesManagerController` is added/removed by the plugin itself when you enable/disable it.

## 🛠️ Troubleshooting

If the `Scene Manager` tab does not show up or the project fails to build:
- Ensure you’re on Godot 4.5 (Mono/.NET) and have .NET SDK 8.0 installed
- Make sure the plugin path is exactly `res://addons/ScenesManager`
- Rebuild the solution and restart the editor
- Verify the AutoLoad entry exists in your project settings

See the full guide: [Troubleshooting](troubleshooting.md) for detailed steps and error examples.