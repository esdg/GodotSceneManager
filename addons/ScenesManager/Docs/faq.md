# Frequently Asked Questions

## ❓ General Questions

### Q: Which Godot versions are supported?
**A**: Godot 4.5 (Mono/.NET). The add-on is C#-only and requires the .NET variant of Godot. Use .NET SDK 8.0.

### Q: Can I use this with GDScript projects?
**A**: No, this plugin is built specifically for C# projects. GDScript support is not currently planned due to the plugin's architecture.

### Q: Is this compatible with Godot 3.x?
**A**: No, this plugin uses Godot 4.x specific APIs and is not backwards compatible.

### Q: Do I need to write any code to use this plugin?
**A**: Minimal code: define C# signals on your scenes (root script) and emit them on events (e.g., button presses). The add-on auto-connects those signals according to your schema and handles the transitions.

### Q: What's the difference between this and Godot's built-in scene system?
**A**: The Scene Manager provides:
- Visual scene flow design
- Professional transition effects
- Automatic signal wiring (you define signals; the add-on connects them)
- Centralized scene organization
- Low-code workflow

## 🎨 Transitions

### Q: Can I create custom transition effects?
**A**: Yes. Create a `.tscn` whose root is `TransitionNode` (or a subclass) and place it in `res://addons/ScenesManager/TransitionsLibrary/`. You can duplicate an existing transition scene and tweak its animation.

### Q: How many transitions can I have in a single scene graph?
**A**: There's no hard limit. The system is designed to handle complex scene graphs with hundreds of transitions efficiently.

### Q: Can I modify transition speed or effects at runtime?
**A**: Transition settings (file, speed, color) are stored per-connection in the schema and configured in the editor. There isn’t a public runtime API intended for changing them dynamically.

### Q: Which transition should I use for my game?
**A**: It depends on your game's style:
- **Puzzle/Strategy**: Cross Fade, Jump Cut
- **Action/Arcade**: Diamond Fade, Chaotic Fade  
- **RPG/Story**: Cross Fade to Color, Cloud Fade
- **Casual**: Cross Fade, Horizontal Curtain

### Q: Are transitions performance-intensive?
**A**: The built-in transitions are optimized for performance. For complex scenes, consider using simpler transitions like Cross Fade or Jump Cut.

## 🔧 Scene Management

### Q: Does this plugin support nested scenes?
**A**: Yes, you can manage transitions between any scenes, including nested scenes and sub-scenes.

### Q: Can I use this with additive scene loading?
**A**: The current version focuses on scene replacement. Additive scene loading support may be considered for future releases.

### Q: How do I handle scene-specific data persistence?
**A**: Use Godot's autoload system or implement a data manager singleton alongside the Scene Manager. The plugin focuses on scene transitions, not data management.

### Q: Can I have multiple scene graphs in one project?
**A**: Yes. Save multiple schemas (e.g., `res://SceneManagerSchemas/MainFlow.tres`, `MenuFlow.tres`). Open/save a schema in the editor to make it active (this updates `SceneManagerSettings.tres > SceneManagerSchemaPath`).

### Q: What happens if a scene file is missing?
**A**: You’ll see an error in the Output panel (e.g., schema load/scene load failures). Verify the `.tscn` path in your schema and that the file exists.

## 🛠️ Development

### Q: Where can I report bugs or request features?
**A**: Please use the [GitHub Issues](https://github.com/esdg/GodotSceneManager/issues) page. Use the provided templates for bug reports and feature requests.

### Q: How can I contribute to the project?
**A**: We welcome contributions! Please read our [Contributing Guidelines](../CONTRIBUTING.md) and [Code of Conduct](../CODE_OF_CONDUCT.md) before submitting pull requests.

### Q: Is there a Discord or community server?
**A**: Follow our social media links in the main README for community updates and discussions.

### Q: Can I use this plugin in commercial projects?
**A**: Yes! The plugin is MIT licensed, which allows commercial use. See the [License](../LICENSE) file for details.

### Q: Will this plugin conflict with other addons?
**A**: The plugin is designed to be non-intrusive and should work alongside other addons. If you encounter conflicts, please report them as issues.

## 📚 Technical Questions

### Q: How does the plugin work internally?
**A**: The Scene Manager:
1. Loads the active schema from `SceneManagerSettings.tres`
2. Sets the first scene from the schema’s Start node
3. Connects your scene’s C# signals (names from the schema) to the transition handler
4. Switches scenes using the configured transition when a signal is emitted
5. Cleans up the previous scene and transition node

### Q: Can I access the Scene Manager from code?
**A**: Yes. The autoload singleton is `/root/ScenesManagerController` (class `ScenesManager`). It’s meant to work automatically, but you can interact for advanced scenarios.

### Q: How are signals handled?
**A**: You define the C# signals on your scenes’ root scripts. In the graph, you pick which signal triggers each connection. At runtime, the add-on auto-connects those signals to its transition logic.

### Q: What file formats does the plugin use?
**A**: 
- Scene graphs (schemas): `.tres` resource files
- Transitions: `.tscn` scene files
- Settings: `.tres` resource files
- Scenes: Standard Godot `.tscn` files

### Q: How much memory does the plugin use?
**A**: The plugin has minimal overhead. Memory usage depends mainly on your scenes and transition effects, not the plugin itself.

### Q: Can i delete the demo folder?
**A**: Yes, the demo is not part of the plugin, you can also remove unused transition in transition library folder.

## 🎮 Game Development

### Q: Is this suitable for mobile games?
**A**: Yes! The plugin works on all Godot-supported platforms. For mobile, consider using simpler transitions for better performance.

### Q: Can I use this for VR/AR projects?
**A**: The plugin works with VR/AR projects, though you may want to use gentler transitions to avoid motion sickness.

### Q: How do I integrate this with save/load systems?
**A**: The Scene Manager handles scene transitions independently of save systems. Implement save/load through separate autoload singletons.

### Q: Can I preview transitions in the editor?
**A**: Test transitions by running the game. Transition scenes can be opened and inspected in the editor, but full scene-to-scene transitions are validated at runtime.

### Q: What's the best way to organize large projects?
**A**: Use multiple scene graphs:
- `MainFlow.tres` — Core game progression
- `MenuSystem.tres` — UI navigation  
- `LevelTransitions.tres` — Between levels
- `Cutscenes.tres` — Story sequences

### Q: Can I keep my own Main Scene?
**A**: Yes. Keep your usual Main Scene in Project Settings. At runtime, the `ScenesManagerController` autoload swaps to the Start node’s first scene and manages transitions from there.

### Q: How do I switch which flow runs at startup?
**A**: Open/save the desired schema in the `Scene Manager` tab. The plugin updates `res://addons/ScenesManager/Settings/SceneManagerSettings.tres > SceneManagerSchemaPath`.

## 🚀 Performance

### Q: How do I optimize transition performance?
**A**: 
- Use simpler transitions (Cross Fade, Jump Cut) for complex scenes
- Reduce texture sizes in transition effects
- Test on your target platform regularly
- Profile your scenes to identify bottlenecks

### Q: Does the plugin work well with large scenes?
**A**: Yes, but consider using Jump Cut transitions for very complex scenes to maintain smooth performance.

### Q: How does the plugin handle memory management?
**A**: The plugin automatically:
- Unloads previous scenes after transitions
- Cleans up transition nodes when complete
- Manages resource loading efficiently

### Q: Are transitions performance-intensive?
**A**: Built-in transitions are light. For very heavy scenes or constrained devices, use Jump Cut (none) or simpler fades, and keep transition scenes lean.

## 📋 Licensing

### Q: What license is this under?
**A**: MIT License. You can use this plugin freely in commercial and non-commercial projects.

### Q: Can I modify and redistribute the plugin?
**A**: Yes, under the MIT License terms. Please maintain the original license notice and attribution.

### Q: Do I need to credit the plugin in my game?
**A**: Not required by the license, but attribution is appreciated and helps support the project.

### Q: Can I sell games that use this plugin?
**A**: Absolutely! The MIT license allows commercial use without restrictions.

## 🔮 Future Development

### Q: What features are planned for future versions?
**A**: See the repository README and issue tracker for planned work and discussions.

### Q: Will there be GDScript support in the future?
**A**: Currently not planned due to the plugin's C#-based architecture, but community contributions are welcome.

### Q: How often is the plugin updated?
**A**: Updates depend on community feedback and development priorities. Follow the GitHub repository for the latest information.

### Q: Can I request specific features?
**A**: Yes! Please use GitHub Issues with the feature request template. Popular requests are more likely to be implemented.

---

**Still have questions?** 
- Check our [Documentation](./README.md)
- Browse [GitHub Issues](https://github.com/esdg/GodotSceneManager/issues)
- Start a [Discussion](https://github.com/esdg/GodotSceneManager/discussions)

Related guides: [Installation](installation.md), [Quick Start](quick-start.md), [Configuration](configuration.md), [Troubleshooting](troubleshooting.md)