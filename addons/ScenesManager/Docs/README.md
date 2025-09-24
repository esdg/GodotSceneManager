# Scene Manager Documentation

Welcome to the documentation for the Godot Scene Manager plugin. These guides help you visually design scene flow, switch scenes via signals, and add polished transitions — all in a structured, low-code workflow.

## What is Scene Manager?

A visual scene flow editor for Godot 4 (.NET) that lets you:
- Design your game/app flow as a graph (schema)
- Connect your scene signals to navigation targets
- Play built-in or custom transitions between scenes
- Let an autoload singleton handle runtime switching

Key features:
- Visual schema editor (in the `Scene Manager` tab)
- Signal-driven navigation (you control when to switch)
- Built-in transitions library + custom transition support
- Autoload runtime (`ScenesManagerController`) wires things up automatically

## 📚 Documentation Structure

### Getting Started
- **[Installation Guide](installation.md)** - Complete installation instructions and system requirements
- **[Quick Start Guide](quick-start.md)** - Get up and running in minutes with step-by-step instructions

### Core Concepts
- **[Transitions Guide](transitions.md)** - Complete guide to all available transition effects
- **[Project Structure](project-structure.md)** - How to organize your project with Scene Manager
- **[Configuration](configuration.md)** - Settings, preferences, and advanced configuration

### Reference
- **[API Reference](api-reference.md)** - Complete technical reference for developers
- **[Troubleshooting](troubleshooting.md)** - Solutions to common issues and problems
- **[FAQ](faq.md)** - Frequently asked questions and answers

## ✅ Requirements

- Godot 4.5 (.NET/Mono)
- .NET SDK 8.0 installed on your system

See details in the [Installation Guide](installation.md).

## 🚀 Quick Navigation

### New to Scene Manager?
1. Start with [Installation Guide](installation.md)
2. Follow the [Quick Start Guide](quick-start.md)
3. Explore [Transitions Guide](transitions.md) for effects

### Setting Up Your Project?
1. Review [Project Structure](project-structure.md)
2. Configure settings in [Configuration](configuration.md)
3. Reference [API Documentation](api-reference.md) as needed

### Having Issues?
1. Check [Troubleshooting Guide](troubleshooting.md)
2. Browse [FAQ](faq.md) for common questions
3. Search [GitHub Issues](https://github.com/esdg/GodotSceneManager/issues)

### Quick start in 30 seconds (at a glance)
1. Open the `Scene Manager` tab and create a new schema
2. Set your Start node (first scene)
3. Add an Out slot, choose a signal from your scene, pick a transition (or “none”)
4. Connect to the target scene node and save the schema
5. Run the project and emit the signal in your scene code to navigate

## 📖 Documentation Features

### Visual Examples
Most guides include:
- Screenshots of the Scene Manager interface
- Step-by-step visual workflows
- Example scene graph configurations
- Code snippets where relevant

### Practical Focus
All documentation emphasizes:
- Real-world usage scenarios
- Best practices and recommendations
- Performance considerations
- Common pitfalls to avoid

### Easy Navigation
- Cross-references between related topics
- Clear section organization
- Searchable content structure
- Links to external resources

## 🧪 Demo

See the included demo under `Demo/` for a working example schema and scenes you can run and inspect.

## 🔗 External Resources

- **[Main Repository](https://github.com/esdg/GodotSceneManager)** - Source code and releases
- **[Issue Tracker](https://github.com/esdg/GodotSceneManager/issues)** - Bug reports and feature requests
- **[Discussions](https://github.com/esdg/GodotSceneManager/discussions)** - Community questions and ideas
- **[Releases](https://github.com/esdg/GodotSceneManager/releases)** - Download stable versions

## 🤝 Contributing to Documentation

Found an error or want to improve the documentation?

- **Quick Fixes**: Submit issues or pull requests on GitHub
- **Major Improvements**: Discuss in GitHub Discussions first
- **New Guides**: Check if your topic fits existing structure
- **Examples**: Real-world examples are always welcome

## 📋 Documentation Status

| Guide | Status | Last Updated |
|-------|--------|--------------|
| Installation | ✅ Complete | 2025-09-23 |
| Quick Start | ✅ Complete | 2025-09-23 |
| Transitions | ✅ Complete | 2025-09-23 |
| Project Structure | ✅ Complete | 2025-09-23 |
| Configuration | ✅ Complete | 2025-09-23 |
| API Reference | ✅ Complete | 2025-09-23 |
| Troubleshooting | ✅ Complete | 2025-09-23 |
| FAQ | ✅ Complete | 2025-09-23 |

---

**Need help getting started?** Begin with the [Quick Start Guide](quick-start.md) for a hands-on introduction to Scene Manager!