using System;
using System.Linq;
using Godot;
using MoF.Addons.ScenesManager.Constants;
using MoF.Addons.ScenesManager.Helpers;
using MoF.Addons.ScenesManager.Scripts.Resources;
using static Godot.PopupMenu;
using static MoF.Addons.ScenesManager.ScenesManagerBaseGraphNode;

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

		private static GraphNodeReadyEventHandler graphNodeReadyEventHandler;

		private string saveFilePath = "";

		public override void _Ready()
		{
			LoadResources();
			InitializeNodes();
			SetupEventHandlers();
			CreateTopMenuBar();
		}

		private void LoadResources()
		{
			trashCanIconTexture = ResourceLoader.Load<Texture2D>("res://addons/ScenesManager/Assets/Icons/trashcan.svg");
		}

		private void InitializeNodes()
		{
			graphEdit = GetNode<GraphEdit>("%GraphEdit");
			mainContextualMenuBar = GetNode<MenuBar>("%ContextualMenuBar");
			mainContextualMenuBar.Visible = false;
			CreateInitialStartAppNode();
		}

		private void SetupEventHandlers()
		{
			graphEdit.NodeSelected += OnNodeSelected;
			graphEdit.NodeDeselected += OnNodeDeselected;
		}

		private void CreateTopMenuBar()
		{
			MenuBar mainMenuBar = GetNode<MenuBar>("%MenuBar");
			MenuHelpers.CreateGraphMenu(mainMenuBar, OnGraphMenuItemPressed);
			MenuHelpers.CreateNodesMenu(mainMenuBar, OnNodesSubMenuItemPressed);
		}

		private void NewGraph()
		{
			ClearGraphNodes();
			saveFilePath = "";
			CreateInitialStartAppNode();
		}

		private void SaveSchema(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				SaveSchemaAs();
				return;
			}

			sceneManagerSchema.Items.Clear();
			foreach (var node in graphEdit.GetChildren().OfType<ScenesManagerBaseGraphNode>())
			{
				AddNodeToSchema(node);
			}

			ResourceSaver.Save(sceneManagerSchema, path);
			GodotHelpers.SaveSceneManagerSettings(path);
		}

		private void AddNodeToSchema(ScenesManagerBaseGraphNode node)
		{
			if (node is SceneGraphNode sceneGraphNode)
			{
				SceneManagerItem sceneManagerItem = new()
				{
					Scene = sceneGraphNode.Scene,
					Position = sceneGraphNode.PositionOffset,
					Name = sceneGraphNode.GraphNodeName,
				};
				SetSceneManagerItemForSchema(sceneGraphNode, sceneManagerItem);
			}
			else if (node is StartAppGraphNode startAppGraphNode)
			{
				StartAppSceneManagerItem startAppSceneManagerItem = new()
				{
					Name = startAppGraphNode.Name,
				};
				SetSceneManagerItemForSchema(startAppGraphNode, startAppSceneManagerItem);
			}
		}

		private void SetSceneManagerItemForSchema(ScenesManagerBaseGraphNode node, SceneManagerBaseItem sceneManagerBaseItem)
		{
			var connections = graphEdit.GetConnectionList().Where(o => (string)o["from_node"] == node.Name);
			foreach (var connection in connections)
			{
				var fromNodeInstance = graphEdit.GetNode<ScenesManagerBaseGraphNode>((string)connection["from_node"]);
				var toNodeInstance = graphEdit.GetNode<ScenesManagerBaseGraphNode>((string)connection["to_node"]);
				SceneManagerOutSlotSignal sceneManagerOutSlotSignal = new()
				{
					OutSlotSignalName = fromNodeInstance.OutSignalsNames[(int)connection["from_port"]],
				};
				if (toNodeInstance is SceneGraphNode toSceneGraphNode)
				{
					sceneManagerOutSlotSignal.TargetScene.PackedScene = toSceneGraphNode.Scene;
					sceneManagerOutSlotSignal.TargetScene.graphNodeName = toSceneGraphNode.GraphNodeName;
				}
				sceneManagerBaseItem.OutSignals.Add(sceneManagerOutSlotSignal);
			}
			sceneManagerSchema.Items.Add(sceneManagerBaseItem);
		}

		private void OpenSchema(string path)
		{
			var loadedSchema = ResourceLoader.Load<SceneManagerSchema>(path);
			if (loadedSchema == null)
			{
				GD.PrintErr("Failed to load SceneManagerSchema from path: " + path);
				return;
			}

			CallDeferred(MethodName.ClearGraphNodes);
			CallDeferred(MethodName.LoadGraphNodesFromSchema, loadedSchema);
		}

		private void ClearGraphNodes()
		{
			graphEdit.ClearConnections();
			nodeCount = 0;
			foreach (Node child in graphEdit.GetChildren())
			{
				if (child is ScenesManagerBaseGraphNode graphNode)
				{
					graphNode.GraphNodeName += "_todelete";
					graphNode.CallDeferred(MethodName.QueueFree);
				}
			}
		}

		private void LoadGraphNodesFromSchema(SceneManagerSchema schema)
		{
			foreach (var item in schema.Items)
			{
				CallDeferred(MethodName.AddNodeFromSchemaItem, item, schema);
			}

		}

		private void AddNodeFromSchemaItem(SceneManagerBaseItem item, SceneManagerSchema schema)
		{
			ScenesManagerBaseGraphNode node = GodotHelpers.CreateGraphNodeFromItem(item);
			if (node == null) return;

			node.OutSignalsToLoad = item.OutSignals;
			node.PositionOffset = item.Position;
			graphNodeReadyEventHandler = () => NodeReady(node, graphNodeReadyEventHandler, schema);
			node.GraphNodeReady += graphNodeReadyEventHandler;
			graphEdit.AddChild(node);
			nodeCount++;
		}

		private void NodeReady(ScenesManagerBaseGraphNode node, GraphNodeReadyEventHandler e, SceneManagerSchema schema)
		{
			node.GraphNodeReady -= e;
			if (nodeCount == schema.Items.Count)
			{
				CallDeferred(MethodName.RestoreConnections, schema);
			}
		}

		private void RestoreConnections(SceneManagerSchema schema)
		{
			foreach (var item in schema.Items)
			{
				var node = graphEdit.GetChildren().OfType<ScenesManagerBaseGraphNode>().FirstOrDefault(o => o.GraphNodeName == item.Name);

				if (node == null) return;

				foreach (SceneManagerOutSlotSignal signal in item.OutSignals)
				{
					var index = node.OutSignalsNames.IndexOf(signal.OutSlotSignalName);
					var targetNode = graphEdit.GetChildren().OfType<ScenesManagerBaseGraphNode>().FirstOrDefault(o => o.GraphNodeName == signal.TargetScene.graphNodeName);
					if (signal?.TargetScene?.graphNodeName != null && node.OutSignalsNames.Count > 0 && index >= 0)
					{
						var fromPort = index;
						var toPort = 0;
						graphEdit.ConnectNode(node.Name, fromPort, targetNode.Name, toPort);
					}
				}
			}
		}

		private void OpenSchema()
		{
			GodotHelpers.CreateFileDialog(FileDialog.FileModeEnum.OpenFile, "Open scene manager schema", OnDialogFileSelected, this);
		}

		private void SaveSchemaAs()
		{
			GodotHelpers.CreateFileDialog(FileDialog.FileModeEnum.SaveFile, "Save scene manager schema As", OnDialogFileSelected, this);
		}

		private void CreateInitialStartAppNode()
		{
			var node = new StartAppGraphNode();
			node.PositionOffset += new Vector2(100, 100);
			graphEdit.AddChild(node);
			node.GraphNodeName = node.Name;
		}

		private void CreateSceneNode()
		{
			var node = new SceneGraphNode();
			node.PositionOffset += new Vector2(40, 40) + (nodeCount * new Vector2(30, 30));
			graphEdit.AddChild(node);
			node.GraphNodeName = node.Name;
			nodeCount++;
		}

		private void CreateTransitionNode()
		{
			// Implement Create Transition Node logic here
		}

		private void OnGraphMenuItemPressed(long index)
		{
			switch (index)
			{
				case 0:
					NewGraph();
					break;
				case 1:
					OpenSchema();
					break;
				case 3:
					SaveSchema(saveFilePath);
					break;
				case 4:
					SaveSchemaAs();
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

		private void OnDialogFileSelected(FileDialog sender, string path, FileDialog.FileModeEnum fileMode)
		{
			saveFilePath = path;
			switch (fileMode)
			{
				case FileDialog.FileModeEnum.SaveFile:
					SaveSchema(path);
					break;
				case FileDialog.FileModeEnum.OpenFile:
					OpenSchema(path);
					break;
			}
			sender.QueueFree();
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

	internal class OnGraphMenuItemPressed
	{
	}

}
