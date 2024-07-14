using System.Linq;
using Godot;
using Godot.Collections;
using MoF.Addons.ScenesManager.Extensions;
using MoF.Addons.ScenesManager.Constants;

namespace MoF.Addons.ScenesManager
{
	[Tool, GlobalClass]
	public partial class SceneGraphNode : ScenesManagerBaseGraphNode
	{

		public PackedScene Scene
		{
			get => scene;
			set
			{
				scene = value;
			}
		}

		public override Array<string> OutSignalsNames
		{
			get
			{
				var signals = new Array<string>();
				foreach (Node node in outSlotNodes)
				{
					if (node.GetChildren()[1] is OptionButton selectBox)
					{
						signals.Add(selectBox.GetItemText(selectBox.Selected));
					}
				}
				return signals;
			}
		}


		private Node sceneRootNode;
		private Node inSlotNode;
		private Array<Node> outSlotNodes = new();
		private Button addOutSlotButton;
		private EditorResourcePicker sceneResourcePicker;
		private PackedScene scene;

		private static Texture2D signalIconTexture;
		private static Texture2D trashCanIconTexture;
		private static StyleBoxFlat sceneGraphNodeStylePanel;
		private static StyleBoxFlat sceneGraphNodeStyleTitlebar;

		public override void _LoadResources()
		{
			signalIconTexture ??= ResourceLoader.Load<Texture2D>("res://addons/ScenesManager/Assets/Icons/Signal.svg");
			trashCanIconTexture ??= ResourceLoader.Load<Texture2D>("res://addons/ScenesManager/Assets/Icons/trashcan.svg");
			sceneGraphNodeStylePanel ??= ResourceLoader.Load<StyleBoxFlat>(AddonConstants.GraphNode.SceneGraphNode.GraphNodeStylePanelPath);
			sceneGraphNodeStyleTitlebar ??= ResourceLoader.Load<StyleBoxFlat>(AddonConstants.GraphNode.SceneGraphNode.GraphNodeStyleTitlebarPath);
		}

		public override void _SetupGraphNode()
		{
			Title = AddonConstants.GraphNode.SceneGraphNode.Title;
			Size = AddonConstants.GraphNode.SceneGraphNode.InitialSize;
			Set("theme_override_constants/separation", AddonConstants.GraphNode.NodeVerticalSpace);
			Set("theme_override_styles/panel", sceneGraphNodeStylePanel);
			Set("theme_override_styles/titlebar", sceneGraphNodeStyleTitlebar);
		}

		public override void _ReadyNode()
		{
			CreateSceneResourcePicker();
		}

		private void CreateSceneResourcePicker()
		{
			sceneResourcePicker = new EditorResourcePicker
			{
				BaseType = nameof(PackedScene)
			};
			AddChild(sceneResourcePicker);

			if (Scene != null)
			{
				sceneResourcePicker.EditedResource = Scene;
				SetScene(Scene);
			}
			sceneResourcePicker.ResourceChanged += OnSceneResourcePickerChanged;

		}

		private void InitializeGraphNode()
		{
			if (inSlotNode != null)
			{
				RemoveChild(inSlotNode);
				inSlotNode.QueueFree();
			}

			if (addOutSlotButton != null)
			{
				RemoveChild(addOutSlotButton);
				addOutSlotButton.QueueFree();
			}

			this.RemoveChildren(outSlotNodes.ToArray());
			outSlotNodes.Clear();
			SetSize(new Vector2(250, 10));
		}

		private void SetScene(PackedScene packedScene)
		{
			sceneRootNode = packedScene.Instantiate<Node>();
			Title = packedScene.ResourcePath;//sceneRootNode.Name;
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
				CreateOutSlotNode(outSignals.OutSlotSignalName);
			}
		}

		private void CreateInSlotNode()
		{
			inSlotNode = new Label { Text = "In" };
			AddChild(inSlotNode);
			SetSlot(inSlotNode.GetIndex(), true, 0, AddonConstants.GraphNode.InSlotColor, false, 0, Colors.White);
		}

		private void CreateAddOutSlotButton()
		{
			addOutSlotButton = new Button
			{
				Text = GetAddOutSlotButtonText()
			};
			addOutSlotButton.Pressed += OnAddOutSlot;
			AddChild(addOutSlotButton);
		}

		private void CreateOutSlotNode(string signalName = "")
		{
			var outSignalNode = new HBoxContainer();
			var deleteButton = new Button
			{
				Icon = trashCanIconTexture
			};
			var signalsSelectBox = CreateSignalsSelectBox();

			if (signalName != "")
			{
				for (int i = 0; i < signalsSelectBox.ItemCount; i++)
				{
					if (signalsSelectBox.GetItemText(i) == signalName)
					{
						signalsSelectBox.Select(i);
					}
				}
			}

			deleteButton.Pressed += () => OnDeleteOutSignalNode(outSignalNode);

			outSignalNode.AddChild(deleteButton);
			outSignalNode.AddChild(signalsSelectBox);

			AddChild(outSignalNode);
			outSlotNodes.Add(outSignalNode);
			SetSlot(outSignalNode.GetIndex(), false, 0, Colors.White, true, 0, AddonConstants.GraphNode.OutSlotColor);
		}

		private OptionButton CreateSignalsSelectBox()
		{
			var optionButton = new OptionButton
			{
				TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis,
				FitToLongestItem = false,
				SizeFlagsHorizontal = SizeFlags.ExpandFill
			};
			foreach (Dictionary signal in sceneRootNode.GetSignalList())
			{
				optionButton.AddIconItem(signalIconTexture, (string)signal.Values.First());
			}
			return optionButton;
		}

		private void UpdateAddOutSlotButtonState()
		{
			addOutSlotButton.Text = GetAddOutSlotButtonText();
			addOutSlotButton.Disabled = outSlotNodes.Count >= AddonConstants.GraphNode.MaxNumberOfOutSlots;
		}

		private string GetAddOutSlotButtonText()
		{
			return $"Add Out slot {outSlotNodes.Count}/{AddonConstants.GraphNode.MaxNumberOfOutSlots}";
		}

		private void OnSceneResourcePickerChanged(Resource resource)
		{
			if (resource is PackedScene packedScene)
			{
				scene = packedScene;
				InitializeGraphNode();
				SetScene(packedScene);
			}
			else
			{
				InitializeGraphNode();
			}
		}

		private void OnAddOutSlot()
		{
			if (outSlotNodes.Count < AddonConstants.GraphNode.MaxNumberOfOutSlots)
			{
				CreateOutSlotNode();
			}
			UpdateAddOutSlotButtonState();
		}

		private void OnDeleteOutSignalNode(Node node)
		{
			RemoveChild(node);
			outSlotNodes.Remove(node);
			UpdateAddOutSlotButtonState();
			SetSize(new Vector2(250, 10));
		}
	}
}
