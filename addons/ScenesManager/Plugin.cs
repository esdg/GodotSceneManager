#if TOOLS
using Godot;
using MoF.Addons.ScenesManager.Extensions;

namespace MoF.Addons.ScenesManager
{
	[Tool]
	public partial class Plugin : EditorPlugin
	{
		public static string PathToPlugin { get; set; }
		private ScenesManagerEditor instantiatedEditorScene;
		public override void _EnterTree()
		{
			Resource currentScript = (Resource)GetScript();
			PathToPlugin = currentScript.GetPath();
			// Declare custom type TransitionCanvas
			var script = GD.Load<Script>(PathToPlugin + "TransitionCanvas.cs");
			var texture = GD.Load<Texture2D>(PathToPlugin + "/Assets/Icons/TransitionIconOn.svg");
			AddCustomType("TransitionCanvas", "Node", script, texture);

			// Add Graph Editor
			var editorScene = GD.Load<PackedScene>(currentScript.GetPath() + "/Assets/Scenes/ScenesManagerEditor.tscn");
			instantiatedEditorScene = editorScene.Instantiate<ScenesManagerEditor>();
			AddControlToBottomPanel(instantiatedEditorScene, "Scenes Manager");

			// Add SceneManager Singleton
			AddAutoloadSingleton("ScenesManagerController", PathToPlugin + "ScenesManager.cs");

		}
		public override void _Ready()
		{
			ScenesManager.PathToPlugin = PathToPlugin;//not working
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
