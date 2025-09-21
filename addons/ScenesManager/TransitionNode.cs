using Godot;
using MoF.Addons.ScenesManager.Scripts;

namespace MoF.Addons.ScenesManager
{
	/// <summary>
	/// Handles animated scene transitions in Godot using an AnimationPlayer.
	/// Sets up containers for current and target scenes, manages animation playback,
	/// and provides preview scenes if none are assigned.
	/// </summary>
	[Tool, GlobalClass]
	public partial class TransitionNode : TransitionNodeBase
	{
		/// <summary>
		/// AnimationPlayer responsible for playing the transition animation.
		/// </summary>
		[Export]
		private AnimationPlayer AnimationPlayer { get; set; }

		/// <summary>
		/// Assigns the root node of the current scene.
		/// </summary>
		public Node CurrentSceneRoot
		{
			set => _currentSceneNode = value;
		}

		/// <summary>
		/// Sets the playback speed for the transition animation.
		/// </summary>
		public float TransitionSpeed
		{
			set => AnimationPlayer.SpeedScale = value;
			get => AnimationPlayer.SpeedScale;
		}

		/// <summary>
		/// Called when the transition is ready to begin.
		/// Validates the AnimationPlayer, sets up scene containers, and starts the animation.
		/// </summary>
		public override void _TransitionReady()
		{
			if (!ValidateAnimationPlayer()) return;

			SetupTargetSceneRoot();
			SetupCurrentSceneRoot();

			AnimationPlayer.Play("TRANSITION");
		}

		/// <summary>
		/// Checks if AnimationPlayer exists and contains the required "TRANSITION" animation.
		/// </summary>
		/// <returns>True if valid, otherwise false.</returns>
		private bool ValidateAnimationPlayer()
		{
			if (AnimationPlayer == null)
			{
				GD.PrintErr("'AnimationPlayer' property field is empty, add one in the inspector field");
				return false;
			}

			if (!AnimationPlayer.HasAnimation("TRANSITION"))
			{
				GD.PrintErr("Could not find animation named 'TRANSITION' in 'AnimationPlayer', create new animation in the 'AnimationPlayer' and name it 'TRANSITION'");
				return false;
			}

			return true;
		}

		/// <summary>
		/// Prepares the container for the target scene and instantiates it.
		/// If no target scene is set, creates a preview scene.
		/// </summary>
		private void SetupTargetSceneRoot()
		{
			if (!HasNode("%target_scene"))
			{
				AddSceneNode("target_scene");
			}

			_targetSceneRoot = GetNode<Control>("%target_scene");

			if (_targetPackedScene == null)
			{
				// Create a preview scene if no target scene is specified
				AddDummySceneNode(_targetSceneRoot, Colors.MediumPurple, "Scene B");
			}
			else
			{
				// Instantiate the actual target scene
				_targetSceneNode = _targetPackedScene.Instantiate();
				_targetSceneRoot.AddChild(_targetSceneNode);
				_targetSceneNode.Name = TargetNodeName;
			}
		}

		/// <summary>
		/// Prepares the container for the current scene and adds it.
		/// If no current scene is set, creates a preview scene.
		/// </summary>
		private void SetupCurrentSceneRoot()
		{
			if (!HasNode("%current_scene"))
			{
				AddSceneNode("current_scene");
			}

			_currentSceneRoot = GetNode<Control>("%current_scene");

			if (_currentSceneNode == null)
			{
				// Create a preview scene if no current scene is set
				AddDummySceneNode(_currentSceneRoot, Colors.MediumSeaGreen, "Scene A");
			}
			else
			{
				_currentSceneRoot.AddChild(_currentSceneNode);
			}
		}

		/// <summary>
		/// Creates a new Control node to serve as a scene container with the given name.
		/// </summary>
		/// <param name="nodeName">Name for the new scene container node.</param>
		private void AddSceneNode(string nodeName)
		{
			var sceneNode = new Control();
			AddChild(sceneNode);
			sceneNode.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			sceneNode.Owner = this;
			sceneNode.Name = nodeName;
			sceneNode.UniqueNameInOwner = true;
		}

		/// <summary>
		/// Adds a dummy scene with a colored background and label for preview purposes.
		/// </summary>
		/// <param name="sceneNode">The container to add the dummy scene to.</param>
		/// <param name="backgroundColor">Background color for the dummy scene.</param>
		/// <param name="labelText">Text to display in the dummy scene.</param>
		private static void AddDummySceneNode(Control sceneNode, Color backgroundColor, string labelText)
		{
			var background = new ColorRect { Color = backgroundColor };
			background.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			sceneNode.AddChild(background);

			var label = new Label
			{
				Text = labelText,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			label.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			label.AddThemeFontSizeOverride("font_size", 50);
			background.AddChild(label);
		}
	}
}
