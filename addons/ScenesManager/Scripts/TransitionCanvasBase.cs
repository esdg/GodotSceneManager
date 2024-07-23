using Godot;

namespace MoF.Addons.ScenesManager.Scripts
{
    /// <summary>
    /// An abstract base class for transition canvas nodes in the scenes manager.
    /// </summary>
    [Tool]
    public abstract partial class TransitionCanvasBase : Node
    {
        /// <summary>
        /// Signal emitted when the transition is finished.
        /// </summary>
        /// <param name="currentScene">The current scene node.</param>
        [Signal]
        public delegate void TransitionFinishedEventHandler(Node currentScene);

        /// <summary>
        /// The root control node of the current scene.
        /// </summary>
        protected Control _currentSceneRoot;

        /// <summary>
        /// The root control node of the target scene.
        /// </summary>
        protected Control _targetSceneRoot;

        /// <summary>
        /// The node representing the current scene.
        /// </summary>
        protected Node _currentSceneNode;

        /// <summary>
        /// The node representing the target scene.
        /// </summary>
        protected Node _targetSceneNode;

        /// <summary>
        /// The packed scene resource for the target scene.
        /// </summary>
        protected PackedScene _targetPackedScene;

        /// <summary>
        /// Gets or sets the name of the target node.
        /// </summary>
        /// <value>
        /// A string representing the name of the target node.
        /// </value>
        public string TargetNodeName { get; set; }

        /// <summary>
        /// Gets or sets the packed scene resource for the target scene.
        /// </summary>
        /// <value>
        /// A <see cref="PackedScene"/> representing the target scene.
        /// </value>
        public virtual PackedScene TargetPackedScene
        {
            set
            {
                _targetPackedScene = value;
            }
        }

        /// <summary>
        /// Called when the node is added to the scene.
        /// </summary>
        public sealed override void _Ready()
        {
            _TransitionReady();
        }

        /// <summary>
        /// Virtual method to perform additional setup when the transition is ready.
        /// </summary>
        /// <remarks>
        /// This method can be overridden by derived classes to provide custom initialization logic for transitions.
        /// </remarks>
        public virtual void _TransitionReady() { }

        /// <summary>
        /// Sends the transition finished signal and performs cleanup of the current scene.
        /// </summary>
        protected virtual void SendTransitionFinishedSignal()
        {
            if (_currentSceneNode != null)
            {
                _currentSceneRoot?.RemoveChild(_currentSceneNode);
                _currentSceneNode.QueueFree();
            }
            _targetSceneRoot?.RemoveChild(_targetSceneNode);
            CallDeferred(MethodName.EmitSignal, SignalName.TransitionFinished, _targetSceneNode);
        }
    }
}
