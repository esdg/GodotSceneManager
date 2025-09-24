# Quick Start Guide

Get from zero to smooth scene transitions in minutes. Minimal code is needed only to define and emit signals; scene switching is handled automatically by the add-on.

## ✅ Prerequisites

- Follow the [Installation](installation.md) guide: enable `SceneManager` and build the solution
- Godot 4.5 (Mono/.NET) with .NET SDK 8.0

## 1) Add signals to your scenes (C#)

Define signals on the root script of each scene that should trigger a transition. Emit them from UI or gameplay events.

Example: `MainMenu.tscn` root script

```csharp
using Godot;

public partial class MainMenu : Control
{
    [Signal] public delegate void StartGameEventHandler();
    [Signal] public delegate void OpenSettingsEventHandler();

    public override void _Ready()
    {
        GetNode<Button>("%StartButton").Pressed += () => EmitSignal(SignalName.StartGame);
        GetNode<Button>("%SettingsButton").Pressed += () => EmitSignal(SignalName.OpenSettings);
    }
}
```

Example: `GameLevel.tscn` root script

```csharp
using Godot;

public partial class GameLevel : Node
{
    [Signal] public delegate void PlayerDiedEventHandler();

    private void OnPlayerDeath()
    {
        EmitSignal(SignalName.PlayerDied);
    }
}
```

Tip: After adding signals, build the solution so the editor can discover them.

## 2) Create your schema visually

1. Open the `Scene Manager` main-screen tab
2. Add a “Game start” node (created automatically in a new graph)
3. Add a Scene Node for each `.tscn` you’ll use
4. For each Scene Node:
   - Pick the `.tscn` with the resource picker
   - Click “Add Out slot” to add an outgoing transition trigger
   - Choose a signal from the dropdown (populated from your scene’s root script)
   - Optionally choose a transition from `TransitionsLibrary` and tweak Speed/Color
5. Connect nodes by dragging from an out slot to a target node’s input
6. Optional: Add a “Game quit” node to exit the game from a signal
7. Save the schema (`File > Save`); the plugin updates `SceneManagerSettings.tres` automatically to point to it

![Scene Manager Editor](https://raw.githubusercontent.com/esdg/GodotSceneManager/main/addons/ScenesManager/Docs/imgs/screenshot-beta-1.png)

## 3) Run the game

- Press Play. The add-on loads the first scene from your “Game start” node and listens for your signals
- When a configured signal is emitted, the Scene Manager switches to the connected scene using the selected transition

Note: You can keep any bootstrap Main Scene set in Project Settings. The `ScenesManagerController` autoload takes over at runtime and swaps to the configured first scene.

## Example flow: Simple menu

Scenes:
- `MainMenu.tscn` (signals: `StartGame`, `OpenSettings`)
- `GameLevel.tscn` (signal: `PlayerDied`)
- `Settings.tscn` (signal: `Back`)
- `GameOver.tscn` (signal: `ReturnToMenu`)

Connections:
- MainMenu → GameLevel on `StartGame` (Transition: `cross_fade.tscn`)
- MainMenu → Settings on `OpenSettings` (Transition: Jump Cut = none)
- Settings → MainMenu on `Back` (Transition: `cross_fade.tscn`)
- GameLevel → GameOver on `PlayerDied` (Transition: `diamond_fade.tscn`)
- GameOver → MainMenu on `ReturnToMenu` (Transition: `cross_fade.tscn`)

Save as `res://SceneManagerSchemas/MainFlow.tres` and run.

## Tips

- If a signal doesn’t appear in the dropdown, ensure it’s defined on the scene’s root script and rebuild the solution
- Transition options come from `res://addons/ScenesManager/TransitionsLibrary/` (use the filename, e.g., `cross_fade.tscn`)
- Use Jump Cut by selecting “none” (no transition scene)
- Keep schemas in `res://SceneManagerSchemas/` for clarity (as in the demo)

## Next steps

- Browse the [Transitions Guide](transitions.md)
- Tweak settings in [Configuration](configuration.md)
- Having issues? See [Troubleshooting](troubleshooting.md)