using System;
using System.Linq;
using System.Threading.Tasks;
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
				foreach (SceneManagerBaseItem sceneManagerBaseItem in SceneManagerSchema.Items)
				{
					if (sceneManagerBaseItem is StartAppSceneManagerItem startAppSceneManagerItem)
					{
						CallDeferred(nameof(SetFirstScene), startAppSceneManagerItem.OutSignals[0]);
					}
				}
			}
		}

		private static void SetFirstScene(SceneManagerOutSlotSignal sceneManagerOutSlotSignal)
		{
			PackedScene packedScene = sceneManagerOutSlotSignal.TargetScene.PackedScene;
			GD.Print($"[SceneManager] Init first scene: {packedScene.ResourcePath}");

			_currentPackedScene = packedScene;
			_currentScene = packedScene.Instantiate();
			_currentScene.Ready += OnReadyInit;
			_tree.Root.RemoveChild(_tree.CurrentScene);
			_tree.Root.AddChild(_currentScene);
		}

		private static void OnReadyInit()
		{
			_tree.Root.GetChildren()
					  .OfType<ScenesManager>()
					  .FirstOrDefault()?
					  .CallDeferred(nameof(SetSignals), new Variant[] { _currentPackedScene });

			_currentScene.Ready -= OnReadyInit;
			_tree.CurrentScene = _currentScene;
		}

		private static void SwitchToScene(SceneManagerOutSlotSignal sceneManagerOutSlotSignal)
		{
			PackedScene packedScene = sceneManagerOutSlotSignal.TargetScene.PackedScene;
			GD.Print($"[SceneManager] Switching scene: {packedScene.ResourcePath}");

			_targetSceneRootNode = packedScene.Instantiate();
			_targetSceneRootNode.Ready += () => OnReadyTarget(sceneManagerOutSlotSignal);
			_targetSceneRootNode.Set("visible", false);
			_tree.Root.AddChild(_targetSceneRootNode);


			if (!string.IsNullOrEmpty(sceneManagerOutSlotSignal.TransitionFileName))
			{
				string transitionPath = $"{AddonConstants.TransitionFolderPath}/{sceneManagerOutSlotSignal.TransitionFileName}";
				PackedScene transitionPackedScene = ResourceLoader.Load<PackedScene>(transitionPath);
				_transitionCanvas = transitionPackedScene.Instantiate<TransitionCanvas>();
				_transitionCanvas.Layer = 10;
				_transitionCanvas.PlayInAnimation();
				_transitionCanvas.InAnimationFinished += async () => await OnInAnimationFinished();
				_tree.Root.AddChild(_transitionCanvas);
			}
		}

		private static void OnReadyTarget(SceneManagerOutSlotSignal sceneManagerOutSlotSignal)
		{
			_tree.Root.GetChildren()
					  .OfType<ScenesManager>()
					  .FirstOrDefault()?
					  .CallDeferred(nameof(SetSignals), new Variant[] { _currentPackedScene });

			_isTargetSceneReady = true;

			if (string.IsNullOrEmpty(sceneManagerOutSlotSignal.TransitionFileName))
			{
				_targetSceneRootNode.Set("visible", true);
				_tree.Root.RemoveChild(_tree.CurrentScene);
				_currentScene = _targetSceneRootNode;
				_tree.CurrentScene = _targetSceneRootNode;
				_targetSceneRootNode = null;
			}
		}

		private static async Task OnInAnimationFinished()
		{
			_transitionCanvas.InAnimationFinished -= async () => await OnInAnimationFinished();

			await Task.Run(() =>
			{
				while (!_isTargetSceneReady) { }
			});
			_isTargetSceneReady = false;

			_targetSceneRootNode.Set("visible", true);
			_tree.Root.RemoveChild(_tree.CurrentScene);
			_currentScene = _targetSceneRootNode;
			_tree.CurrentScene = _targetSceneRootNode;
			_targetSceneRootNode = null;
			_transitionCanvas.PlayOutAnimation();
			_transitionCanvas.OutAnimationFinished += OnOutAnimationFinished;
		}

		private static void OnOutAnimationFinished()
		{
			_transitionCanvas.OutAnimationFinished -= OnOutAnimationFinished;
			_transitionCanvas.QueueFree();
		}

		public void SetSignals(PackedScene packedScene)
		{
			foreach (SceneManagerBaseItem sceneManagerBaseItem in SceneManagerSchema.Items)
			{
				if (sceneManagerBaseItem is SceneManagerItem sceneManagerItem && sceneManagerItem.Scene == packedScene)
				{
					foreach (SceneManagerOutSlotSignal sceneManagerOutSlotSignal in sceneManagerItem.OutSignals)
					{
						Node nodeSource = GetTree().CurrentScene;
						nodeSource.ConnectToStaticDelegate<SceneManagerOutSlotSignal>(
							this,
							sceneManagerOutSlotSignal.OutSlotSignalName,
							nameof(SignalEmitted),
							nodeSource,
							sceneManagerOutSlotSignal);
					}
				}
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
			Resource settingsResource = ResourceLoader.Load<Resource>(AddonConstants.SettingsFilePath);
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

			Resource schemaResource = ResourceLoader.Load<Resource>(_sceneManagerSettings.SceneManagerSchemaPath);
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
