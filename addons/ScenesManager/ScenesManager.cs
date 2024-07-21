using System;
using System.Linq;
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

		private bool _treeInitialized = false;
		private static SceneManagerSettings _sceneManagerSettings;

		private static PackedScene _currentPackedScene;

		private static SceneTree _tree;
		private static Node _currentScene;

		private static TransitionCanvas _transitionCanvas;
		private static Node _targetSceneRootNode = new();


		public override void _Ready()
		{
			_tree = GetTree();
			LoadSettings();
			LoadSchema();

			_tree.TreeChanged += OnTreeChanged;
		}

		private static void ChangeSceneToPacked(PackedScene packedScene)
		{
			// _targetCanvasLayer.AddChild(packedScene.Instantiate());
			// _tree.CurrentScene.AddChild(_targetCanvasLayer);
		}

		private static void SwitchToScene(SceneManagerOutSlotSignal sceneManagerOutSlotSignal)
		{
			var packedScene = sceneManagerOutSlotSignal.TargetScene.PackedScene;
			GD.Print($"[SceneManager] Switching scene : {packedScene.ResourcePath}");

			_targetSceneRootNode = packedScene.Instantiate();
			_targetSceneRootNode.Ready += OnReadyTarget;
			_tree.Root.AddChild(_targetSceneRootNode);
			// if (sceneManagerOutSlotSignal.TransitionFileName == "")
			// {
			// 	ChangeSceneToPacked(packedScene);
			// }
			// else
			// {
			// 	PackedScene transitionPackedScene = ResourceLoader.Load<PackedScene>(AddonConstants.TransitionFolderPath + "/" + sceneManagerOutSlotSignal.TransitionFileName);
			// 	_transitionCanvas = transitionPackedScene.Instantiate<TransitionCanvas>();
			// 	_transitionCanvas.PlayInAnimation();
			// 	_transitionCanvas.InAnimationFinished += OnInAnimationFinished;
			// 	_tree.CurrentScene.AddChild(_transitionCanvas);
			// }
		}

		private static void OnReadyTarget()
		{
			_tree.Root.RemoveChild(_tree.CurrentScene);
			GD.Print("sss");
		}

		private static void SetFirstScene(SceneManagerOutSlotSignal sceneManagerOutSlotSignal)
		{
			var packedScene = sceneManagerOutSlotSignal.TargetScene.PackedScene;
			GD.Print($"[SceneManager] Init first scene : {packedScene.ResourcePath}");
			_currentPackedScene = packedScene;
			_currentScene = packedScene.Instantiate();
			_currentScene.Ready += OnReadyInit;
			_tree.Root.RemoveChild(_tree.CurrentScene);
			_tree.Root.AddChild(_currentScene);
		}

		private static void OnReadyInit()
		{
			_tree.Root.GetChildren().OfType<ScenesManager>().FirstOrDefault().CallDeferred(MethodName.SetSignals, new Variant[] { _currentPackedScene });
			_currentScene.Ready -= OnReadyInit;
			_tree.CurrentScene = _currentScene;
		}

		private static void OnInAnimationFinished()
		{
			_transitionCanvas.InAnimationFinished -= OnInAnimationFinished;
			_tree.ChangeSceneToPacked(_currentPackedScene);
		}

		private static void OnTreeChangedAnimation(SceneManagerOutSlotSignal sceneManagerOutSlotSignal)
		{
			PackedScene transitionPackedScene = ResourceLoader.Load<PackedScene>(AddonConstants.TransitionFolderPath + "/" + sceneManagerOutSlotSignal.TransitionFileName);
			_transitionCanvas = transitionPackedScene.Instantiate<TransitionCanvas>();
			_tree.CurrentScene.AddChild(_transitionCanvas);
			_transitionCanvas.PlayOutAnimation();
			_transitionCanvas.OutAnimationFinished += OnOutAnimationFinished;
		}

		private static void OnOutAnimationFinished()
		{
			_transitionCanvas.OutAnimationFinished -= OnOutAnimationFinished;
			_tree.CurrentScene.RemoveChild(_transitionCanvas);
			_transitionCanvas = null;
			_transitionCanvas.QueueFree();
		}

		private void OnTreeChanged()
		{
			if (_currentPackedScene == null)
			{
				foreach (SceneManagerBaseItem sceneManagerBaseItem in SceneManagerSchema.Items)
				{
					if (sceneManagerBaseItem is StartAppSceneManagerItem startAppSceneManagerItem)
					{
						CallDeferred(MethodName.SetFirstScene, startAppSceneManagerItem.OutSignals[0]);
					}
				}
			}
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
