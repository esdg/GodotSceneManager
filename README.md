<a name="header"></a>

[![Godot Scene Manager Cover image](https://raw.githubusercontent.com/esdg/GodotSceneManager/main/addons/ScenesManager/Docs/cover-image.png?raw=true)](#header)

[![GitHub Release](https://img.shields.io/github/v/release/esdg/GodotSceneManager?include_prereleases&style=flat-square)](https://github.com/esdg/GodotSceneManager/releases)
[![GitHub repo size](https://img.shields.io/github/repo-size/esdg/GodotSceneManager?style=flat-square)](#header)

# Godot Scene Manager Plugin

The **Scene Manager** plugin streamlines your Godot development workflow with a visual graph editor for managing transitions between scenes and nodes. Effortlessly create, visualize, and organize scene relationships using an intuitive drag-and-drop interface.

---

## Table of Contents

- [Features](#features)
- [Compatibility](#compatibility)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Folder Structure](#folder-structure)
- [Configuration](#configuration)
- [Usage](#usage)
- [Troubleshooting](#troubleshooting)
- [FAQ](#faq)
- [Planned Releases](#planned-releases)
- [Contributing](#contributing)
- [License](#license)
- [Links](#links)

---

## Features

- Visual graph editor for scene and node transitions
- Extensive collection of transition effects
- Drag-and-drop interface
- Intuitive scene flow visualization
- Simplified scene management
- Improved project organization

## Compatibility

- Compatible with Godot 4.3 and 4.4
- Updated for Godot 4.4 by [@malachite23](https://github.com/malachite23)

## Installation

1. [Download the plugin](https://github.com/esdg/GodotSceneManager/releases).
2. Copy the plugin folder into your project's `addons` directory.
3. Enable the plugin in **Project Settings > Plugins**.

## Quick Start

```gdscript
# Example: Switching scenes using SceneManager
var scene_manager = get_node("/root/SceneManager")
scene_manager.change_scene("res://scenes/MainMenu.tscn")
```

1. Open the Scene Manager from the Godot editor.
2. Create nodes and define transitions between them.
3. Visualize and manage your scene relationships.

![Screenshot](https://raw.githubusercontent.com/esdg/GodotSceneManager/main/addons/ScenesManager/Docs/screenshot.png)

## Folder Structure

```
addons/
└── ScenesManager/
    ├── Docs/           # Documentation and images
    ├── GraphEditor/    # Visual graph editor scripts
    ├── Transitions/    # Transition effects
    ├── SceneManager.gd # Main plugin script
    └── ...             # Other supporting files
```

## Configuration

- Access plugin settings via **Project > Project Settings > Plugins > Scene Manager**.
- Customize transition effects and graph editor options in the plugin panel.

## Usage

1. Open the Scene Manager from the Godot editor.
2. Use the graph editor to create nodes and transitions.
3. Double-click nodes to edit scene properties.
4. Drag to connect nodes and define transitions.
5. Save your scene graph for future use.

## Troubleshooting

- **Plugin not showing up:** Ensure the folder is in `addons/ScenesManager` and enabled in Project Settings.
- **Transition effects not working:** Check that your scenes are compatible with the selected effects.
- **Graph editor issues:** Try restarting the editor or updating Godot to the latest supported version.

## FAQ

**Q: Which Godot versions are supported?**  
A: Godot 4.3 and 4.4.

**Q: Can I use custom transition effects?**  
A: Yes, add your scripts to the `Transitions` folder.

**Q: Does this plugin support nested scenes?**  
A: Yes, you can manage transitions between nested scenes and nodes.

**Q: Where can I report bugs or request features?**  
A: Please use the [GitHub Issues](https://github.com/esdg/GodotSceneManager/issues) page.

## Planned Releases

### [v1 Alpha (MVP)](#alpha)

- Core functionality for basic scene management.
- Focused on gathering early feedback and addressing critical issues.
- [Milestone progress](https://github.com/esdg/GodotSceneManager/milestone/5?closed=1)
- [Downloads](https://github.com/esdg/GodotSceneManager/releases/tag/1.0.1-alpha.1)

### [v1 Beta](#beta)

- Additional features and improvements based on alpha feedback.
- Enhanced user experience and functionality.
- [Milestone progress](https://github.com/esdg/GodotSceneManager/milestone/2)

### [v1 RC](#rc)

- Nearly complete version with all planned features.
- Final testing and quality assurance.
- [Milestone progress](https://github.com/esdg/GodotSceneManager/milestone/3)

### [v1 Release](#release)

- Stable, fully-featured, and production-ready.
- Thoroughly tested for robust scene management.
- [Milestone progress](https://github.com/esdg/GodotSceneManager/milestone/4)

## Contributing

Contributions are welcome! Please open issues or submit pull requests via [GitHub](https://github.com/esdg/GodotSceneManager).

### Contributors

- [@malachite23](https://github.com/malachite23)

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

## Links

<a href="https://mid-or-feed.itch.io/godot-scene-manager" target="_blank"><img src="https://img.shields.io/badge/Itch.io-FA5C5C?style=for-the-badge&logo=itchdotio&logoColor=white" alt="Itch.io"></a>
<a href="https://www.facebook.com/people/Mid-or-Feed/61559305242385/" target="_blank"><img src="https://img.shields.io/badge/facebook-1877F2?style=for-the-badge&logo=facebook&logoColor=white" alt="Facebook"></a>
<a href="https://github.com/esdg/GodotSceneManager" target="_blank"><img src="https://img.shields.io/badge/GitHub-100000?style=for-the-badge&logo=github&logoColor=white" alt="GitHub"></a>
<a href="https://x.com/MidorFeed270577" target="_blank"><img src="https://img.shields.io/badge/X-000000?style=for-the-badge&logo=x&logoColor=white" alt="GitHub"></a>

