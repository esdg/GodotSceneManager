using System;
using Godot;

namespace MoF.Addons.ScenesManager.Scripts
{
    [Tool]
    public abstract partial class TransitionCanvasBase : Node
    {
        [Signal] public delegate void TransitionFinishedEventHandler(Node currentScene);
        protected Control _currentSceneRoot;
        protected Control _targetSceneRoot;

        protected Node _currentSceneNode;
        protected Node _targetSceneNode;


        protected PackedScene _targetPackedScene;

        public string TargetNodeName { get; set; }
        public virtual PackedScene TargetPackedScene
        {
            set
            {
                _targetPackedScene = value;
            }
        }

        public sealed override void _Ready()
        {
            _TransitionReady();
        }

        public virtual void _TransitionReady() { }

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
