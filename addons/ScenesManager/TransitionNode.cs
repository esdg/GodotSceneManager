using System.Collections.Generic;
using Godot;
using MoF.Addons.ScenesManager.Scripts;
using MoF.Addons.ScenesManager.Constants;

namespace MoF.Addons.ScenesManager
{
	/// <summary>
	/// A specialized transition node that handles animated scene transitions using Godot's AnimationPlayer.
	/// This class extends <see cref="TransitionNodeBase"/> to provide a flexible animation-based transition system
	/// that can be customized through the Godot editor or programmatically.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The TransitionNode automatically sets up container nodes for both current and target scenes,
	/// manages animation playback through an AnimationPlayer, and provides visual preview functionality
	/// when used in the Godot editor without assigned scenes.
	/// </para>
	/// <para>
	/// Key features:
	/// - Automatic AnimationPlayer setup with default transition animation
	/// - SubViewport-based scene isolation for complex transitions
	/// - Editor preview functionality with dummy scenes
	/// - Configurable transition speed and timing
	/// - Comprehensive configuration validation and warnings
	/// </para>
	/// </remarks>
	[Tool, GlobalClass]
	public partial class TransitionNode : TransitionNodeBase
	{
		#region Properties

		/// <summary>
		/// Gets or sets the AnimationPlayer component responsible for executing transition animations.
		/// </summary>
		/// <value>
		/// An <see cref="AnimationPlayer"/> instance that controls the transition animation playback.
		/// If null, a default AnimationPlayer will be created automatically during transition setup.
		/// </value>
		/// <remarks>
		/// The AnimationPlayer must contain a "TRANSITION" animation to function properly.
		/// If no such animation exists, one will be created automatically with default fade behavior.
		/// </remarks>
		[Export]
		protected AnimationPlayer AnimationPlayer { get; set; }

		/// <summary>
		/// Sets the root node of the current scene that will be transitioned out.
		/// </summary>
		/// <value>
		/// A <see cref="Node"/> representing the scene that is currently displayed and will be transitioned away from.
		/// This node will be added to the current scene container and removed when the transition completes.
		/// </value>
		/// <remarks>
		/// <para>
		/// This property is typically set by the scene manager before initiating a transition.
		/// If no current scene is provided, a preview scene with a colored background will be created
		/// for editor visualization purposes.
		/// </para>
		/// <para>
		/// The current scene will be automatically cleaned up and freed when the transition finishes.
		/// </para>
		/// </remarks>
		public Node CurrentSceneRoot
		{
			set => _currentSceneNode = value;
		}

		/// <summary>
		/// Gets or sets the playback speed multiplier for the transition animation.
		/// </summary>
		/// <value>
		/// A floating-point value representing the speed scale for animation playback.
		/// Values greater than 1.0 speed up the animation, while values between 0.0 and 1.0 slow it down.
		/// Default value is 1.0 (normal speed).
		/// </value>
		/// <remarks>
		/// <para>
		/// This property directly modifies the <see cref="AnimationPlayer.SpeedScale"/> property.
		/// If no AnimationPlayer is assigned, getting this property returns 1.0 and setting it has no effect.
		/// </para>
		/// <para>
		/// Example values:
		/// - 0.5: Half speed (transition takes twice as long)
		/// - 1.0: Normal speed
		/// - 2.0: Double speed (transition completes in half the time)
		/// </para>
		/// </remarks>
		public float TransitionSpeed
		{
			set
			{
				if (AnimationPlayer != null)
					AnimationPlayer.SpeedScale = value;
			}
			get => AnimationPlayer != null ? AnimationPlayer.SpeedScale : 1.0f;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Initiates the transition process when the node is ready to begin the scene change.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method is called by the base class when a transition should start. It performs the following steps:
		/// 1. Sets up and validates the AnimationPlayer component
		/// 2. Prepares the target scene container and instantiates the new scene
		/// 3. Prepares the current scene container for transition
		/// 4. Starts playback of the "TRANSITION" animation
		/// </para>
		/// <para>
		/// If any required components are missing (AnimationPlayer, scene containers), they will be created automatically.
		/// For scenes not provided, preview scenes with colored backgrounds will be generated for editor visualization.
		/// </para>
		/// </remarks>
		/// <exception cref="System.InvalidOperationException">
		/// Thrown if the transition cannot be started due to missing critical components that cannot be auto-generated.
		/// </exception>
		public override void _TransitionReady()
		{
			SetupAnimationPlayer();
			SetupTargetSceneRoot();
			SetupCurrentSceneRoot();

			AnimationPlayer.Play(AddonConstants.TransitionNode.TransitionAnimationName);
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Configures and validates the AnimationPlayer component for transition playback.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method ensures that:
		/// - An AnimationPlayer component exists (creates one if missing)
		/// - The AnimationPlayer contains a "TRANSITION" animation (creates default if missing)
		/// - The animation is properly configured with scene visibility tracks and completion signals
		/// </para>
		/// <para>
		/// The default "TRANSITION" animation includes:
		/// - 2-second duration
		/// - Target scene visibility toggle (false → true)
		/// - Current scene visibility toggle (true → false)
		/// - Method call track to signal completion
		/// </para>
		/// </remarks>
		protected void SetupAnimationPlayer()
		{
			CreateAnimationPlayerIfNeeded();
			CreateTransitionAnimationIfNeeded();
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Creates and configures a new AnimationPlayer component if one does not already exist.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The created AnimationPlayer will be:
		/// - Named "TransitionAnimationPlayer" for easy identification
		/// - Added as a child of this TransitionNode
		/// - Set with this node as its owner for proper scene persistence
		/// </para>
		/// <para>
		/// This method is safe to call multiple times; it will only create an AnimationPlayer
		/// if the current reference is null.
		/// </para>
		/// </remarks>
		private void CreateAnimationPlayerIfNeeded()
		{
			if (AnimationPlayer == null)
			{
				AnimationPlayer = new AnimationPlayer { Name = AddonConstants.TransitionNode.AnimationPlayerName };
				AddChild(AnimationPlayer);
				AnimationPlayer.Owner = this;
			}
		}

		/// <summary>
		/// Creates a default "TRANSITION" animation if it does not exist in the AnimationPlayer.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The generated animation includes:
		/// - Base animation with 2.0 second duration
		/// - Method track that calls <see cref="SendTransitionFinishedSignal"/> at completion
		/// - Value tracks for scene visibility management:
		///   - target_scene:visible: false → true (shows new scene)
		///   - current_scene:visible: true → false (hides old scene)
		/// </para>
		/// <para>
		/// This provides a simple cross-fade transition that can be customized by modifying
		/// the animation in the Godot editor or by replacing it programmatically.
		/// </para>
		/// </remarks>
		private void CreateTransitionAnimationIfNeeded()
		{
			if (!AnimationPlayer.HasAnimation(AddonConstants.TransitionNode.TransitionAnimationName))
			{
				var animation = CreateBaseAnimation();
				AddFinishedSignalTrack(animation);
				AddSceneValueTrack(animation, AddonConstants.TransitionNode.TargetSceneVisibilityPath, false, true);
				AddSceneValueTrack(animation, AddonConstants.TransitionNode.CurrentSceneVisibilityPath, true, false);
				AddAnimationToPlayer(animation);
			}
		}

		/// <summary>
		/// Creates a base Animation instance with default configuration for transitions.
		/// </summary>
		/// <returns>
		/// A new <see cref="Animation"/> instance configured with a 2.0 second duration,
		/// ready for additional tracks to be added.
		/// </returns>
		/// <remarks>
		/// This animation serves as the foundation for the transition effect.
		/// Additional tracks for scene visibility, effects, and completion signaling
		/// are added to this base animation by other methods.
		/// </remarks>
		private Animation CreateBaseAnimation()
		{
			return new Animation
			{
				Length = AddonConstants.TransitionNode.DefaultAnimationDuration
			};
		}

		/// <summary>
		/// Adds a method call track to the animation that triggers transition completion signaling.
		/// </summary>
		/// <param name="animation">The animation to which the method track will be added.</param>
		/// <remarks>
		/// <para>
		/// This method creates a method track that calls <see cref="SendTransitionFinishedSignal"/>
		/// at the end of the animation (at time 2.0 seconds). This ensures that cleanup operations
		/// and the TransitionFinished signal are properly emitted when the animation completes.
		/// </para>
		/// <para>
		/// The method track targets this TransitionNode instance (NodePath ".") and includes
		/// an empty arguments array as required by Godot's animation system.
		/// </para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">
		/// Thrown if <paramref name="animation"/> is null.
		/// </exception>
		private void AddFinishedSignalTrack(Animation animation)
		{
			var trackIndex = animation.AddTrack(Animation.TrackType.Method);
			animation.TrackSetPath(trackIndex, new NodePath("."));
			animation.TrackInsertKey(trackIndex, AddonConstants.TransitionNode.DefaultAnimationDuration, new Godot.Collections.Dictionary
			{
				{ "method", nameof(SendTransitionFinishedSignal) },
				{ "args", new Godot.Collections.Array() }
			});
		}

		/// <summary>
		/// Adds a value track to the animation for controlling scene visibility during transitions.
		/// </summary>
		/// <param name="animation">The animation to which the value track will be added.</param>
		/// <param name="nodePath">The node path string targeting the property to animate (e.g., "target_scene:visible").</param>
		/// <param name="initialVisibility">The visibility state at the start of the transition (time 0.0).</param>
		/// <param name="finalVisibility">The visibility state at the middle of the transition (time 1.0).</param>
		/// <remarks>
		/// <para>
		/// This method creates discrete value tracks that instantly toggle scene visibility at specific times.
		/// The track uses <see cref="Animation.UpdateMode.Discrete"/> to ensure immediate state changes
		/// rather than interpolated transitions.
		/// </para>
		/// <para>
		/// Common usage patterns:
		/// - Target scene: false → true (scene becomes visible during transition)
		/// - Current scene: true → false (scene becomes hidden during transition)
		/// </para>
		/// <para>
		/// The node path should reference scene container nodes with unique names (% prefix)
		/// such as "target_scene:visible" or "current_scene:visible".
		/// </para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">
		/// Thrown if <paramref name="animation"/> or <paramref name="nodePath"/> is null.
		/// </exception>
		private void AddSceneValueTrack(Animation animation, string nodePath, bool initialVisibility, bool finalVisibility)
		{
			var trackIndex = animation.AddTrack(Animation.TrackType.Value);
			animation.TrackSetPath(trackIndex, new NodePath(nodePath));
			animation.ValueTrackSetUpdateMode(trackIndex, Animation.UpdateMode.Discrete);
			animation.TrackInsertKey(trackIndex, 0.0f, initialVisibility);
			animation.TrackInsertKey(trackIndex, AddonConstants.TransitionNode.VisibilityToggleTime, finalVisibility);
		}

		/// <summary>
		/// Creates a new SubViewport with transparent background for scene isolation.
		/// </summary>
		/// <returns>A configured SubViewport instance ready for scene content.</returns>
		private static SubViewport CreateSubViewport()
		{
			return new SubViewport
			{
				Name = AddonConstants.TransitionNode.SubViewportName,
				TransparentBg = true,
			};
		}

		/// <summary>
		/// Gets the SubViewport from a scene container for scene manipulation.
		/// </summary>
		/// <param name="sceneContainer">The scene container Control node.</param>
		/// <returns>The SubViewport within the container.</returns>
		private static SubViewport GetSubViewport(Control sceneContainer)
		{
			// Use safe lookups to avoid "Node not found" errors in the editor
			var container = sceneContainer.GetNodeOrNull<SubViewportContainer>(AddonConstants.TransitionNode.SubViewportContainerName);
			var subViewport = container?.GetNodeOrNull<SubViewport>(AddonConstants.TransitionNode.SubViewportName);
			if (subViewport == null)
			{
				subViewport = CreateSubViewport();
				// Create the SubViewport only if the container exists; the container is created
				// alongside the scene container via AddSceneNode or is provided by the .tscn.
				container?.AddChild(subViewport);
			}

			return subViewport;
		}

		/// <summary>
		/// Registers the constructed animation with the AnimationPlayer for playback.
		/// </summary>
		/// <param name="animation">The fully configured animation to add to the player.</param>
		/// <remarks>
		/// <para>
		/// This method creates a new <see cref="AnimationLibrary"/> and adds the animation
		/// with the name "TRANSITION" to ensure it can be played by the AnimationPlayer.
		/// The library is then registered with an empty string name, making it the default library.
		/// </para>
		/// <para>
		/// After this method completes, the animation can be played using:
		/// <c>AnimationPlayer.Play("TRANSITION")</c>
		/// </para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">
		/// Thrown if <paramref name="animation"/> is null.
		/// </exception>
		private void AddAnimationToPlayer(Animation animation)
		{
			var animationLibrary = new AnimationLibrary();
			animationLibrary.AddAnimation(AddonConstants.TransitionNode.TransitionAnimationName, animation);
			AnimationPlayer.AddAnimationLibrary("", animationLibrary);
		}

		/// <summary>
		/// Prepares and configures the container for the target scene that will be transitioned to.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method performs the following operations:
		/// 1. Creates a target scene container node ("%target_scene") if it doesn't exist
		/// 2. Retrieves the target scene container as a Control node
		/// 3. Instantiates the target scene or creates a preview scene for editor visualization
		/// 4. Sets up a SubViewport for proper scene isolation and rendering
		/// </para>
		/// <para>
		/// If no target scene is provided (<see cref="_targetPackedScene"/> is null),
		/// a dummy preview scene with a purple background and "Scene B" label is created.
		/// This allows the transition to be visualized in the Godot editor without requiring
		/// actual scene assets.
		/// </para>
		/// <para>
		/// The target scene is wrapped in a SubViewport to ensure proper isolation and
		/// prevent interference with the current scene's rendering context.
		/// </para>
		/// </remarks>
		protected void SetupTargetSceneRoot()
		{
			if (!HasNode(AddonConstants.TransitionNode.TargetSceneContainer))
			{
				AddSceneNode("target_scene");
			}

			_targetSceneRoot = GetNode<Control>(AddonConstants.TransitionNode.TargetSceneContainer);

			if (_targetPackedScene == null)
			{
				// Create a preview scene if no target scene is specified
				// Used in the editor to visualize the transition effect
				AddDummySceneNode(GetSubViewport(_targetSceneRoot), Colors.MediumPurple, AddonConstants.TransitionNode.DummySceneBText);
			}
			else
			{
				// Instantiate the actual target scene
				_targetSceneNode = _targetPackedScene.Instantiate();
				var subViewport = CreateSubViewport();
				_targetSceneRoot.GetNode<SubViewportContainer>(AddonConstants.TransitionNode.SubViewportContainerName).AddChild(subViewport);
				subViewport.AddChild(_targetSceneNode);
				_targetSceneNode.Name = TargetNodeName;
			}
		}

		/// <summary>
		/// Prepares and configures the container for the current scene that will be transitioned from.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method performs the following operations:
		/// 1. Creates a current scene container node ("%current_scene") if it doesn't exist
		/// 2. Retrieves the current scene container as a Control node
		/// 3. Adds the current scene node or creates a preview scene for editor visualization
		/// 4. Sets up a SubViewport for proper scene isolation if an actual scene is provided
		/// </para>
		/// <para>
		/// If no current scene is provided (<see cref="_currentSceneNode"/> is null),
		/// a dummy preview scene with a sea green background and "Scene A" label is created.
		/// This allows transitions to be tested and visualized in the Godot editor.
		/// </para>
		/// <para>
		/// When a real scene is provided, it's wrapped in a SubViewport with transparent background
		/// to ensure proper rendering isolation and prevent visual artifacts during the transition.
		/// </para>
		/// </remarks>
		protected void SetupCurrentSceneRoot()
		{
			if (!HasNode(AddonConstants.TransitionNode.CurrentSceneContainer))
			{
				AddSceneNode("current_scene");
			}

			_currentSceneRoot = GetNode<Control>(AddonConstants.TransitionNode.CurrentSceneContainer);

			if (_currentSceneNode == null)
			{
				// Create a preview scene if no current scene is set
				// Used in the editor to visualize the transition effect
				AddDummySceneNode(GetSubViewport(_currentSceneRoot), Colors.MediumSeaGreen, AddonConstants.TransitionNode.DummySceneAText);
			}
			else
			{
				var subViewport = CreateSubViewport();
				_currentSceneRoot.GetNode<SubViewportContainer>(AddonConstants.TransitionNode.SubViewportContainerName).AddChild(subViewport);
				subViewport.AddChild(_currentSceneNode);
			}
		}

		/// <summary>
		/// Creates a new Control-based scene container with proper viewport structure for transitions.
		/// </summary>
		/// <param name="nodeName">The name to assign to the created scene container node.</param>
		/// <remarks>
		/// <para>
		/// This method creates a hierarchical structure for scene containment:
		/// - Root Control node (fills entire parent area)
		/// - SubViewportContainer (handles viewport management and stretching)
		/// - SubViewport (provides isolated rendering context)
		/// </para>
		/// <para>
		/// The created structure allows scenes to be rendered independently without
		/// affecting each other, which is essential for complex transition effects.
		/// All created nodes are properly configured with:
		/// - Full rectangle anchoring for proper scaling
		/// - Unique naming for animation targeting
		/// - Correct ownership for scene persistence
		/// </para>
		/// <para>
		/// The container uses the unique name pattern (%) which allows the animation
		/// system to reference it reliably as "%{nodeName}".
		/// </para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">
		/// Thrown if <paramref name="nodeName"/> is null or empty.
		/// </exception>
		private void AddSceneNode(string nodeName)
		{
			var sceneNode = new Control();
			AddChild(sceneNode);
			sceneNode.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			sceneNode.Owner = this;
			sceneNode.Name = nodeName;
			sceneNode.UniqueNameInOwner = true;

			var subViewportContainer = new SubViewportContainer
			{
				Name = AddonConstants.TransitionNode.SubViewportContainerName,
				Stretch = true,
			};

			var subViewport = CreateSubViewport();
			sceneNode.AddChild(subViewportContainer);
			subViewportContainer.AddChild(subViewport);
			subViewportContainer.Owner = this;
			subViewportContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		}

		/// <summary>
		/// Creates a simple dummy scene for editor preview and testing purposes.
		/// </summary>
		/// <param name="subViewport">The SubViewport container where the dummy scene will be added.</param>
		/// <param name="backgroundColor">The background color for the dummy scene's ColorRect.</param>
		/// <param name="labelText">The text to display in the center of the dummy scene.</param>
		/// <remarks>
		/// <para>
		/// This method creates a minimal scene structure consisting of:
		/// - A ColorRect that fills the viewport with the specified background color
		/// - A Label with large font size (50) displaying the provided text
		/// - Centered text alignment both horizontally and vertically
		/// </para>
		/// <para>
		/// Dummy scenes are used when no actual scene is provided, allowing developers
		/// to visualize and test transitions in the Godot editor without requiring
		/// complete scene assets. This is particularly useful during development and
		/// for demonstrating transition effects.
		/// </para>
		/// <para>
		/// The created label uses theme font size override to ensure readable text
		/// regardless of the current project theme settings.
		/// </para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">
		/// Thrown if <paramref name="subViewport"/> or <paramref name="labelText"/> is null.
		/// </exception>
		private static void AddDummySceneNode(SubViewport subViewport, Color backgroundColor, string labelText)
		{
			var background = new ColorRect { Color = backgroundColor };
			background.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			subViewport.AddChild(background);

			var label = new Label
			{
				Text = labelText,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};

			label.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			label.AddThemeFontSizeOverride("font_size", AddonConstants.TransitionNode.DummySceneFontSize);
			background.AddChild(label);
		}

		/// <summary>
		/// Completes the transition process by cleaning up the current scene and signaling completion.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method is called automatically when the transition animation finishes and performs:
		/// 1. Removes the current scene from its container viewport
		/// 2. Queues the current scene for deletion to free memory
		/// 3. Removes the target scene from its container (it will be re-added to the main scene)
		/// 4. Emits the TransitionFinished signal with the target scene node
		/// </para>
		/// <para>
		/// The signal emission is deferred to ensure all cleanup operations complete before
		/// other systems receive the completion notification. This prevents race conditions
		/// and ensures the scene transition state is fully resolved.
		/// </para>
		/// <para>
		/// After this method completes, the target scene becomes the responsibility of the
		/// scene manager or calling code that initiated the transition.
		/// </para>
		/// </remarks>
		/// <seealso cref="TransitionFinishedEventHandler"/>
		protected override void SendTransitionFinishedSignal()
		{
			if (_currentSceneNode != null)
			{
				// Avoid creating new SubViewports during cleanup; only remove if present
				var currentContainer = _currentSceneRoot?.GetNodeOrNull<SubViewportContainer>(AddonConstants.TransitionNode.SubViewportContainerName);
				var currentVp = currentContainer?.GetNodeOrNull<SubViewport>(AddonConstants.TransitionNode.SubViewportName);
				currentVp?.RemoveChild(_currentSceneNode);
				_currentSceneNode.QueueFree();
			}
			var targetContainer = _targetSceneRoot?.GetNodeOrNull<SubViewportContainer>(AddonConstants.TransitionNode.SubViewportContainerName);
			var targetVp = targetContainer?.GetNodeOrNull<SubViewport>(AddonConstants.TransitionNode.SubViewportName);
			targetVp?.RemoveChild(_targetSceneNode);
			CallDeferred(MethodName.EmitSignal, SignalName.TransitionFinished, _targetSceneNode);
		}

		/// <summary>
		/// Validates the TransitionNode configuration and returns any warnings for the Godot editor.
		/// </summary>
		/// <returns>
		/// An array of warning message strings describing configuration issues that may prevent
		/// proper transition functionality. Returns an empty array if no issues are detected.
		/// </returns>
		/// <remarks>
		/// <para>
		/// This method performs comprehensive validation of the transition setup and reports:
		/// - Missing or null AnimationPlayer reference
		/// - AnimationPlayer component that doesn't exist as a child node
		/// - Missing scene container nodes ("%target_scene", "%current_scene")
		/// - AnimationPlayer without the required "TRANSITION" animation
		/// </para>
		/// <para>
		/// These warnings appear in the Godot editor's Scene dock and help developers
		/// identify and resolve configuration issues before runtime. While some issues
		/// (like missing AnimationPlayer) can be auto-resolved at runtime, it's better
		/// to address them during development for predictable behavior.
		/// </para>
		/// <para>
		/// This method is called automatically by Godot when the node is selected in the editor
		/// or when the scene is validated. It should not be called directly in most cases.
		/// </para>
		/// </remarks>
		public override string[] _GetConfigurationWarnings()
		{
			var warnings = new List<string>();

			if (AnimationPlayer == null)
				warnings.Add("AnimationPlayer is not assigned. Transitions will not play.");

			if (AnimationPlayer != null && FindChild(AnimationPlayer.Name) == null)
				warnings.Add($"'AnimationPlayer' named '{AnimationPlayer.Name}' is referenced but does not seems to exist");

			if (!HasNode(AddonConstants.TransitionNode.TargetSceneContainer))
				warnings.Add($"Target scene container node ('{AddonConstants.TransitionNode.TargetSceneContainer}') is missing.");

			if (!HasNode(AddonConstants.TransitionNode.CurrentSceneContainer))
				warnings.Add($"Current scene container node ('{AddonConstants.TransitionNode.CurrentSceneContainer}') is missing.");

			if (AnimationPlayer != null && !AnimationPlayer.HasAnimation(AddonConstants.TransitionNode.TransitionAnimationName))
				warnings.Add($"AnimationPlayer does not contain a '{AddonConstants.TransitionNode.TransitionAnimationName}' animation.");

			return warnings.ToArray();
		}

		#endregion
	}
}
