#if TOOLS
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using MoF.Addons.ScenesManager.Extensions;
using MoF.Addons.ScenesManager.Constants;
using MoF.Addons.ScenesManager.Helpers;
using MoF.Addons.ScenesManager.Scripts.Resources;

namespace MoF.Addons.ScenesManager.Scripts.Editor
{
	/// <summary>
	/// Represents a graph node for a scene in the Scenes Manager editor.
	/// Handles the UI and logic for connecting scenes and transitions.
	/// </summary>
	[Tool]
	public partial class SceneGraphNode : ScenesManagerBaseGraphNode
	{
		#region Fields

		// Scene and node references
		private Node _sceneRootNode;
		private Node _inSlotNode;
		private Array<Node> _outSlotNodes = new();
		private System.Collections.Generic.Dictionary<OutSlotSceneGraphNode, FoldableContainer> _outSlotContainers = new();
		private Button _addOutSlotButton;
		private EditorResourcePicker _sceneResourcePicker;
		private PackedScene _scene;
		private Array<string> _transitionNameList;

		// Static style resources
		private static StyleBoxFlat _sceneGraphNodeStylePanel;
		private static StyleBoxFlat _sceneGraphNodeStyleTitlebar;
		private static Theme _foldablePanelStyleTitlebar;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the PackedScene associated with this node.
		/// </summary>
		public PackedScene Scene
		{
			get => _scene;
			set => _scene = value;
		}

		/// <summary>
		/// Gets the list of signal names for all out slots.
		/// </summary>
		public override Array<string> OutSignalsNames =>
			[.. GetOutSlotNodes().Select(node => node.SignalSelect.GetItemText(node.SignalSelect.Selected))];

		/// <summary>
		/// Gets the list of transition PackedScene paths for all out slots.
		/// </summary>
		public Array<string> OutTransitionPackedScenePaths =>
			[.. GetOutSlotNodes().Select(node => GetTransitionPath(node))];

		/// <summary>
		/// Gets the list of transition modifiers for all out slots.
		/// </summary>
		public Array<TransitionModifier> OutTransitionModifers =>
			[.. GetOutSlotNodes().Select(node => GetTransitionModifier(node))];

		#endregion

		#region Initialization and Setup

		/// <summary>
		/// Loads required resources for the node (styles, themes, transitions).
		/// </summary>
		public override void _LoadResources()
		{
			LoadStyleResources();
			LoadTransitionResources();
		}

		/// <summary>
		/// Sets up the graph node's appearance and initial state.
		/// </summary>
		public override void _SetupGraphNode()
		{
			ConfigureNodeAppearance();
		}

		/// <summary>
		/// Called when the node is ready. Initializes the resource picker.
		/// </summary>
		public override void _ReadyNode() => CreateSceneResourcePicker();

		/// <summary>
		/// Loads style resources for the graph node.
		/// </summary>
		private void LoadStyleResources()
		{
			_sceneGraphNodeStylePanel ??= ResourceLoader.Load<StyleBoxFlat>(Plugin.PathToPlugin + AddonConstants.GraphNode.SceneGraphNode.GraphNodeStylePanelPath);
			_sceneGraphNodeStyleTitlebar ??= ResourceLoader.Load<StyleBoxFlat>(Plugin.PathToPlugin + AddonConstants.GraphNode.SceneGraphNode.GraphNodeStyleTitlebarPath);
			_foldablePanelStyleTitlebar ??= ResourceLoader.Load<Theme>(Plugin.PathToPlugin + AddonConstants.GraphNode.SceneGraphNode.FoldablePanelStyleTitlebarPath);
		}

		/// <summary>
		/// Loads transition resources.
		/// </summary>
		private void LoadTransitionResources()
		{
			_transitionNameList = FileSystemHelper.DirScenes<TransitionNode>(Plugin.PathToPlugin + AddonConstants.TransitionFolderPath, false, "*.tscn");
		}

		/// <summary>
		/// Configures the node's visual appearance.
		/// </summary>
		private void ConfigureNodeAppearance()
		{
			Title = AddonConstants.GraphNode.SceneGraphNode.Title;
			Size = AddonConstants.GraphNode.SceneGraphNode.InitialSize;
			Set("theme_override_constants/separation", AddonConstants.GraphNode.NodeVerticalSpace);
			Set("theme_override_styles/panel", _sceneGraphNodeStylePanel);
			Set("theme_override_styles/titlebar", _sceneGraphNodeStyleTitlebar);
		}

		#endregion

		#region Scene and Node Management

		/// <summary>
		/// Creates the resource picker for selecting a scene.
		/// </summary>
		private void CreateSceneResourcePicker()
		{
			_sceneResourcePicker = new EditorResourcePicker { BaseType = nameof(PackedScene) };
			AddChild(_sceneResourcePicker);

			if (Scene != null)
			{
				_sceneResourcePicker.EditedResource = Scene;
				SetSceneGraphNode(Scene);
			}
			_sceneResourcePicker.ResourceChanged += OnSceneResourcePickerChanged;
		}

		/// <summary>
		/// Removes and frees all child nodes and resets the out slot list.
		/// </summary>
		private void ResetGraphNode()
		{
			RemoveAndFreeNode(ref _inSlotNode);
			RemoveAndFreeNode(ref _addOutSlotButton);

			this.RemoveChildren(_outSlotNodes.ToArray());
			_outSlotNodes.Clear();
			_outSlotContainers.Clear();
		}

		/// <summary>
		/// Removes and frees a node if it exists.
		/// </summary>
		/// <typeparam name="T">Type of the node.</typeparam>
		/// <param name="node">Reference to the node to remove and free.</param>
		private void RemoveAndFreeNode<T>(ref T node) where T : Node
		{
			if (node != null)
			{
				RemoveChild(node);
				node.QueueFree();
				node = null;
			}
		}

		/// <summary>
		/// Sets up the graph node UI and logic for the given scene.
		/// </summary>
		/// <param name="packedScene">The scene to display in this node.</param>
		private void SetSceneGraphNode(PackedScene packedScene)
		{
			_sceneRootNode = packedScene.Instantiate<Node>();
			Title = GodotHelpers.GetSceneGraphNodeTitle(_sceneRootNode);
			CreateInSlotNode();
			CreateAddOutSlotButton();
			CreateSignalSlots();
			UpdateAddOutSlotButtonState();
			EmitSignal(SignalName.GraphNodeReady);
		}

		/// <summary>
		/// Creates out slot nodes for all signals to load.
		/// </summary>
		private void CreateSignalSlots()
		{
			foreach (var outSignals in OutSignalsToLoad)
			{
				CreateOutSlotNode(outSignals.OutSlotSignalName, outSignals.TransitionFileName, outSignals.TransitionModifier);
			}
		}

		#endregion

		#region Slot Management

		/// <summary>
		/// Creates the input slot node.
		/// </summary>
		private void CreateInSlotNode()
		{
			_inSlotNode = new Label { Text = "In" };
			var fontFileBold = ResourceLoader.Load<FontFile>("res://addons/ScenesManager/Assets/Fonts/JetBrainsMono-Bold.ttf");
			_inSlotNode.Set("theme_override_colors/font_color", AddonConstants.GraphNode.SceneGraphNode.Color);
			_inSlotNode.Set("theme_override_fonts/font", fontFileBold);
			AddChild(_inSlotNode);
			SetSlot(_inSlotNode.GetIndex(), true, 0, AddonConstants.GraphNode.SceneGraphNode.Color, false, 0, AddonConstants.GraphNode.SceneGraphNode.Color);
		}

		/// <summary>
		/// Creates the button for adding new out slots.
		/// </summary>
		private void CreateAddOutSlotButton()
		{
			_addOutSlotButton = new Button { Text = GetAddOutSlotButtonText() };
			_addOutSlotButton.Pressed += OnAddOutSlot;
			AddChild(_addOutSlotButton);
		}

		/// <summary>
		/// Creates an out slot node with optional signal and transition data.
		/// </summary>
		/// <param name="signalName">Signal name for the out slot.</param>
		/// <param name="transitionPath">Transition scene path.</param>
		/// <param name="transitionModifiers">Transition modifier data.</param>
		private void CreateOutSlotNode(string signalName = "", string transitionPath = "", TransitionModifier transitionModifiers = null)
		{
			var outSignalNode = new OutSlotSceneGraphNode(_sceneRootNode, _transitionNameList, signalName, transitionPath);

			outSignalNode.DeleteButtonPressed += () => OnDeleteOutSignalNode(outSignalNode);

			outSignalNode.SelectTransitionChanged += (isTransitionSelect) =>
			{
				OnTransitionSelectionChanged(outSignalNode, isTransitionSelect, transitionModifiers);
			};

			AddChild(outSignalNode);
			_outSlotNodes.Add(outSignalNode);

			SetSlot(outSignalNode.GetIndex(), false, 0, AddonConstants.GraphNode.SceneGraphNode.Color, true, 0, AddonConstants.GraphNode.SceneGraphNode.Color);

			// Create initial container if there's a transition path
			if (!string.IsNullOrEmpty(transitionPath))
			{
				CreateTransitionContainer(outSignalNode, transitionModifiers);
			}
		}

		/// <summary>
		/// Handles transition selection changes by destroying and recreating containers.
		/// </summary>
		/// <param name="outSignalNode">The out slot node.</param>
		/// <param name="isTransitionSelect">Whether a transition is selected.</param>
		/// <param name="transitionModifiers">Initial transition modifiers.</param>
		private void OnTransitionSelectionChanged(OutSlotSceneGraphNode outSignalNode, bool isTransitionSelect, TransitionModifier transitionModifiers = null)
		{
			// Destroy existing container if it exists
			DestroyTransitionContainer(outSignalNode);

			if (isTransitionSelect)
			{
				// Create new container
				CreateTransitionContainer(outSignalNode, transitionModifiers);
			}
			else
			{
				SetSize(AddonConstants.GraphNode.SceneGraphNode.InitialSize);
			}
		}

		/// <summary>
		/// Creates a transition container for the specified out slot node.
		/// </summary>
		/// <param name="outSignalNode">The out slot node.</param>
		/// <param name="transitionModifiers">Initial transition modifiers.</param>
		private void CreateTransitionContainer(OutSlotSceneGraphNode outSignalNode, TransitionModifier transitionModifiers = null)
		{
			var outSignalModifiersContainer = CreateTransitionOptionsContainer();

			outSignalModifiersContainer.FoldingChanged += (folded) =>
			{
				if (folded)
					SetSize(AddonConstants.GraphNode.SceneGraphNode.InitialSize);
			};

			// Add the container right after the out slot node
			var outSlotIndex = outSignalNode.GetIndex();
			AddChild(outSignalModifiersContainer);
			MoveChild(outSignalModifiersContainer, outSlotIndex + 1);
			_outSlotContainers[outSignalNode] = outSignalModifiersContainer;

			// Reorganize all slots after moving nodes
			ReorganizeAllSlots();

			if (transitionModifiers?.Speed == 1.0f && transitionModifiers?.Color == Colors.Black)
				outSignalModifiersContainer.Folded = true;

			CreateTransitionModifiersContent(outSignalModifiersContainer, outSignalNode, transitionModifiers);
		}

		/// <summary>
		/// Destroys the transition container for the specified out slot node.
		/// </summary>
		/// <param name="outSignalNode">The out slot node.</param>
		private void DestroyTransitionContainer(OutSlotSceneGraphNode outSignalNode)
		{
			if (_outSlotContainers.TryGetValue(outSignalNode, out var container))
			{
				RemoveChild(container);
				container.QueueFree();
				_outSlotContainers.Remove(outSignalNode);

				// Reorganize all slots after removing nodes
				ReorganizeAllSlots();
			}
		}

		/// <summary>
		/// Creates a foldable container for transition options.
		/// </summary>
		/// <returns>A new FoldableContainer instance.</returns>
		private FoldableContainer CreateTransitionOptionsContainer() =>
			new()
			{
				Title = AddonConstants.GraphNode.SceneGraphNode.TransitionFolderContainerLabelText,
				Theme = _foldablePanelStyleTitlebar
			};

		/// <summary>
		/// Creates the UI for editing transition modifiers (speed and color).
		/// </summary>
		/// <param name="container">The parent container for the controls.</param>
		/// <param name="outSignalNode">The out slot node to bind values to.</param>
		/// <param name="transitionModifiers">Initial modifier values.</param>
		private void CreateTransitionModifiersContent(FoldableContainer container, OutSlotSceneGraphNode outSignalNode, TransitionModifier transitionModifiers)
		{
			var vBox = new VBoxContainer();
			var hBoxSpeed = new HBoxContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
			var hBoxColor = new HBoxContainer { SizeFlagsVertical = SizeFlags.ExpandFill };

			var labelSpeed = new Label
			{
				Text = "speed:",
				SizeFlagsHorizontal = SizeFlags.ShrinkBegin,
			};
			var labelSpeedValue = new Label
			{
				Text = (transitionModifiers?.Speed ?? 1.0f).ToString() + "x",
				SizeFlagsHorizontal = SizeFlags.Fill,
				HorizontalAlignment = HorizontalAlignment.Right,
				CustomMinimumSize = new Vector2(30, 0),
			};

			var labelColor = new Label
			{
				Text = "Color:",
				SizeFlagsHorizontal = SizeFlags.ShrinkBegin,
			};

			var speedSlider = new HSlider
			{
				MinValue = 0,
				MaxValue = 2.0f,
				Step = 0.1,
				Value = transitionModifiers?.Speed ?? 1.0f,
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical = SizeFlags.ShrinkCenter,
			};

			var colorPicker = new ColorPickerButton
			{
				Color = transitionModifiers?.Color ?? Colors.Black,
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical = SizeFlags.ExpandFill,
			};

			colorPicker.ColorChanged += color => outSignalNode.TransitionModifier.Color = color;

			speedSlider.ValueChanged += value =>
			{
				outSignalNode.TransitionModifier.Speed = (float)value;
				labelSpeedValue.Text = value.ToString() + "x";
			};

			hBoxSpeed.AddChild(labelSpeed);
			hBoxSpeed.AddChild(speedSlider);
			hBoxSpeed.AddChild(labelSpeedValue);

			hBoxColor.AddChild(labelColor);
			hBoxColor.AddChild(colorPicker);

			vBox.AddChild(hBoxSpeed);
			vBox.AddChild(hBoxColor);

			container.AddChild(vBox);
		}

		/// <summary>
		/// Updates the add out slot button's text and enabled state.
		/// </summary>
		private void UpdateAddOutSlotButtonState()
		{
			_addOutSlotButton.Text = GetAddOutSlotButtonText();
			_addOutSlotButton.Disabled = _outSlotNodes.Count >= AddonConstants.GraphNode.MaxNumberOfOutSlots;
		}

		/// <summary>
		/// Gets the display text for the add out slot button.
		/// </summary>
		/// <returns>The button text.</returns>
		private string GetAddOutSlotButtonText() =>
			$"Add Out slot {_outSlotNodes.Count}/{AddonConstants.GraphNode.MaxNumberOfOutSlots}";

		#endregion

		#region Helper Methods

		/// <summary>
		/// Gets the out slot nodes cast to the correct type.
		/// </summary>
		/// <returns>Collection of OutSlotSceneGraphNode instances.</returns>
		private IEnumerable<OutSlotSceneGraphNode> GetOutSlotNodes() =>
			_outSlotNodes.Cast<OutSlotSceneGraphNode>();

		/// <summary>
		/// Gets the transition path for a given out slot node.
		/// </summary>
		/// <param name="node">The out slot node.</param>
		/// <returns>The transition path or empty string.</returns>
		private string GetTransitionPath(OutSlotSceneGraphNode node) =>
			node.TransitionSelect.Selected != 0
				? node.TransitionPaths[node.TransitionSelect.Selected - 1]
				: "";

		/// <summary>
		/// Gets the transition modifier for a given out slot node.
		/// </summary>
		/// <param name="node">The out slot node.</param>
		/// <returns>The transition modifier or a default one.</returns>
		private TransitionModifier GetTransitionModifier(OutSlotSceneGraphNode node) =>
			node.TransitionSelect.Selected != 0
				? node.TransitionModifier
				: new TransitionModifier();

		/// <summary>
		/// Reorganizes all slots to match the current node indices after node movements.
		/// </summary>
		private void ReorganizeAllSlots()
		{
			// Clear all existing slots
			ClearAllSlots();

			// Re-set the input slot
			SetupInputSlot();

			// Re-set all output slots
			SetupOutputSlots();
		}

		/// <summary>
		/// Sets up the input slot if the input node exists.
		/// </summary>
		private void SetupInputSlot()
		{
			if (_inSlotNode != null)
			{
				SetSlot(_inSlotNode.GetIndex(), true, 0, AddonConstants.GraphNode.SceneGraphNode.Color, false, 0, AddonConstants.GraphNode.SceneGraphNode.Color);
			}
		}

		/// <summary>
		/// Sets up all output slots.
		/// </summary>
		private void SetupOutputSlots()
		{
			foreach (OutSlotSceneGraphNode outSlotNode in _outSlotNodes)
			{
				SetSlot(outSlotNode.GetIndex(), false, 0, AddonConstants.GraphNode.SceneGraphNode.Color, true, 0, AddonConstants.GraphNode.SceneGraphNode.Color);
			}
		}

		#endregion


		#region Event Handlers

		/// <summary>
		/// Handles changes to the scene resource picker.
		/// </summary>
		/// <param name="resource">The selected resource.</param>
		private void OnSceneResourcePickerChanged(Resource resource)
		{
			if (resource is PackedScene packedScene)
			{
				_scene = packedScene;
				ResetGraphNode();
				SetSceneGraphNode(packedScene);
			}
			else
			{
				ResetGraphNode();
			}
		}

		/// <summary>
		/// Handles the add out slot button press event.
		/// </summary>
		private void OnAddOutSlot()
		{
			if (_outSlotNodes.Count < AddonConstants.GraphNode.MaxNumberOfOutSlots)
			{
				CreateOutSlotNode();
			}
			UpdateAddOutSlotButtonState();
		}

		/// <summary>
		/// Handles the deletion of an out signal node and its transition container.
		/// </summary>
		/// <param name="node">The out slot node to remove.</param>
		private void OnDeleteOutSignalNode(OutSlotSceneGraphNode node)
		{
			// Destroy the associated transition container
			DestroyTransitionContainer(node);

			// Remove and free the out slot node
			RemoveChild(node);
			node.QueueFree();
			_outSlotNodes.Remove(node);

			// Reorganize all slots after removing the out slot node
			ReorganizeAllSlots();

			UpdateAddOutSlotButtonState();
			SetSize(AddonConstants.GraphNode.SceneGraphNode.InitialSize);
		}

		#endregion
	}
}
#endif