using System;
using System.Linq;
using Godot;
using MoF.Addons.ScenesManager.Scripts.Resources;
using static MoF.Addons.ScenesManager.ScenesManagerBaseGraphNode;

namespace MoF.Addons.ScenesManager.Helpers
{
    public static class SceneManagerSchemaFileHelpers
    {
        private static int nodeCount = 0;
        private static GraphNodeReadyEventHandler graphNodeReadyEventHandler;

        #region Load
        public static int LoadGraphNodesFromSchema(GraphEdit graphEdit, SceneManagerSchema schema)
        {
            nodeCount = 0;
            foreach (var item in schema.Items)
            {
                AddNodeFromSchemaItem(graphEdit, item, schema);
            }
            return nodeCount;
        }
        private static void AddNodeFromSchemaItem(GraphEdit graphEdit, SceneManagerBaseItem item, SceneManagerSchema schema)
        {
            ScenesManagerBaseGraphNode node = CreateGraphNodeFromItem(item);
            if (node == null) return;

            node.OutSignalsToLoad = item.OutSignals;
            node.PositionOffset = item.Position;
            graphNodeReadyEventHandler = () => NodeReady(graphEdit, node, graphNodeReadyEventHandler, schema);
            node.GraphNodeReady += graphNodeReadyEventHandler;
            graphEdit.AddChild(node);
        }
        public static ScenesManagerBaseGraphNode CreateGraphNodeFromItem(SceneManagerBaseItem item)
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
            }
            else
            {
                GD.PrintErr("Unknown SceneManagerItem type: " + item.GetType().Name);
            }
            return node;
        }

        private static void NodeReady(GraphEdit graphEdit, ScenesManagerBaseGraphNode node, GraphNodeReadyEventHandler e, SceneManagerSchema schema)
        {
            nodeCount++;
            node.GraphNodeReady -= e;
            if (nodeCount == schema.Items.Count)
            {
                RestoreConnections(graphEdit, schema);
            }
        }

        private static void RestoreConnections(GraphEdit graphEdit, SceneManagerSchema schema)
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
        #endregion

        #region Save
        public static SceneManagerSchema PrepareSchemaForSave(GraphEdit graphEdit)
        {
            var schema = new SceneManagerSchema();

            foreach (var node in graphEdit.GetChildren().OfType<ScenesManagerBaseGraphNode>())
            {
                AddNodeToSchema(node, graphEdit, schema);
            }

            return schema;
        }

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
                        Name = startAppGraphNode.Name,
                        Position = startAppGraphNode.PositionOffset,
                    };
                    SetSceneManagerItemForSchema(startAppGraphNode, startAppSceneManagerItem, graphEdit, schema);
                    break;
                case QuitAppGraphNode quitAppGraphNode:
                    QuitAppSceneManagerItem quitAppSceneManagerItem = new()
                    {
                        Name = quitAppGraphNode.Name,
                        Position = quitAppGraphNode.PositionOffset,
                    };
                    SetSceneManagerItemForSchema(quitAppGraphNode, quitAppSceneManagerItem, graphEdit, schema);
                    break;
            }
        }

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
                    sceneManagerOutSlotSignal.TargetSceneType = 1;
                }

                if (toNodeInstance is QuitAppGraphNode)
                {
                    sceneManagerOutSlotSignal.TargetSceneType = 2;
                }

                if (toNodeInstance is not StartAppGraphNode)
                {
                    sceneManagerOutSlotSignal.TargetScene.graphNodeName = toNodeInstance.GraphNodeName;

                }

                sceneManagerBaseItem.OutSignals.Add(sceneManagerOutSlotSignal);
            }

            schema.Items.Add(sceneManagerBaseItem);
        }
        #endregion
    }
}
