# Project Structure Guide

## 📁 Plugin Architecture

```
addons/ScenesManager/
├── 📄 plugin.cfg                    # Plugin configuration
├── 📄 Plugin.cs                     # Main plugin entry point
├── 📄 ScenesManager.cs              # Core scene manager logic
├── 📄 ScenesManagerEditor.cs        # Editor interface
├── 📄 TransitionNode.cs             # Animation-based transition node
├── 📄 TransitionNodeWithColor.cs    # Transition with color overlay
├── Assets/                          # UI assets and resources
│   ├── Fonts/                       # Custom fonts
│   ├── Icons/                       # Editor icons
│   ├── Scenes/                      # UI scene templates
│   ├── Shaders/                     # Transition shaders
│   └── Styles/                      # UI themes
├── Constants/                       # Plugin constants
├── Docs/                            # Documentation and images
├── Enums/                           # Scene and transition types
├── Extensions/                      # Godot extension methods
├── Helpers/                         # Utility classes
├── Scripts/                         # Core functionality
│   ├── JumpCutTransitionNode.cs     # Instant (no animation) transition
│   ├── Editor/                      # Editor-specific scripts
│   └── Resources/                   # Resource definitions
├── Settings/                        # Plugin settings
│   └── SceneManagerSettings.tres    # Runtime settings (schema path)
└── TransitionsLibrary/              # Pre-built transitions
  ├── chaotic_fade.tscn
  ├── cloud_fade_to_color.tscn
  ├── cross_fade.tscn
  ├── cross_fade_to_color.tscn
  ├── diamond_fade.tscn
  ├── diamond_fade_to_color.tscn
  ├── horizontal_cutrain.tscn
  └── vertical_cutrain.tscn
```

## Core Components

### Main Classes

#### `ScenesManager.cs`
- **Purpose**: Core singleton handling scene transitions
- **Location**: `addons/ScenesManager/ScenesManager.cs`
- **Function**: Reads graph data and manages scene switching automatically

#### `Plugin.cs`
- **Purpose**: Plugin entry point and editor integration
- **Location**: `addons/ScenesManager/Plugin.cs`
- **Function**: Registers the plugin with Godot editor

#### `ScenesManagerEditor.cs`
- **Purpose**: Visual graph editor interface
- **Location**: `addons/ScenesManager/ScenesManagerEditor.cs`
- **Function**: Provides the graph editing UI in Godot

### Transition System

#### Base Classes
- **`TransitionNodeBase.cs`**: Abstract base for all transitions
- **`TransitionNode.cs`**: Standard animation-based transitions
- **`TransitionNodeWithColor.cs`**: Color overlay transitions
- **`JumpCutTransitionNode.cs`**: Instant transitions

#### Transition Library
Built-in transitions in `TransitionsLibrary/`:
- Cross fades (with/without color)
- Diamond fades (with/without color)
- Cloud/chaotic fades
- Horizontal/vertical curtain

Note: You can use a Jump Cut (no animation) by selecting “none”; the library is optional for functionality.

### Support Systems

#### Extensions (`Extensions/`)
- **`GodotExtensionMethods.cs`**: Godot-specific utility methods
- **`StringExtensionMethods.cs`**: String manipulation helpers

#### Helpers (`Helpers/`)
- **`DirectoryHelper.cs`**: File system operations
- **`GodotHelpers.cs`**: Godot engine utilities
- **`MenuHelpers.cs`**: UI helper functions
- **`SceneManagerSchemaFileHelpers.cs`**: Schema file management

#### Constants (`Constants/`)
- **`Constants.cs`**: Plugin-wide constant definitions

#### Enums (`Enums/`)
- **`SceneType.cs`**: Scene type definitions
- **`SlotMode.cs`**: Connection slot types

## Recommended Project Layout

### With Scene Manager

When using the Scene Manager plugin, organize your project like this:

```
YourProject/
├── addons/
│   └── ScenesManager/          # Plugin files (don't modify)
├── SceneManagerSchemas/        # Your scene flow definitions (.tres)
│   ├── MainGameFlow.tres
│   ├── MenuSystem.tres
│   └── TutorialFlow.tres
├── Scenes/                     # Your game scenes
│   ├── UI/
│   │   ├── MainMenu.tscn
│   │   ├── Settings.tscn
│   │   └── Credits.tscn
│   ├── Gameplay/
│   │   ├── Level1.tscn
│   │   ├── Level2.tscn
│   │   └── GameOver.tscn
│   └── Shared/
│       ├── LoadingScreen.tscn
│       └── Pause.tscn
├── Scripts/                    # Your game logic
│   ├── UI/
│   ├── Gameplay/
│   └── Managers/
└── Assets/                     # Game assets
    ├── Audio/
    ├── Textures/
    └── Models/
```

### Scene Graph Organization

#### Single Graph Approach
- **Use when**: Simple, linear game flow
- **File**: `SceneGraphs/MainFlow.tres`
- **Contains**: All scenes and transitions in one graph

#### Multiple Graph Approach
- **Use when**: Complex games with distinct sections
- **Files**: 
  - `MainFlow.tres` - Core game progression
  - `MenuSystem.tres` - UI navigation
  - `LevelFlow.tres` - Between-level transitions
- **Benefits**: Easier to manage, better organization

### Asset Integration

#### Transition Assets
- Keep custom transitions in `res://addons/ScenesManager/TransitionsLibrary/`
- Root node must be `TransitionNode` (or subclass)
- Prefer consistent animation naming (`TRANSITION`) and timing

#### Scene Assets
- Organize scenes by function (UI, Gameplay, Shared)
- Use descriptive scene names that match graph nodes
- Keep scene file structure consistent

#### Demo (reference)
This repository includes a `Demo/` folder showing a working setup:
- `Demo/SceneManagerSchemas/DemoGraph.tres` — example schema
- `Demo/Scenes/` — sample scenes used by the schema

## File Dependencies

### Required Files
- `plugin.cfg` - Plugin registration
- `ScenesManager.cs` - Core functionality
- `Settings/SceneManagerSettings.tres` - Points to your active schema
- A scene graph resource file (`.tres`), created via the Scene Manager tab

### Optional Files
- Custom transition scenes
- Additional helper scripts

### Auto-Generated Files
- Scene graph resources (`.tres` files) when you save from the editor
- `Settings/SceneManagerSettings.tres` is updated automatically when opening/saving a schema

## Development Workflow

### Adding New Scenes
1. Create your scene file in appropriate folder
2. Open Scene Manager editor
3. Add scene node to graph
4. Define signals on the scene root script and build the solution (signals appear in the editor dropdown)
5. Configure connections and transitions (choose signals, transition file, speed/color)
6. Save graph schema — this updates `SceneManagerSettings.tres`

### Modifying Transitions
1. Edit existing transition scenes in `TransitionsLibrary/`
2. Or create new transition scenes
3. Update graph connections to use new transitions
4. Test in Scene Manager preview

### Project Maintenance
- Keep scene graphs organized and documented
- Regularly test all transition paths
- Update documentation when adding custom transitions
- Backup scene graph files with version control

Tip: You can keep your main scene as usual; the `ScenesManagerController` autoload takes over at runtime, switches to the schema’s first scene, and wires signals automatically.