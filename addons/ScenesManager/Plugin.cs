#if TOOLS
using Godot;
using MoF.Addons.ScenesManager.Constants;
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
			PathToPlugin = GetPathToPlugin();
			// Declare custom type TransitionNode
			var script = GD.Load<Script>(PathToPlugin + "TransitionNode.cs");
			var texture = GD.Load<Texture2D>(PathToPlugin + "Assets/Icons/TransitionIconOn.svg");
			AddCustomType("TransitionNode", "Node", script, texture);

			// Add Graph Editor
			var editorScene = GD.Load<PackedScene>(PathToPlugin + "Assets/Scenes/ScenesManagerEditor.tscn");
			instantiatedEditorScene = editorScene.Instantiate<ScenesManagerEditor>();
			//AddControlToBottomPanel(instantiatedEditorScene, "Scenes Manager");

			// Add the main panel to the editor's main viewport.
			EditorInterface.Singleton.GetEditorMainScreen().AddChild(instantiatedEditorScene);
			// Hide the main panel. Very much required.
			_MakeVisible(false);

			// Add SceneManager Singleton
			AddAutoloadSingleton("ScenesManagerController", PathToPlugin + "ScenesManager.cs");
		}

		public override void _ExitTree()
		{
			if (instantiatedEditorScene != null)
			{
				RemoveAutoloadSingleton("ScenesManagerController");
				RemoveControlFromBottomPanel(instantiatedEditorScene);
				instantiatedEditorScene.QueueFree();
			}
		}

		public override bool _HasMainScreen() => true;

		public override void _MakeVisible(bool visible)
		{
			if (instantiatedEditorScene != null)
			{
				instantiatedEditorScene.Visible = visible;
			}
		}

		public override string _GetPluginName() => AddonConstants.PluginName;

		public override Texture2D _GetPluginIcon() => GD.Load<Texture2D>(GetPathToPlugin() + "Assets/Icons/scenes-manager-icon.svg");

		private string GetPathToPlugin()
		{
			Resource currentScript = (Resource)GetScript();
			return currentScript.GetPath().GetBaseDir() + "/";
		}
	}
}
#endif
