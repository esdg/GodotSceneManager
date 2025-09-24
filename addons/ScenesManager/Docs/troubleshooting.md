# Troubleshooting Guide

Use this guide to quickly diagnose and fix common problems when using the Scene Manager add-on.

## 🔍 Quick diagnostics

Run through these checks first:
- Using Godot 4.5 (Mono/.NET) and .NET SDK 8.0
- Plugin folder exactly at `res://addons/ScenesManager`
- Solution builds without errors: `Build > Build Solution`
- Plugin enabled in `Project > Project Settings > Plugins` (entry name: `SceneManager`)
- A main-screen tab named `Scene Manager` appears
- `ScenesManagerController` is present in AutoLoad (added automatically when the plugin is enabled)

## 🧩 Installation & activation

### Plugin not appearing in the editor

Symptoms: No `Scene Manager` tab after enabling the plugin.

Fixes:
1. Check path: `res://addons/ScenesManager` (case-sensitive on some platforms)
2. Verify `addons/ScenesManager/plugin.cfg` exists and contains a valid `[plugin]` section
3. In `Project > Project Settings > Plugins`, ensure `SceneManager` is set to `Enabled`
4. Build the solution: `Build > Build Solution`, then restart the editor
5. Toggle the plugin off/on (disable → restart → enable)

### Build/compilation errors

Fixes:
1. Ensure you’re running the Mono/.NET build of Godot 4.5
2. Confirm .NET SDK 8.0 is installed:
   
     ```powershell
     dotnet --info
     ```
3. Editor: `Build > Clean Solution`, then `Build > Build Solution`
4. Close and reopen the project to clear stale assemblies

Note: You do not need to edit your `.csproj` to include add-on files—the Godot .NET SDK discovers C# files in the project automatically.

## 🎬 Scene switching & transitions

### Scenes switch but no transition plays

Likely cause: Transition file not found or invalid.

Fixes:
1. Use a known-good transition from `res://addons/ScenesManager/TransitionsLibrary/` (e.g., `cross_fade.tscn`, `diamond_fade.tscn`)
2. Ensure the connection’s transition file name exactly matches the file in `TransitionsLibrary` (including `.tscn`)
3. Test with no transition (defaults to Jump Cut) to isolate the issue

If a transition path is wrong, you may see an error like “Attempt to call function 'Instantiate' on a null instance”. Correct the transition filename.

### Scene transitions fail completely

Fixes:
1. In `res://addons/ScenesManager/Settings/SceneManagerSettings.tres`, set `SceneManagerSchemaPath` to a valid `.tres` schema
2. Ensure your schema contains a “Game start” node with at least one outgoing connection to a scene node
3. Verify each scene node references a valid `.tscn` path that exists
4. Open the Output panel and look for Scene Manager logs

## 🧭 Graph editor problems

Symptoms: The visual graph is empty, stale, or unresponsive.

Fixes:
1. Save the schema `.tres`, close, and reopen the project
2. Toggle the plugin off/on
3. Recreate only the problematic connections
4. Confirm all referenced `.tscn` files exist

## ⚙️ AutoLoad singleton

The plugin adds an AutoLoad named `ScenesManagerController` pointing to `res://addons/ScenesManager/ScenesManager.cs` when enabled, and removes it when disabled.

Fixes when not present:
1. Disable the plugin, restart the editor, enable the plugin again
2. Ensure the plugin builds successfully
3. Avoid adding duplicate manual AutoLoad entries with the same name

## 🧪 Manual testing steps

### Basic sanity test
1. Create a simple schema with a “Game start” node → one scene node (valid `.tscn`)
2. Use the default Jump Cut (no transition file) and run the project
3. Add a transition (e.g., `cross_fade.tscn`) and retest

### Stress tests
1. Add multiple connections and transition types
2. Switch rapidly between scenes
3. Test with heavy scenes to evaluate performance

## 📝 Logs and diagnostics

Open the Output panel to see messages printed by the add-on. Typical messages:

```
[SceneManager] Init first scene: res://Scenes/MainMenu.tscn
[SceneManager] connecting signal: StartGame
[SceneManager] Switching scene: res://Scenes/GameLevel.tscn
[SceneManager] Quitting the program
```

Error messages you may see:

- Settings not found:
    ```
    [SceneManager] Failed to load settings from path: res://addons/ScenesManager/Settings/SceneManagerSettings.tres
    ```
    Fix: Ensure the settings file exists at that path.

- Schema not found:
    ```
    [SceneManager] Failed to load schema from path: res://path/to/invalid.tres
    ```
    Fix: Set `SceneManagerSchemaPath` in `SceneManagerSettings.tres` to a valid `.tres`.

For more detailed engine output, you can run the editor from a terminal with verbose logs (platform-specific), or simply rely on the Output panel.

## 📬 Getting help

Before reporting an issue:
1. Search existing issues: https://github.com/esdg/GodotSceneManager/issues
2. Reduce to a minimal reproduction schema
3. Capture the Output panel logs
4. Recheck the installation guide

When filing a bug, include:
- Godot version (e.g., 4.5 .NET) and OS
- Plugin version (release tag or commit hash)
- Steps to reproduce (numbered)
- Minimal schema (or screenshots) and relevant paths
- Output logs (copy/paste)

## ✅ Prevention tips

Best practices:
1. Start simple; add complexity gradually
2. Test after each change to the graph
3. Keep schemas under a dedicated folder (e.g., `res://SceneManagerSchemas/`)
4. Use version control to track schema changes
5. Regularly update to the latest plugin

Common pitfalls:
1. Typos in transition filenames (must match files in `TransitionsLibrary`)
2. Incorrect schema path in `SceneManagerSettings.tres`
3. Using standard (non-.NET) Godot build
4. Forgetting to rebuild after enabling or upgrading the plugin
5. Missing “Game start” node or no outgoing connection