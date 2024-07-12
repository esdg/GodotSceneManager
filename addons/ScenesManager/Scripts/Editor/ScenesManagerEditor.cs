using System;
using System.Linq;
using Godot;
using MoF.Addons.ScenesManager.Constants;
using MoF.Addons.ScenesManager.Helpers;
using MoF.Addons.ScenesManager.Scripts.Resources;

namespace MoF.Addons.ScenesManager
{
	[Tool]
	public partial class ScenesManagerEditor : Control
	{
		private GraphEdit graphEdit;
		private int nodeCount = 0;
		private Texture2D trashCanIconTexture;
		private MenuBar mainContextualMenuBar;
		private GraphNode selectedNode;
		private SceneManagerSchema sceneManagerSchema = new();

		private string saveFilePath = "";

		public override void _Ready()
		{
			LoadResources();
			InitializeNodes();
			SetupEventHandlers();
			CreateTopMenuBar();
		}

		private void InitializeNodes()
		{
			graphEdit = GetNode<GraphEdit>("%GraphEdit");
			mainContextualMenuBar = GetNode<MenuBar>("%ContextualMenuBar");
			mainContextualMenuBar.Visible = false;
			var node = new StartAppGraphNode();
			node.PositionOffset += new Vector2(100, 100);
			graphEdit.AddChild(node);
		}

		private void LoadResources()
		{
			trashCanIconTexture = ResourceLoader.Load<Texture2D>("res://addons/ScenesManager/Assets/Icons/trashcan.svg");
		}

		private void SetupEventHandlers()
		{
			graphEdit.NodeSelected += OnNodeSelected;
			graphEdit.NodeDeselected += OnNodeDeselected;
		}

		private void OnNodeSelected(Node node)
		{
			selectedNode = node as GraphNode;
			mainContextualMenuBar.Visible = true;
		}

		private void OnNodeDeselected(Node node)
		{
			selectedNode = null;
			mainContextualMenuBar.Visible = false;
		}

		private void CreateTopMenuBar()
		{
			MenuBar mainMenuBar = GetNode<MenuBar>("%MenuBar");

			PopupMenu menuGraph = GodotHelpers.CreatePopupMenu("Graph", new string[] {
				"New Graph",
				"Open Graph...",
				AddonConstants.PopupMenuSeparator,
				"Save Graph",
				"Save Graph As..."
			}, OnGraphMenuItemPressed);
			mainMenuBar.AddChild(menuGraph);

			PopupMenu menuItemNodes = GodotHelpers.CreatePopupMenu("Nodes", Array.Empty<string>());
			PopupMenu nodesAddSubMenuItem = GodotHelpers.CreatePopupMenu("NodesAddSubMenu", new string[] {
				"Add Scene Node",
				"Add Transition Node"
			}, OnNodesSubMenuItemPressed);
			menuItemNodes.AddSubmenuItem("Add", nodesAddSubMenuItem.Name);
			menuItemNodes.AddChild(nodesAddSubMenuItem);
			mainMenuBar.AddChild(menuItemNodes);
		}

		private void OnGraphMenuItemPressed(long index)
		{
			switch (index)
			{
				case 0:
					NewGraph();
					break;
				case 1:
					OpenGraph();
					break;
				case 3:
					Save(saveFilePath);
					break;
				case 4:
					SaveAs();
					break;
			}
		}

		private void OnNodesSubMenuItemPressed(long index)
		{
			switch (index)
			{
				case 0:
					CreateSceneNode();
					break;
				case 1:
					CreateTransitionNode();
					break;
			}
		}

		private void NewGraph()
		{
			// Implement New Graph logic here
		}

		private void OpenGraph()
		{
			// Implement Open Graph logic here
		}

		private void Save(string path)
		{
			if (path == "")
			{
				SaveAs();
				return;
			}
			// TODO : delete file if exist before saving
			sceneManagerSchema.Items.Clear();
			foreach (var node in graphEdit.GetChildren().OfType<ScenesManagerBaseGraphNode>())
			{
				if (node is SceneGraphNode sceneGraphNode)
				{
					SceneManagerItem sceneManagerItem = new()
					{
						Scene = sceneGraphNode.Scene
					};
					SetSceneManagerItem(sceneGraphNode, sceneManagerItem);
				}
				if (node is StartAppGraphNode startAppGraphNode)
				{
					StartAppSceneManagerItem startAppSceneManagerItem = new();
					SetSceneManagerItem(startAppGraphNode, startAppSceneManagerItem);
				}
			}
			ResourceSaver.Save(sceneManagerSchema, path);

			// save settings with sceneManagerSchema file path so it can be loaded by the singleton.
			SceneManagerSettings sceneManagerSettings = new()
			{
				SceneManagerSchemaPath = path,
			};
			ResourceSaver.Save(sceneManagerSettings, AddonConstants.SettingsFilePath);
		}

		private void SetSceneManagerItem(ScenesManagerBaseGraphNode node, SceneManagerBaseItem sceneManagerBaseItem)
		{
			var connections = graphEdit.GetConnectionList().Where(o => (string)o["from_node"] == node.Name);
			foreach (var connection in connections)
			{
				var fromNodeInstance = graphEdit.GetNode<ScenesManagerBaseGraphNode>((string)connection["from_node"]);
				var toNodeInstance = graphEdit.GetNode<ScenesManagerBaseGraphNode>((string)connection["to_node"]);
				SceneManagerOutSlotSignal sceneManagerOutSlotSignal = new()
				{
					OutSlotSignalName = fromNodeInstance.OutSignals[(int)connection["from_port"]],
				};
				if (toNodeInstance is SceneGraphNode toSceneGraphNode)
				{
					sceneManagerOutSlotSignal.TargetPackedScene = toSceneGraphNode.Scene;
				}
				sceneManagerBaseItem.OutSignals.Add(sceneManagerOutSlotSignal);
			}
			sceneManagerSchema.Items.Add(sceneManagerBaseItem);
		}

		private void SaveAs()
		{
			FileDialog fileDialog = new()
			{
				Filters = new string[] { "*.tres" },
				DialogHideOnOk = true,
				Size = new Vector2I((int)GetViewport().GetVisibleRect().Size.X / 3, (int)GetViewport().GetVisibleRect().Size.Y / 3),
				InitialPosition = Window.WindowInitialPosition.CenterPrimaryScreen,
			};
			AddChild(fileDialog);
			fileDialog.Popup();
			fileDialog.FileSelected += (string path) => OnSaveDialogFileSelected(path);
		}

		private void OnSaveDialogFileSelected(string path)
		{
			saveFilePath = path;
			Save(path);
		}

		private void CreateSceneNode()
		{
			var node = new SceneGraphNode();
			node.PositionOffset += new Vector2(40, 40) + (nodeCount * new Vector2(30, 30));
			graphEdit.AddChild(node);
			nodeCount++;
		}

		private void CreateTransitionNode()
		{
			// Implement Create Transition Node logic here
		}

		private void _on_graph_edit_connection_request(StringName from_node, long from_port, StringName to_node, long to_port)
		{
			var fromNode = graphEdit.GetConnectionList().FirstOrDefault(o => (string)o["from_node"] == from_node && (int)o["from_port"] == from_port);

			if (fromNode != null) // If node have connection, remove it before adding the new one, preventing multiple connections
				graphEdit.DisconnectNode((string)fromNode["from_node"], (int)fromNode["from_port"], (string)fromNode["to_node"], (int)fromNode["to_port"]);

			graphEdit.ConnectNode(from_node, (int)from_port, to_node, (int)to_port);
		}

		private void _on_graph_edit_disconnection_request(StringName from_node, long from_port, StringName to_node, long to_port)
		{
			graphEdit.DisconnectNode(from_node, (int)from_port, to_node, (int)to_port);
		}

		private void _on_delete_node_button_pressed()
		{
			graphEdit.RemoveChild(selectedNode);
			selectedNode.QueueFree();
			selectedNode = null;
			mainContextualMenuBar.Visible = false;
		}
	}
}
