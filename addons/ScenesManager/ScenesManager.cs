using System.Linq;
using Godot;
using MoF.Addons.ScenesManager.Constants;
using MoF.Addons.ScenesManager.Extensions;
using MoF.Addons.ScenesManager.Scripts;
using MoF.Addons.ScenesManager.Scripts.Resources;

namespace MoF.Addons.ScenesManager
{
	public partial class ScenesManager : Node
	{
		[Export] public SceneManagerSchema SceneManagerSchema { get; set; }

		public static string PathToPlugin { get; set; }

		private bool _treeInitialized = false;
		private static SceneManagerSettings _sceneManagerSettings;
		private static PackedScene _currentPackedScene;
		private static SceneTree _tree;
		private static Node _currentScene;
		private static TransitionNodeBase _transitionNode;
		private static Node _targetSceneRootNode = new();

		public ScenesManager()
		{
			PathToPlugin = GetPathToPlugin();
		}

		public override void _Ready()
		{
			_tree = GetTree();
			LoadSettings();
			LoadSchema();
			_tree.TreeChanged += OnTreeChanged;
		}

		private void OnTreeChanged()
		{
			_tree.TreeChanged -= OnTreeChanged;
			if (_currentPackedScene == null)
			{
				SetFirstSceneFromSchema();
			}
		}

		private void SetFirstSceneFromSchema()
		{
			foreach (SceneManagerBaseItem sceneManagerBaseItem in SceneManagerSchema.Items)
			{
				if (sceneManagerBaseItem is StartAppSceneManagerItem startAppSceneManagerItem)
				{
					CallDeferred(nameof(SetFirstScene), startAppSceneManagerItem);
				}
			}
		}

		private static void SetFirstScene(StartAppSceneManagerItem sceneManagerOutSlotSignal)
		{
			var packedScene = sceneManagerOutSlotSignal.OutSignals[0].TargetScene.PackedScene;
			GD.Print($"[SceneManager] Init first scene: {packedScene.ResourcePath}");

			_currentPackedScene = packedScene;
			_currentScene = packedScene.Instantiate();
			_currentScene.Ready += OnReadyInit;
			_tree.Root.RemoveChild(_tree.CurrentScene);
			_tree.Root.AddChild(_currentScene);
			_currentScene.Name = sceneManagerOutSlotSignal.OutSignals[0].TargetScene.graphNodeName;
		}

		private static void OnReadyInit()
		{
			_currentScene.Ready -= OnReadyInit;
			_tree.CurrentScene = _currentScene;

			_tree.Root.GetChildren().OfType<ScenesManager>().FirstOrDefault()?.CallDeferred(nameof(SetSignals), _currentScene);
		}

		private static void SwitchToScene(SceneManagerOutSlotSignal sceneManagerOutSlotSignal)
		{
			var targetPackedScene = sceneManagerOutSlotSignal.TargetScene.PackedScene;
			GD.Print($"[SceneManager] Switching scene: {targetPackedScene.ResourcePath}");

			if (string.IsNullOrEmpty(sceneManagerOutSlotSignal.TransitionFileName))
			{
				_transitionNode = new JumpCutTransitionNode();
			}
			else
			{
				string transitionPath = $"{PathToPlugin + AddonConstants.TransitionFolderPath}/{sceneManagerOutSlotSignal.TransitionFileName}";
				PackedScene transitionPackedScene = ResourceLoader.Load<PackedScene>(transitionPath);
				_transitionNode = transitionPackedScene.Instantiate<TransitionNode>();
			}

			_transitionNode.TransitionFinished += OnTransitionFinished;
			if (_transitionNode is TransitionNode transitionNode)
			{
				transitionNode.TransitionSpeed = sceneManagerOutSlotSignal.TransitionModifier.Speed;
				transitionNode.TransitionColor = sceneManagerOutSlotSignal.TransitionModifier.Color;
				transitionNode.CurrentSceneRoot = _currentScene;
			}
			_transitionNode.TargetNodeName = sceneManagerOutSlotSignal.TargetScene.graphNodeName;
			_transitionNode.TargetPackedScene = targetPackedScene;
			_tree.Root.RemoveChild(_tree.CurrentScene);
			_tree.Root.AddChild(_transitionNode);
		}

		private static void OnTransitionFinished(Node currentScene)
		{
			_transitionNode.TransitionFinished -= OnTransitionFinished;
			_tree.Root.RemoveChild(_transitionNode);
			_tree.Root.AddChild(currentScene);
			_tree.CurrentScene = currentScene;
			_tree.Root.GetChildren().OfType<ScenesManager>().FirstOrDefault()?.CallDeferred(nameof(SetSignals), currentScene);
			_currentScene = currentScene;
		}

		public void SetSignals(Node nodeSource)
		{
			foreach (SceneManagerBaseItem sceneManagerBaseItem in SceneManagerSchema.Items)
			{
				if (sceneManagerBaseItem is SceneManagerItem sceneManagerItem && sceneManagerItem.Name.ToString().Replace("@", "_") == nodeSource.Name)
				{
					ConnectSignals(nodeSource, sceneManagerItem);
				}
			}
		}

		private void ConnectSignals(Node nodeSource, SceneManagerItem sceneManagerItem)
		{
			foreach (SceneManagerOutSlotSignal sceneManagerOutSlotSignal in sceneManagerItem.OutSignals)
			{
				GD.Print($"[SceneManager] connecting signal: {sceneManagerOutSlotSignal.OutSlotSignalName}");
				nodeSource.ConnectToStaticDelegate<SceneManagerOutSlotSignal>(
					this,
					sceneManagerOutSlotSignal.OutSlotSignalName,
					nameof(SignalEmitted),
					nodeSource,
					sceneManagerOutSlotSignal);
			}
		}

		public static void SignalEmitted(Node sourceNode, SceneManagerOutSlotSignal sceneManagerOutSlotSignal)
		{
			if (sceneManagerOutSlotSignal.TargetSceneType == Enums.TargetSceneType.QuitGraphNode)
			{
				GD.Print("[SceneManager] Quitting the program");
				sourceNode.GetTree().Quit();
				return;
			}
			SwitchToScene(sceneManagerOutSlotSignal);
		}

		private static void LoadSettings()
		{
			var settingsResource = ResourceLoader.Load<Resource>(PathToPlugin + AddonConstants.SettingsFilePath);
			if (settingsResource is SceneManagerSettings settings)
			{
				_sceneManagerSettings = settings;
			}
			else
			{
				GD.PrintErr($"[SceneManager] Failed to load settings from path: {PathToPlugin + AddonConstants.SettingsFilePath}");
			}
		}

		private void LoadSchema()
		{
			if (_sceneManagerSettings == null)
			{
				GD.PrintErr("[SceneManager] SceneManagerSettings is null. Schema cannot be loaded");
				return;
			}

			var schemaResource = ResourceLoader.Load<Resource>(_sceneManagerSettings.SceneManagerSchemaPath);

			if (schemaResource is SceneManagerSchema schema)
			{
				SceneManagerSchema = schema;
			}
			else
			{
				GD.PrintErr($"[SceneManager] Failed to load schema from path: {_sceneManagerSettings.SceneManagerSchemaPath}");
			}
		}

		private string GetPathToPlugin()
		{
			var curScript = (Resource)GetScript();
			return curScript.GetPath().GetBaseDir() + "/";
		}
	}
}
