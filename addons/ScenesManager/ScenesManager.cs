using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MoF.Addons.ScenesManager.Constants;
using MoF.Addons.ScenesManager.Extensions;
using MoF.Addons.ScenesManager.Scripts;
using MoF.Addons.ScenesManager.Scripts.Resources;

namespace MoF.Addons.ScenesManager
{
	[GlobalClass]
	public partial class ScenesManager : Node
	{
		[Export] public SceneManagerSchema SceneManagerSchema { get; set; }

		private bool _treeInitialized = false;
		private static SceneManagerSettings _sceneManagerSettings;
		private static PackedScene _currentPackedScene;
		private static SceneTree _tree;
		private static Node _currentScene;
		private static TransitionCanvasBase _transitionCanvas;
		private static Node _targetSceneRootNode = new();
		private static bool _isTargetSceneReady = false;

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
			var packedScene = sceneManagerOutSlotSignal.TargetScene.PackedScene;
			GD.Print($"[SceneManager] Switching scene: {packedScene.ResourcePath}");

			_targetSceneRootNode = packedScene.Instantiate();

			_targetSceneRootNode.Ready += () => OnReadyTarget(sceneManagerOutSlotSignal);
			_targetSceneRootNode.Set("visible", false);
			_tree.Root.AddChild(_targetSceneRootNode);

			StartTransition(sceneManagerOutSlotSignal);
		}

		private static void StartTransition(SceneManagerOutSlotSignal sceneManagerOutSlotSignal)
		{
			if (string.IsNullOrEmpty(sceneManagerOutSlotSignal.TransitionFileName))
			{
				_transitionCanvas = new JumpCutTransitionCanvas();
			}
			else
			{
				string transitionPath = $"{AddonConstants.TransitionFolderPath}/{sceneManagerOutSlotSignal.TransitionFileName}";
				PackedScene transitionPackedScene = ResourceLoader.Load<PackedScene>(transitionPath);
				_transitionCanvas = transitionPackedScene.Instantiate<TransitionCanvas>();
			}

			_transitionCanvas.Layer = 10;
			_transitionCanvas.InAnimationFinished += async () => await OnInAnimationFinished();
			_transitionCanvas.PlayInAnimation();
			_tree.Root.AddChild(_transitionCanvas);
		}

		private static void OnReadyTarget(SceneManagerOutSlotSignal sceneManagerOutSlotSignal)
		{
			_isTargetSceneReady = true;
			_targetSceneRootNode.Name = sceneManagerOutSlotSignal.TargetScene.graphNodeName;
			_currentPackedScene = sceneManagerOutSlotSignal.TargetScene.PackedScene;
			_tree.Root.GetChildren().OfType<ScenesManager>().FirstOrDefault()?.CallDeferred(nameof(SetSignals), _targetSceneRootNode);
		}

		private static void CompleteSceneSwitch()
		{
			_targetSceneRootNode.Set("visible", true);
			_tree.Root.RemoveChild(_tree.CurrentScene);
			_currentScene.QueueFree();
			_currentScene = _targetSceneRootNode;
			_tree.CurrentScene = _currentScene;
		}

		private static async Task OnInAnimationFinished()
		{
			_transitionCanvas.InAnimationFinished -= async () => await OnInAnimationFinished();

			await Task.Run(() =>
			{
				while (!_isTargetSceneReady) { }
			});

			_isTargetSceneReady = false;
			CompleteSceneSwitch();
			_targetSceneRootNode = null;
			_transitionCanvas.PlayOutAnimation();
			_transitionCanvas.OutAnimationFinished += OnOutAnimationFinished;
		}

		private static void OnOutAnimationFinished(Control targetScene)
		{
			_transitionCanvas.OutAnimationFinished -= OnOutAnimationFinished;
			_transitionCanvas.QueueFree();
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
			var settingsResource = ResourceLoader.Load<Resource>(AddonConstants.SettingsFilePath);
			if (settingsResource is SceneManagerSettings settings)
			{
				_sceneManagerSettings = settings;
			}
			else
			{
				GD.PrintErr($"[SceneManager] Failed to load settings from path: {AddonConstants.SettingsFilePath}");
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
	}
}
