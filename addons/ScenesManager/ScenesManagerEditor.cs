#if TOOLS
using System.Linq;
using Godot;
using Godot.Collections;
using MoF.Addons.ScenesManager.Helpers;
using MoF.Addons.ScenesManager.Scripts.Editor;
using MoF.Addons.ScenesManager.Scripts.Resources;


namespace MoF.Addons.ScenesManager
{
	[Tool]
	public partial class ScenesManagerEditor : Control
	{
		private GraphEdit graphEdit;
		private int nodeCount = 0;
		public MenuBar MainMenuBar { get; set; }
		private MenuBar mainContextualMenuBar;
		private SceneManagerSchema currentSceneManagerSchema = new();

		private ScenesManagerBaseGraphNode selectedNode;
		private Array<SceneGraphNode> selectedNodes = new();

		private string saveFilePath = "";

		public override void _Ready()
		{
			InitializeNodes();
			SetupEventHandlers();
			CreateTopMenuBar();
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
			MainMenuBar = GetNode<MenuBar>("%MenuBar");
			MenuHelpers.CreateGraphMenu(MainMenuBar, OnGraphMenuItemPressed);
			MenuHelpers.CreateNodesMenu(MainMenuBar, OnNodesSubMenuItemPressed);
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

			FileSystemHelper.SaveAndCreateFolder(currentSceneManagerSchema, path);
			GodotHelpers.SaveSceneManagerSettings(path);
		}

		private void SaveSchemaAs()
		{
			GodotHelpers.CreateFileDialog(FileDialog.FileModeEnum.SaveFile, "Save scene manager schema As", OnDialogFileSelected, this);
		}

		private void OpenSchema(string path)
		{
			var loadedSchema = ResourceLoader.Load<SceneManagerSchema>(path, null, ResourceLoader.CacheMode.Ignore);
			if (loadedSchema == null)
			{
				GD.PrintErr("[SceneManagerEditor] Failed to load SceneManagerSchema from path: " + path);
				return;
			}

			ClearGraphNodes();
			nodeCount = SceneManagerSchemaFileHelpers.LoadGraphNodesFromSchema(graphEdit, loadedSchema);
			GodotHelpers.SaveSceneManagerSettings(path);
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
				case 2:
					var nodeMenu = MainMenuBar.GetChildren().OfType<PopupMenu>().FirstOrDefault(o => o.Name == "Node menu");
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
			if (node is SceneGraphNode)
			{
				selectedNode = node as ScenesManagerBaseGraphNode;
				selectedNodes.Add(node as SceneGraphNode);
				if (selectedNodes.Count == 1)
					mainContextualMenuBar.Visible = true;
				else
					mainContextualMenuBar.Visible = false;
			}

		}

		private void OnNodeDeselected(Node node)
		{
			selectedNode = null;
			selectedNodes.Remove(node as SceneGraphNode);
			if (selectedNodes.Count == 1)
				mainContextualMenuBar.Visible = true;
			else
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
			var connectionList = graphEdit.GetConnectionList().Where(o => (StringName)o["from_node"] == selectedNode.Name || (StringName)o["to_node"] == selectedNode.Name);

			foreach (var connection in connectionList)
			{
				graphEdit.DisconnectNode((StringName)connection["from_node"], (int)connection["from_port"], (StringName)connection["to_node"], (int)connection["to_port"]);
			}

			graphEdit.RemoveChild(selectedNode);
			selectedNode.QueueFree();
			selectedNode = null;
			mainContextualMenuBar.Visible = false;
		}
	}
}
#endif
