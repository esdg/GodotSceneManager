#if TOOLS
using Godot;
using MoF.Addons.ScenesManager.Extensions;

namespace MoF.Addons.ScenesManager
{
	[Tool]
	public partial class Plugin : EditorPlugin
	{
		private ScenesManagerEditor instantiatedEditorScene;
		public override void _EnterTree()
		{
			Resource currentScript = (Resource)GetScript();
			// Declare TransitionCanvas
			var script = GD.Load<Script>(currentScript.GetPath() + "TransitionCanvas.cs");
			var texture = GD.Load<Texture2D>(currentScript.GetPath() + "/Assets/Icons/TransitionIconOn.svg");
			AddCustomType("TransitionCanvas", "Node", script, texture);

			// Add Graph Editor
			var editorScene = GD.Load<PackedScene>(currentScript.GetPath() + "/Assets/Scenes/ScenesManagerEditor.tscn");
			instantiatedEditorScene = editorScene.Instantiate<ScenesManagerEditor>();
			AddControlToBottomPanel(instantiatedEditorScene, "Scenes Manager");

			// Add SceneManager Singleton
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
