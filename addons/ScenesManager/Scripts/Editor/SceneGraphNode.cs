#if TOOLS
using System.Linq;
using Godot;
using Godot.Collections;
using MoF.Addons.ScenesManager.Extensions;
using MoF.Addons.ScenesManager.Constants;
using MoF.Addons.ScenesManager.Helpers;

namespace MoF.Addons.ScenesManager.Scripts.Editor
{
	[Tool]
	public partial class SceneGraphNode : ScenesManagerBaseGraphNode
	{

		public PackedScene Scene
		{
			get => _scene;
			set
			{
				_scene = value;
			}
		}

		public override Array<string> OutSignalsNames
		{
			get
			{
				var signals = new Array<string>();
				foreach (OutSlotSceneGraphNode node in _outSlotNodes.Cast<OutSlotSceneGraphNode>())
				{
					signals.Add(node.SignalSelect.GetItemText(node.SignalSelect.Selected));
				}
				return signals;
			}
		}

		public Array<string> OutTransitionPackedScenePaths
		{
			get
			{
				var transitions = new Array<string>();
				foreach (OutSlotSceneGraphNode node in _outSlotNodes.Cast<OutSlotSceneGraphNode>())
				{
					if (node.TransitionSelect.Selected != 0)
						transitions.Add(node.TransitionPaths[node.TransitionSelect.Selected - 1]);
					else
						transitions.Add("");
				}
				return transitions;
			}
		}


		private Node _sceneRootNode;
		private Node _inSlotNode;
		private Array<Node> _outSlotNodes = new(); // probably remove this? using children?
		private Button _addOutSlotButton;
		private EditorResourcePicker _sceneResourcePicker;
		private PackedScene _scene;
		private Array<string> _transitionNameList;

		private static StyleBoxFlat _sceneGraphNodeStylePanel;
		private static StyleBoxFlat _sceneGraphNodeStyleTitlebar;

		public override void _LoadResources()
		{
			_sceneGraphNodeStylePanel ??= ResourceLoader.Load<StyleBoxFlat>(Plugin.PathToPlugin + AddonConstants.GraphNode.SceneGraphNode.GraphNodeStylePanelPath);
			_sceneGraphNodeStyleTitlebar ??= ResourceLoader.Load<StyleBoxFlat>(Plugin.PathToPlugin + AddonConstants.GraphNode.SceneGraphNode.GraphNodeStyleTitlebarPath);
			_transitionNameList = FileSystemHelper.DirScenes<TransitionNode>(Plugin.PathToPlugin + AddonConstants.TransitionFolderPath, false, "*.tscn");
		}

		public override void _SetupGraphNode()
		{
			Title = AddonConstants.GraphNode.SceneGraphNode.Title;
			Size = AddonConstants.GraphNode.SceneGraphNode.InitialSize;
			Set("theme_override_constants/separation", AddonConstants.GraphNode.NodeVerticalSpace);
			Set("theme_override_styles/panel", _sceneGraphNodeStylePanel);
			Set("theme_override_styles/titlebar", _sceneGraphNodeStyleTitlebar);
		}

		public override void _ReadyNode()
		{
			CreateSceneResourcePicker();
		}

		private void CreateSceneResourcePicker()
		{
			_sceneResourcePicker = new EditorResourcePicker
			{
				BaseType = nameof(PackedScene)
			};
			AddChild(_sceneResourcePicker);

			if (Scene != null)
			{
				_sceneResourcePicker.EditedResource = Scene;
				SetSceneGraphNode(Scene);
			}
			_sceneResourcePicker.ResourceChanged += OnSceneResourcePickerChanged;

		}

		private void InitializeGraphNode()
		{
			if (_inSlotNode != null)
			{
				RemoveChild(_inSlotNode);
				_inSlotNode.QueueFree();
			}

			if (_addOutSlotButton != null)
			{
				RemoveChild(_addOutSlotButton);
				_addOutSlotButton.QueueFree();
			}

			this.RemoveChildren(_outSlotNodes.ToArray());
			_outSlotNodes.Clear();
		}

		private void SetSceneGraphNode(PackedScene packedScene)
		{
			_sceneRootNode = packedScene.Instantiate<Node>();
			Title = GodotHelpers.GetSceneGraphNodeTitle(_sceneRootNode);
			CreateInSlotNode();
			CreateAddOutSlotButton();
			SetSignalsSlot();
			UpdateAddOutSlotButtonState();
			EmitSignal(SignalName.GraphNodeReady);
		}

		private void SetSignalsSlot()
		{
			foreach (var outSignals in OutSignalsToLoad)
			{
				CreateOutSlotNode(outSignals.OutSlotSignalName, outSignals.TransitionFileName);
			}
		}

		private void CreateInSlotNode()
		{
			_inSlotNode = new Label { Text = "In" };
			FontFile fontFileBold = ResourceLoader.Load<FontFile>("res://addons/ScenesManager/Assets/Fonts/JetBrainsMono-Bold.ttf");
			_inSlotNode.Set("theme_override_colors/font_color", AddonConstants.GraphNode.SceneGraphNode.Color);
			_inSlotNode.Set("theme_override_fonts/font", fontFileBold);
			AddChild(_inSlotNode);
			SetSlot(_inSlotNode.GetIndex(), true, 0, AddonConstants.GraphNode.SceneGraphNode.Color, false, 0, AddonConstants.GraphNode.SceneGraphNode.Color);
		}

		private void CreateAddOutSlotButton()
		{
			_addOutSlotButton = new Button
			{
				Text = GetAddOutSlotButtonText()
			};
			_addOutSlotButton.Pressed += OnAddOutSlot;
			AddChild(_addOutSlotButton);
		}

		private void CreateOutSlotNode(string signalName = "", string transitionPath = "")
		{
			var outSignalNode = new OutSlotSceneGraphNode(_sceneRootNode, _transitionNameList, signalName, transitionPath);
			outSignalNode.DeleteButtonPressed += () => OnDeleteOutSignalNode(outSignalNode);

			AddChild(outSignalNode);
			_outSlotNodes.Add(outSignalNode);
			SetSlot(outSignalNode.GetIndex(), false, 0, AddonConstants.GraphNode.SceneGraphNode.Color, true, 0, AddonConstants.GraphNode.SceneGraphNode.Color);
		}

		private void UpdateAddOutSlotButtonState()
		{
			_addOutSlotButton.Text = GetAddOutSlotButtonText();
			_addOutSlotButton.Disabled = _outSlotNodes.Count >= AddonConstants.GraphNode.MaxNumberOfOutSlots;
		}

		private string GetAddOutSlotButtonText()
		{
			return $"Add Out slot {_outSlotNodes.Count}/{AddonConstants.GraphNode.MaxNumberOfOutSlots}";
		}

		private void OnSceneResourcePickerChanged(Resource resource)
		{
			if (resource is PackedScene packedScene)
			{
				_scene = packedScene;
				InitializeGraphNode();
				SetSceneGraphNode(packedScene);
			}
			else
			{
				InitializeGraphNode();
			}
		}

		private void OnAddOutSlot()
		{
			if (_outSlotNodes.Count < AddonConstants.GraphNode.MaxNumberOfOutSlots)
			{
				CreateOutSlotNode();
			}
			UpdateAddOutSlotButtonState();
		}

		private void OnDeleteOutSignalNode(Node node)
		{
			RemoveChild(node);
			_outSlotNodes.Remove(node);
			UpdateAddOutSlotButtonState();
			SetSize(AddonConstants.GraphNode.SceneGraphNode.InitialSize);
		}
	}
}
#endif