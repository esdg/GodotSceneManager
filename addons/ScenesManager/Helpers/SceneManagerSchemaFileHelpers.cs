using System.Linq;
using Godot;
using MoF.Addons.ScenesManager.Scripts.Editor;
using MoF.Addons.ScenesManager.Scripts.Resources;
using static MoF.Addons.ScenesManager.Scripts.Editor.ScenesManagerBaseGraphNode;

namespace MoF.Addons.ScenesManager.Helpers
{
	/// <summary>
	/// Provides helper methods for loading and saving scene manager schema files.
	/// </summary>
	public static class SceneManagerSchemaFileHelpers
	{
		private static int nodeCount = 0;
		private static GraphNodeReadyEventHandler graphNodeReadyEventHandler;

		#region Load
		/// <summary>
		/// Loads graph nodes from the specified schema into the given graph edit.
		/// </summary>
		/// <param name="graphEdit">The graph edit to which nodes will be added.</param>
		/// <param name="schema">The schema from which nodes will be loaded.</param>
		/// <returns>The number of nodes loaded.</returns>
		public static int LoadGraphNodesFromSchema(GraphEdit graphEdit, SceneManagerSchema schema)
		{
			nodeCount = 0;
			foreach (var item in schema.Items)
			{
				AddNodeFromSchemaItem(graphEdit, item, schema);
			}
			return nodeCount;
		}

		/// <summary>
		/// Adds a node to the graph edit from the specified schema item.
		/// </summary>
		/// <param name="graphEdit">The graph edit to which the node will be added.</param>
		/// <param name="item">The schema item from which the node will be created.</param>
		/// <param name="schema">The schema containing the item.</param>
		private static void AddNodeFromSchemaItem(GraphEdit graphEdit, SceneManagerBaseItem item, SceneManagerSchema schema)
		{
			ScenesManagerBaseGraphNode node = CreateGraphNodeFromItem(graphEdit, item);
			if (node == null) return;

			node.OutSignalsToLoad = item.OutSignals;
			node.PositionOffset = item.Position;
			graphNodeReadyEventHandler = () => NodeReady(graphEdit, node, graphNodeReadyEventHandler, schema);
			node.GraphNodeReady += graphNodeReadyEventHandler;
			graphEdit.AddChild(node);
		}

		/// <summary>
		/// Creates a graph node from the specified schema item.
		/// </summary>
		/// <param name="graphEdit">The graph edit to which the node will be added.</param>
		/// <param name="item">The schema item from which the node will be created.</param>
		/// <returns>The created <see cref="ScenesManagerBaseGraphNode"/>.</returns>
		public static ScenesManagerBaseGraphNode CreateGraphNodeFromItem(GraphEdit graphEdit, SceneManagerBaseItem item)
		{
			ScenesManagerBaseGraphNode node = null;
			if (item is SceneManagerItem sceneManagerItem)
			{
				node = new SceneGraphNode { Scene = sceneManagerItem.Scene, GraphNodeName = sceneManagerItem.Name };
			}
			else if (item is StartAppSceneManagerItem startAppSceneManagerItem)
			{
				node = new StartAppGraphNode { GraphNodeName = startAppSceneManagerItem.Name };
			}
			else if (item is QuitAppSceneManagerItem quitAppSceneManagerItem)
			{
				node = new QuitAppGraphNode { GraphNodeName = quitAppSceneManagerItem.Name };
				var nodeMenu = graphEdit.GetParent().GetParent<ScenesManagerEditor>().MainMenuBar.GetChildren().OfType<PopupMenu>().FirstOrDefault(o => o.Name == "Node menu");
				nodeMenu.SetItemChecked(2, true);
			}
			else
			{
				GD.PrintErr("[SceneManagerEditor] Unknown SceneManagerItem type: " + item.GetType().Name);
			}
			return node;
		}

		/// <summary>
		/// Handles the event when a graph node is ready.
		/// </summary>
		/// <param name="graphEdit">The graph edit containing the node.</param>
		/// <param name="node">The node that is ready.</param>
		/// <param name="e">The event handler for the node ready event.</param>
		/// <param name="schema">The schema containing the node.</param>
		private static void NodeReady(GraphEdit graphEdit, ScenesManagerBaseGraphNode node, GraphNodeReadyEventHandler e, SceneManagerSchema schema)
		{
			nodeCount++;
			node.GraphNodeReady -= e;
			if (nodeCount == schema.Items.Count)
			{
				RestoreConnections(graphEdit, schema);
			}
		}

		/// <summary>
		/// Restores connections between nodes in the graph edit from the specified schema.
		/// </summary>
		/// <param name="graphEdit">The graph edit to which connections will be restored.</param>
		/// <param name="schema">The schema containing the connections.</param>
		private static void RestoreConnections(GraphEdit graphEdit, SceneManagerSchema schema)
		{
			foreach (var item in schema.Items)
			{
				var node = graphEdit.GetChildren().OfType<ScenesManagerBaseGraphNode>().FirstOrDefault(o => o.GraphNodeName == item.Name);

				if (node == null) return;

				foreach (SceneManagerOutSlotSignal signal in item.OutSignals)
				{
					var index = signal.Index;
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
		#endregion

		#region Save
		/// <summary>
		/// Prepares a schema for saving from the specified graph edit.
		/// </summary>
		/// <param name="graphEdit">The graph edit from which the schema will be prepared.</param>
		/// <returns>The prepared <see cref="SceneManagerSchema"/>.</returns>
		public static SceneManagerSchema PrepareSchemaForSave(GraphEdit graphEdit)
		{
			var schema = new SceneManagerSchema();

			foreach (var node in graphEdit.GetChildren().OfType<ScenesManagerBaseGraphNode>())
			{
				AddNodeToSchema(node, graphEdit, schema);
			}

			return schema;
		}

		/// <summary>
		/// Adds a node to the schema from the specified graph edit.
		/// </summary>
		/// <param name="node">The node to be added to the schema.</param>
		/// <param name="graphEdit">The graph edit containing the node.</param>
		/// <param name="schema">The schema to which the node will be added.</param>
		private static void AddNodeToSchema(ScenesManagerBaseGraphNode node, GraphEdit graphEdit, SceneManagerSchema schema)
		{
			switch (node)
			{
				case SceneGraphNode sceneGraphNode:
					SceneManagerItem sceneManagerItem = new()
					{
						Scene = sceneGraphNode.Scene,
						Position = sceneGraphNode.PositionOffset,
						Name = sceneGraphNode.GraphNodeName,
					};
					SetSceneManagerItemForSchema(sceneGraphNode, sceneManagerItem, graphEdit, schema);
					break;
				case StartAppGraphNode startAppGraphNode:
					StartAppSceneManagerItem startAppSceneManagerItem = new()
					{
						Name = startAppGraphNode.GraphNodeName,
						Position = startAppGraphNode.PositionOffset,
					};
					SetSceneManagerItemForSchema(startAppGraphNode, startAppSceneManagerItem, graphEdit, schema);
					break;
				case QuitAppGraphNode quitAppGraphNode:
					QuitAppSceneManagerItem quitAppSceneManagerItem = new()
					{
						Name = quitAppGraphNode.GraphNodeName,
						Position = quitAppGraphNode.PositionOffset,
					};
					SetSceneManagerItemForSchema(quitAppGraphNode, quitAppSceneManagerItem, graphEdit, schema);
					break;
			}
		}

		/// <summary>
		/// Sets the scene manager item for the schema from the specified node.
		/// </summary>
		/// <param name="node">The node from which the scene manager item will be set.</param>
		/// <param name="sceneManagerBaseItem">The scene manager base item to be set.</param>
		/// <param name="graphEdit">The graph edit containing the node.</param>
		/// <param name="schema">The schema to which the item will be added.</param>
		private static void SetSceneManagerItemForSchema(ScenesManagerBaseGraphNode node, SceneManagerBaseItem sceneManagerBaseItem, GraphEdit graphEdit, SceneManagerSchema schema)
		{
			var connections = graphEdit.GetConnectionList().Where(o => (string)o["from_node"] == node.Name);

			foreach (var connection in connections)
			{
				var fromNodeInstance = graphEdit.GetNode<ScenesManagerBaseGraphNode>((string)connection["from_node"]);
				var toNodeInstance = graphEdit.GetNode<ScenesManagerBaseGraphNode>((string)connection["to_node"]);

				var sceneManagerOutSlotSignal = new SceneManagerOutSlotSignal
				{
					OutSlotSignalName = fromNodeInstance.OutSignalsNames[(int)connection["from_port"]]
				};

				if (toNodeInstance is SceneGraphNode toSceneGraphNode)
				{
					sceneManagerOutSlotSignal.TargetScene.PackedScene = toSceneGraphNode.Scene;
					if (fromNodeInstance is SceneGraphNode sceneGraphNode)
					{
						sceneManagerOutSlotSignal.TransitionFileName = sceneGraphNode.OutTransitionPackedScenePaths[(int)connection["from_port"]];
					}

					sceneManagerOutSlotSignal.TargetSceneType = Enums.TargetSceneType.SceneGraphNode;
				}

				if (toNodeInstance is QuitAppGraphNode)
				{
					sceneManagerOutSlotSignal.TargetSceneType = Enums.TargetSceneType.QuitGraphNode;
				}

				if (toNodeInstance is not StartAppGraphNode)
				{
					sceneManagerOutSlotSignal.TargetScene.graphNodeName = toNodeInstance.GraphNodeName;
				}

				sceneManagerOutSlotSignal.Index = sceneManagerBaseItem.OutSignals.Count;

				sceneManagerBaseItem.OutSignals.Add(sceneManagerOutSlotSignal);
			}

			schema.Items.Add(sceneManagerBaseItem);
		}
		#endregion
	}
}
