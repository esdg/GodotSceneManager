#if TOOLS
using Godot;
using Godot.Collections;
using MoF.Addons.ScenesManager.Scripts.Resources;

namespace MoF.Addons.ScenesManager.Scripts.Editor
{
    /// <summary>
    /// An abstract base class representing a node in the scenes manager graph.
    /// </summary>
    public abstract partial class ScenesManagerBaseGraphNode : GraphNode
    {
        /// <summary>
        /// Signal emitted when the graph node is ready.
        /// </summary>
        [Signal]
        public delegate void GraphNodeReadyEventHandler();

        /// <summary>
        /// Gets the names of the output signals of this node.
        /// </summary>
        /// <value>
        /// An array of strings representing the names of the output signals.
        /// </value>
        public abstract Array<string> OutSignalsNames { get; }

        /// <summary>
        /// Gets or sets the output signals to load for this node.
        /// </summary>
        /// <value>
        /// An array of <see cref="SceneManagerOutSlotSignal"/> representing the output signals to load.
        /// </value>
        public Array<SceneManagerOutSlotSignal> OutSignalsToLoad { get; set; } = new();

        /// <summary>
        /// The name of the graph node.
        /// </summary>
        /// <value>
        /// A string representing the name of the graph node.
        /// </value>
        public string GraphNodeName = "";

        /// <summary>
        /// Called when the node is added to the scene.
        /// </summary>
        public sealed override void _Ready()
        {
            CallDeferred(MethodName._LoadResources);
            CallDeferred(MethodName._SetupGraphNode);
            CallDeferred(MethodName._ReadyNode);
        }

        /// <summary>
        /// Virtual method to perform additional setup when the node is ready.
        /// </summary>
        /// <remarks>
        /// This method can be overridden by derived classes to provide custom initialization logic.
        /// </remarks>
        public virtual void _ReadyNode() { }

        /// <summary>
        /// Virtual method to load resources required by the node.
        /// </summary>
        /// <remarks>
        /// This method can be overridden by derived classes to load necessary resources.
        /// </remarks>
        public virtual void _LoadResources() { }

        /// <summary>
        /// Virtual method to setup the graph node.
        /// </summary>
        /// <remarks>
        /// This method can be overridden by derived classes to perform additional setup on the node.
        /// </remarks>
        public virtual void _SetupGraphNode() { }
    }
}
#endif
