using System.Linq;
using Godot;
using MoF.Addons.ScenesManager.Helpers;
using MoF.Addons.ScenesManager.Scripts.Resources;

namespace MoF.Addons.ScenesManager.Scripts.Editor
{
	[Tool]
	public partial class ScenesManagerEditor : Control
	{
		private GraphEdit graphEdit;
		private int nodeCount = 0;
		private Texture2D trashCanIconTexture;
		public MenuBar mainMenuBar { get; set; }
		private MenuBar mainContextualMenuBar;
		private GraphNode selectedNode;
		private SceneManagerSchema currentSceneManagerSchema = new();

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
			mainMenuBar = GetNode<MenuBar>("%MenuBar");
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

			currentSceneManagerSchema = SceneManagerSchemaFileHelpers.PrepareSchemaForSave(graphEdit);

			ResourceSaver.Save(currentSceneManagerSchema, path);
			GodotHelpers.SaveSceneManagerSettings(path);
		}

		private void SaveSchemaAs()
		{
			GodotHelpers.CreateFileDialog(FileDialog.FileModeEnum.SaveFile, "Save scene manager schema As", OnDialogFileSelected, this);
		}

		private void OpenSchema(string path)
		{
			var loadedSchema = ResourceLoader.Load<SceneManagerSchema>(path);
			if (loadedSchema == null)
			{
				GD.PrintErr("[SceneManagerEditor] Failed to load SceneManagerSchema from path: " + path);
				return;
			}

			ClearGraphNodes();
			nodeCount = SceneManagerSchemaFileHelpers.LoadGraphNodesFromSchema(graphEdit, loadedSchema);
		}

		private void ClearGraphNodes()
		{
			graphEdit.ClearConnections();
			nodeCount = 0;
			foreach (Node child in graphEdit.GetChildren())
			{
				if (child is ScenesManagerBaseGraphNode graphNode)
				{
					graphNode.Name += "_is_freeing";
					graphNode.GraphNodeName += "_is_freeing";
					graphNode.CallDeferred(MethodName.QueueFree);
				}
			}
		}

		private void OpenSchema()
		{
			GodotHelpers.CreateFileDialog(FileDialog.FileModeEnum.OpenFile, "Open scene manager schema", OnDialogFileSelected, this);
		}

		private void CreateInitialStartAppNode()
		{
			StartAppGraphNode node = new();
			node.PositionOffset += new Vector2(100, 100);
			graphEdit.AddChild(node);
			node.GraphNodeName = node.Name;
		}

		private void CreateSceneNode()
		{
			SceneGraphNode node = new();
			node.PositionOffset += new Vector2(40, 40) + (nodeCount * new Vector2(30, 30));
			graphEdit.AddChild(node);
			node.GraphNodeName = node.Name;
			nodeCount++;
		}

		private void CreateQuitAppNode()
		{
			QuitAppGraphNode node = new();
			node.PositionOffset += new Vector2(600, 100);
			graphEdit.AddChild(node);
			node.GraphNodeName = node.Name;
		}
		private void RemoveQuitAppNode()
		{
			graphEdit.GetChildren().OfType<QuitAppGraphNode>().First().QueueFree();
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
				case 2:
					var nodeMenu = mainMenuBar.GetChildren().OfType<PopupMenu>().FirstOrDefault(o => o.Name == "Node menu");
					if (!graphEdit.GetChildren().OfType<QuitAppGraphNode>().Any())
					{
						nodeMenu.SetItemChecked((int)index, true);
						CreateQuitAppNode();
					}
					else
					{
						nodeMenu.SetItemChecked((int)index, false);
						RemoveQuitAppNode();
					}
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
			selectedNode = node as ScenesManagerBaseGraphNode;
			if (selectedNode is not StartAppGraphNode && selectedNode is not QuitAppGraphNode)
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
}
