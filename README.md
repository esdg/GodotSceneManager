<a name="header"></a>

[![Godot Scene Manager Cover image](https://raw.githubusercontent.com/esdg/GodotSceneManager/main/addons/ScenesManager/Docs/cover-image.png?raw=true)](#header)

[![GitHub Release](https://img.shields.io/github/v/release/esdg/GodotSceneManager?include_prereleases&style=flat-square)](https://github.com/esdg/GodotSceneManager/releases)
[![GitHub repo size](https://img.shields.io/github/repo-size/esdg/GodotSceneManager?style=flat-square)](#header)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](https://opensource.org/licenses/MIT)
[![Godot Engine](https://img.shields.io/badge/Godot-4.5.NET-478cbf?style=flat-square&logo=godot-engine&logoColor=white)](https://godotengine.org/)

# Godot Scene Manager Plugin

🎬 **Transform your Godot development workflow** with a powerful visual graph editor for managing scene transitions and relationships. Create stunning, smooth transitions between scenes with an intuitive drag-and-drop interface that makes complex scene management feel effortless.

---

## Table of Contents

- [🚀 Key Highlights](#key-highlights)
- [⚙️ System Requirements](#system-requirements)
- [📦 Installation](#installation)
- [🚀 Quick Start](#quick-start)
- [📚 Documentation](#documentation)
- [🎨 Available Transitions](#available-transitions)
- [💻 API Reference](#api-reference)
- [🔧 Configuration](#configuration)
- [📖 Usage Examples](#usage-examples)
- [🛠️ Troubleshooting](#troubleshooting)
- [❓ FAQ](#faq)
- [🗺️ Roadmap](#roadmap)
- [🤝 Contributing](#contributing)
- [📄 License](#license)
- [🔗 Links & Support](#links--support)

---

<a name="key-highlights"></a>
## 🚀 Key Highlights

- **🎨 Visual Graph Editor**: Design scene flow without coding
- **🎬 8+ Professional Transitions**: Cross-fade, diamond fade, curtain effects, and more
- **⚡ Zero-Code Workflow**: Everything configured through visual interface
- **🔧 Godot 4.5 .NET**: Built specifically for C# projects
- **🔗 Works with GDScript projects**: Use with the Godot .NET editor even if your game scripts are in GDScript
- **📖 Open Source**: MIT licensed with comprehensive documentation



<a name="system-requirements"></a>
## ⚙️ System Requirements

| Requirement | Version |
|-------------|---------|
| **Godot Engine** | 4.5+ (.NET variant) |
| **.NET SDK** | 6.0 or higher |
| **Platform** | Windows, macOS, Linux |

**Note**: Requires the Godot .NET editor/runtime. You can still write your game in GDScript; the plugin itself is implemented in C#.

📖 **Detailed Requirements**: See [Installation Guide](addons/ScenesManager/Docs/installation.md#system-requirements) for complete specifications.

<a name="installation"></a>
## 📦 Installation

### Quick Installation

1. **Download** the [latest release](https://github.com/esdg/GodotSceneManager/releases)
2. **Extract** and copy the `addons` folder to your project root
3. **Build** your project: `Build > Build Solution` in Godot
4. **Enable** the plugin: `Project > Project Settings > Plugins > SceneManager`

✅ **Verification**: You should see the Scene Manager tab in Godot's editor

📖 **Detailed Instructions**: See our [Installation Guide](addons/ScenesManager/Docs/installation.md) for alternative methods and troubleshooting.

<a name="quick-start"></a>
## 🚀 Quick Start

### 1. Create Your Scene Graph

1. **Open Scene Manager**: Click the "Scene Manager" tab in Godot's editor
2. **Add Scene Nodes**: Right-click → "Add Scene Node" for each game scene
3. **Connect Scenes**: Drag between nodes to create transitions
4. **Choose Effects**: Select transition effects for each connection
5. **Save**: File → Save to create your scene graph (.tres file)

![Scene Manager Editor](https://raw.githubusercontent.com/esdg/GodotSceneManager/main/addons/ScenesManager/Docs/imgs/screenshot-beta-1.png)

### 2. That’s it!

**No coding required!** Your scene transitions now work automatically based on your visual configuration.

📖 **Detailed Tutorial**: See our [Quick Start Guide](addons/ScenesManager/Docs/quick-start.md) for step-by-step instructions with examples.

<a name="documentation"></a>
## 📚 Documentation

**Complete guides and references:**

- **[📖 Documentation Hub](addons/ScenesManager/Docs/README.md)** — Browse all guides
- **[🚀 Quick Start](addons/ScenesManager/Docs/quick-start.md)** — Get started in minutes
- **[🎨 Transitions](addons/ScenesManager/Docs/transitions.md)** — Effects and customization
- **[🔧 Configuration](addons/ScenesManager/Docs/configuration.md)** — Settings and options
- **[🛠️ Troubleshooting](addons/ScenesManager/Docs/troubleshooting.md)** — Common issues and fixes
- **[❓ FAQ](addons/ScenesManager/Docs/faq.md)** — Frequently asked questions

<a name="available-transitions"></a>
## 🎨 Available Transitions

The Scene Manager includes a comprehensive library of professional transition effects:

### Standard Transitions
| Transition | Description | Use Case |
|------------|-------------|----------|
| **Cross Fade** | Smooth alpha-based transition | General scene changes |
| **Diamond Fade** | Diamond-shaped wipe effect | Dramatic scene transitions |
| **Horizontal Curtain** | Side-to-side curtain effect | Menu transitions |
| **Vertical Curtain** | Top-to-bottom curtain effect | Level changes |
| **Chaotic Fade** | Random fragment transition | Dynamic, energetic scenes |

### Color-Enhanced Transitions
| Transition | Description | Customization |
|------------|-------------|---------------|
| **Cross Fade to Color** | Fade through custom color | Configurable overlay color |
| **Diamond Fade to Color** | Diamond transition with color | Color and timing control |
| **Cloud Fade to Color** | Organic cloud-like transition | Color and pattern options |

<a name="api-reference"></a>
## 💻 API Reference

### Graph-Driven Workflow

The Scene Manager is entirely **graph-driven** — no programming required:

1. **Visual Configuration**: All scene flow is designed in the graph editor
2. **Automatic Signal Generation**: The plugin creates all necessary signals from your graph data
3. **Zero-Code Transitions**: Scene transitions happen automatically based on your configuration

### Graph Configuration

All functionality is configured through the visual graph editor:
- **Scene Nodes**: Define your game scenes
- **Connections**: Create relationships between scenes  
- **Transitions**: Choose visual effects for each connection
- **Triggers**: Configure what activates each transition
- **Flow Logic**: Design your complete scene flow visually

<a name="configuration"></a>
## 🔧 Configuration

### Plugin Settings

Access plugin settings through `Project > Project Settings > Plugins > SceneManager`:

| Setting | Description | Default |
|---------|-------------|---------|
| **Default Transition** | Fallback transition effect | Cross Fade |
| **Transition Speed** | Global speed multiplier | 1.0 |
| **Debug Mode** | Enable verbose logging | false |
| **Auto-save Schemas** | Automatically save scene graphs | true |

<a name="usage-examples"></a>
## 📖 Usage Examples

### Complete Workflow - No Coding Required

The Scene Manager provides a **100% visual workflow** for scene management:

#### 1. Graph Design
- Open the Scene Manager tab in Godot
- Add nodes for each scene in your project
- Connect nodes to define scene flow
- Configure transitions and triggers visually
- Save your scene graph as a resource file

#### 2. Automatic Operation
- The plugin reads your graph configuration
- Automatically generates all necessary signals and connections
- Handles scene transitions based on your visual design
- Applies transition effects as configured

#### 3. Runtime Behavior
- Your game runs exactly as designed in the graph
- Scene transitions occur automatically based on triggers
- No additional code or manual signal handling required
- All transition effects work seamlessly

### Example: Menu System

**Traditional approach** (without Scene Manager):
```csharp
// Lots of manual scene loading code...
GetTree().ChangeSceneToFile("res://scenes/GameLevel.tscn");
```

**Scene Manager approach**:
- Design your menu flow visually in the graph editor
- Configure transitions between Menu → Game → Settings
- Save the graph
- ✨ **Done!** Everything works automatically

### Example: Game Flow

Instead of writing complex scene management code, you simply:
1. **Create nodes** for: Main Menu, Game Level, Game Over, Settings
2. **Draw connections** between scenes with desired transitions
3. **Configure triggers** (button presses, game events, timers, etc.)
4. **Test immediately** - your complete scene flow works without any coding

The plugin handles scene loading, transition effects, state management, and signal routing automatically based on your visual design.

<a name="troubleshooting"></a>
## 🛠️ Troubleshooting

📖 **Troubleshooting**: See [Troubleshooting](addons/ScenesManager/Docs/troubleshooting.md) for more information.

### Getting Help

If you encounter issues not covered here:

1. **Check the Issues**: Browse [existing issues](https://github.com/esdg/GodotSceneManager/issues) on GitHub
2. **Create a Bug Report**: Use the issue template with detailed information
3. **Join the Community**: Connect with other users on our social platforms
4. **Review Documentation**: Check the `/Docs` folder for additional guides

<a name="faq"></a>
## ❓ FAQ

**Q: Do I need to write any code?**
A: No! Everything is configured visually in the graph editor.

**Q: Which Godot versions work?**
A: Godot 4.5 .NET and later.

**Q: Can I use Godot Scene Manager in a GDScript project?**
A: Yes — as long as you use the Godot .NET editor/runtime. Your game code can be in GDScript.

**Q: Is this free for commercial use?**
A: Yes! MIT licensed - use freely in any project.

📖 **More Questions**: See our [complete FAQ](addons/ScenesManager/Docs/faq.md) for detailed answers.

<a name="roadmap"></a>
## 🗺️ Roadmap

### Current: v1.0.1 Alpha
- ✅ Core scene management and visual editor
- ✅ 8 built-in transition effects
- 🔄 Performance optimizations and bug fixes

### Upcoming: v1.0 Beta (Q1 2026)
- Enhanced editor interface with real-time preview
- Advanced transition controls and scene loading strategies
- Expanded API documentation and testing suite

📖 **Detailed Roadmap**: See [complete roadmap](addons/ScenesManager/Docs/README.md#roadmap) with timelines and features.

<a name="contributing"></a>
## 🤝 Contributing

We welcome contributions! Whether you're fixing bugs, adding features, or improving documentation, your help makes the plugin better for everyone.

**Quick Start**:
1. Fork the repository and create a feature branch
2. Make your changes following our coding standards
3. Test thoroughly and update documentation
4. Submit a pull request with clear description

📖 **Complete Guide**: See [Contributing Guidelines](CONTRIBUTING.md) for detailed instructions, coding standards, and development setup.

<a name="license"></a>
## 📄 License

**MIT License** - Use freely in personal and commercial projects.

✅ **You can**: Use, modify, distribute, and sell  
✅ **You must**: Include original license notice  
❌ **You cannot**: Hold authors liable for damages

📖 **Full License**: See [LICENSE](LICENSE) file for complete terms.

<a name="links--support"></a>
## 🔗 Links & Support

### 🌐 Official Channels

[![Itch.io](https://img.shields.io/badge/Itch.io-FA5C5C?style=for-the-badge&logo=itchdotio&logoColor=white)](https://mid-or-feed.itch.io/godot-scene-manager)
[![GitHub](https://img.shields.io/badge/GitHub-100000?style=for-the-badge&logo=github&logoColor=white)](https://github.com/esdg/GodotSceneManager)
[![Facebook](https://img.shields.io/badge/facebook-1877F2?style=for-the-badge&logo=facebook&logoColor=white)](https://www.facebook.com/people/Mid-or-Feed/61559305242385/)
[![X (Twitter)](https://img.shields.io/badge/X-000000?style=for-the-badge&logo=x&logoColor=white)](https://x.com/MidorFeed270577)

### 📎 Resources

- **[🏷️ Releases](https://github.com/esdg/GodotSceneManager/releases)** - Download stable versions
- **[🐛 Issues](https://github.com/esdg/GodotSceneManager/issues)** - Report bugs and request features  
- **[💬 Discussions](https://github.com/esdg/GodotSceneManager/discussions)** - Ask questions and share ideas
- **[📚 Documentation](addons/ScenesManager/Docs/README.md)** - Complete guides and references

---

<div align="center">

**Made with ❤️ for the Godot Community**

*If this plugin helps your project, consider starring the repository!*

**[⬆ Back to Top](#header)**

</div>

