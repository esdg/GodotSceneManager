using System.Collections.Generic;
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
		/// Gets or sets the AnimationPlayer responsible for playing the transition animation.
		/// </summary>
		[Export]
		protected AnimationPlayer AnimationPlayer { get; set; }

		/// <summary>
		/// Sets the root node of the current scene to be transitioned out.
		/// </summary>
		public Node CurrentSceneRoot
		{
			set => _currentSceneNode = value;
		}

		/// <summary>
		/// Gets or sets the playback speed for the transition animation.
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
			SetupAnimationPlayer();
			SetupTargetSceneRoot();
			SetupCurrentSceneRoot();

			AnimationPlayer.Play("TRANSITION");
		}

		/// <summary>
		/// Ensures the AnimationPlayer exists and contains the required "TRANSITION" animation.
		/// </summary>
		protected void SetupAnimationPlayer()
		{
			CreateAnimationPlayerIfNeeded();
			CreateTransitionAnimationIfNeeded();
		}

		/// <summary>
		/// Creates an AnimationPlayer if one does not already exist.
		/// </summary>
		private void CreateAnimationPlayerIfNeeded()
		{
			if (AnimationPlayer == null)
			{
				AnimationPlayer = new AnimationPlayer { Name = "TransitionAnimationPlayer" };
				AddChild(AnimationPlayer);
				AnimationPlayer.Owner = this;
			}
		}

		/// <summary>
		/// Creates the "TRANSITION" animation if it does not already exist in the AnimationPlayer.
		/// </summary>
		private void CreateTransitionAnimationIfNeeded()
		{
			if (!AnimationPlayer.HasAnimation("TRANSITION"))
			{
				var animation = CreateBaseAnimation();
				AddFinishedSignalTrack(animation);
				AddSceneValueTrack(animation, "target_scene:visible", false, true);
				AddSceneValueTrack(animation, "current_scene:visible", true, false);
				AddAnimationToPlayer(animation);
			}
		}

		/// <summary>
		/// Creates a base Animation with a default length.
		/// </summary>
		/// <returns>A new Animation instance.</returns>
		private Animation CreateBaseAnimation()
		{
			return new Animation
			{
				Length = 2.0f
			};
		}

		/// <summary>
		/// Adds a method track to the animation that signals when the transition is finished.
		/// </summary>
		/// <param name="animation">The animation to modify.</param>
		private void AddFinishedSignalTrack(Animation animation)
		{
			var trackIndex = animation.AddTrack(Animation.TrackType.Method);
			animation.TrackSetPath(trackIndex, new NodePath("."));
			animation.TrackInsertKey(trackIndex, 2.0f, new Godot.Collections.Dictionary
			{
				{ "method", nameof(SendTransitionFinishedSignal) },
				{ "args", new Godot.Collections.Array() }
			});
		}

		/// <summary>
		/// Adds a value track to the animation for toggling scene visibility.
		/// </summary>
		/// <param name="animation">The animation to modify.</param>
		/// <param name="nodePath">The node path for the value track.</param>
		/// <param name="initialVisibility">Initial visibility value.</param>
		/// <param name="finalVisibility">Final visibility value.</param>
		private void AddSceneValueTrack(Animation animation, string nodePath, bool initialVisibility, bool finalVisibility)
		{
			var trackIndex = animation.AddTrack(Animation.TrackType.Value);
			animation.TrackSetPath(trackIndex, new NodePath(nodePath));
			animation.ValueTrackSetUpdateMode(trackIndex, Animation.UpdateMode.Discrete);
			animation.TrackInsertKey(trackIndex, 0.0f, initialVisibility);
			animation.TrackInsertKey(trackIndex, 1.0f, finalVisibility);
		}

		/// <summary>
		/// Adds the constructed animation to the AnimationPlayer.
		/// </summary>
		/// <param name="animation">The animation to add.</param>
		private void AddAnimationToPlayer(Animation animation)
		{
			var animationLibrary = new AnimationLibrary();
			animationLibrary.AddAnimation("TRANSITION", animation);
			AnimationPlayer.AddAnimationLibrary("", animationLibrary);
		}

		/// <summary>
		/// Prepares the container for the target scene and instantiates it.
		/// If no target scene is set, creates a preview scene.
		/// </summary>
		protected void SetupTargetSceneRoot()
		{
			if (!HasNode("%target_scene"))
			{
				AddSceneNode("target_scene");
			}

			_targetSceneRoot = GetNode<Control>("%target_scene");

			if (_targetPackedScene == null)
			{
				// Create a preview scene if no target scene is specified
				// Used in the editor to visualize the transition effect
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
		protected void SetupCurrentSceneRoot()
		{
			if (!HasNode("%current_scene"))
			{
				AddSceneNode("current_scene");
			}

			_currentSceneRoot = GetNode<Control>("%current_scene");

			if (_currentSceneNode == null)
			{
				// Create a preview scene if no current scene is set
				// Used in the editor to visualize the transition effect
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

		/// <summary>
		/// Returns a list of configuration warnings for this node in the Godot editor.
		/// </summary>
		/// <returns>A list of warning messages if configuration is incomplete; otherwise, an empty list.</returns>
		public override string[] _GetConfigurationWarnings()
		{
			var warnings = new List<string>();

			if (AnimationPlayer == null)
				warnings.Add("AnimationPlayer is not assigned. Transitions will not play.");

			if (FindChildren(AnimationPlayer.Name, "AnimationPlayer", false).Count == 0)
				warnings.Add($"'AnimationPlayer' named '{AnimationPlayer.Name}' is not a child of this node. It must be a direct child to function correctly.");

			if (!HasNode("%target_scene"))
				warnings.Add("Target scene container node ('%target_scene') is missing.");

			if (!HasNode("%current_scene"))
				warnings.Add("Current scene container node ('%current_scene') is missing.");

			if (AnimationPlayer != null && !AnimationPlayer.HasAnimation("TRANSITION"))
				warnings.Add("AnimationPlayer does not contain a 'TRANSITION' animation.");

			return warnings.ToArray();
		}
	}
}
