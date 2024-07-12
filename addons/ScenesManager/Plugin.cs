#if TOOLS
using Godot;
using MoF.Addons.ScenesManager.Extensions;
using System;

namespace MoF.Addons.ScenesManager
{
	[Tool]
	public partial class Plugin : EditorPlugin
	{
		private ScenesManagerEditor instantiatedEditorScene;
		public override void _EnterTree()
		{
			Resource currentScript = (Resource)GetScript();
			var script = GD.Load<Script>(currentScript.GetPath() + "ScenesManager.cs");
			var texture = GD.Load<Texture2D>(currentScript.GetPath() + "/Assets/Icons/scenes-manager-icon.svg");
			AddCustomType("Control", "Node", script, texture);
			var scriptSceneGraphNode = GD.Load<Script>(currentScript.GetPath() + "/Scripts/Editor/SceneGraphNode.cs");
			var textureSceneGraphNode = GD.Load<Texture2D>(currentScript.GetPath() + "/Assets/Icons/scenes-manager-icon.svg");
			AddCustomType("GraphNode", "Node", scriptSceneGraphNode, textureSceneGraphNode);
			var editorScene = GD.Load<PackedScene>(currentScript.GetPath() + "/Assets/Scenes/ScenesManagerEditor.tscn");
			instantiatedEditorScene = editorScene.Instantiate<ScenesManagerEditor>();
			AddControlToBottomPanel(instantiatedEditorScene, "Scenes Manager");
			AddAutoloadSingleton("ScenesManagerController", currentScript.GetPath() + "ScenesManager.cs");
		}

		public override void _ExitTree()
		{
			RemoveAutoloadSingleton("ScenesManagerController");
			RemoveControlFromBottomPanel(instantiatedEditorScene);
			instantiatedEditorScene.QueueFree();
		}
	}
}
#endif
