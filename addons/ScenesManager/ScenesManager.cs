using Godot;
using MoF.Addons.ScenesManager.Constants;
using MoF.Addons.ScenesManager.Extensions;
using MoF.Addons.ScenesManager.Scripts.Resources;

namespace MoF.Addons.ScenesManager
{
	[GlobalClass]
	public partial class ScenesManager : Node
	{
		[Export] public SceneManagerSchema SceneManagerSchema { get; set; }

		private bool treeInitialized = false;
		private static SceneManagerSettings SceneManagerSettings;

		private static PackedScene CurrentPackedScene;

		private static SceneTree Tree;
		private static Node currentScene;

		public override void _Ready()
		{
			Tree = GetTree();
			LoadSettings();
			LoadSchema();

			GetTree().TreeChanged += () => OnTreeChanged();
		}

		private static void SwitchToScene(PackedScene packedScene)
		{
			GD.Print($"[SceneManager] Switching scene : {packedScene.ResourcePath}");
			CurrentPackedScene = packedScene;
			Tree.ChangeSceneToPacked(packedScene);

		}

		private void OnTreeChanged()
		{
			if (CurrentPackedScene == null)
			{
				foreach (SceneManagerBaseItem sceneManagerBaseItem in SceneManagerSchema.Items)
				{
					if (sceneManagerBaseItem is StartAppSceneManagerItem startAppSceneManagerItem)
					{
						CallDeferred(MethodName.SwitchToScene, new Variant[] { startAppSceneManagerItem.OutSignals[0].TargetScene.PackedScene });
					}
				}
			}
			else if (GetTree()?.CurrentScene != null && GetTree()?.CurrentScene != currentScene)
			{
				CallDeferred(MethodName.SetSignals, new Variant[] { CurrentPackedScene });
			}
			currentScene = GetTree()?.CurrentScene;
		}

		public void SetSignals(PackedScene packedScene)
		{
			foreach (SceneManagerBaseItem sceneManagerBaseItem in SceneManagerSchema.Items)
			{
				if (sceneManagerBaseItem is SceneManagerItem sceneManagerItem)
				{
					if (sceneManagerItem.Scene == packedScene)
					{
						foreach (SceneManagerOutSlotSignal sceneManagerOutSlotSignal in sceneManagerItem.OutSignals)
						{
							var nodeSource = GetTree().CurrentScene;
							nodeSource.ConnectToStaticDelegate<SceneManagerOutSlotSignal>(this, sceneManagerOutSlotSignal.OutSlotSignalName, nameof(SignalEmitted), nodeSource, sceneManagerOutSlotSignal);
						}
					}
				}
			}
		}

		public static void SignalEmitted(Node sourceNode, SceneManagerOutSlotSignal sceneManagerOutSlotSignal)
		{
			//sourceNode.Disconnect(sceneManagerOutSlotSignal.OutSlotSignalName, new Callable(d));
			if (sceneManagerOutSlotSignal.TargetSceneType == Enums.TargetSceneType.QuitGraphNode)
			{
				GD.Print($"[SceneManager] Quitting the program");
				sourceNode.GetTree().Quit();
				return;
			}
			SwitchToScene(sceneManagerOutSlotSignal.TargetScene.PackedScene);
		}

		private static void LoadSettings()
		{
			var settingsResource = ResourceLoader.Load<Resource>(AddonConstants.SettingsFilePath);
			if (settingsResource is SceneManagerSettings settings)
			{
				SceneManagerSettings = settings;
			}
			else
			{
				GD.PrintErr($"[SceneManager] Failed to load settings from path: {AddonConstants.SettingsFilePath}");
			}
		}

		private void LoadSchema()
		{
			if (SceneManagerSettings == null)
			{
				GD.PrintErr("[SceneManager] SceneManagerSettings is null. Schema cannot be loaded");
				return;
			}

			var schemaResource = ResourceLoader.Load<Resource>(SceneManagerSettings.SceneManagerSchemaPath);
			if (schemaResource is SceneManagerSchema schema)
			{
				SceneManagerSchema = schema;
			}
			else
			{
				GD.PrintErr($"[SceneManager] Failed to load schema from path: {SceneManagerSettings.SceneManagerSchemaPath}");
			}
		}
	}
}
