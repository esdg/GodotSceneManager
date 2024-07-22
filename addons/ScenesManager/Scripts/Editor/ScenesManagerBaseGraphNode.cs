using Godot;
using Godot.Collections;
using MoF.Addons.ScenesManager.Scripts.Resources;

namespace MoF.Addons.ScenesManager.Scripts.Editor
{
    public abstract partial class ScenesManagerBaseGraphNode : GraphNode
    {
        [Signal] public delegate void GraphNodeReadyEventHandler();

        public abstract Array<string> OutSignalsNames { get; }

        public Array<SceneManagerOutSlotSignal> OutSignalsToLoad { get; set; } = new();

        public string GraphNodeName = "";

        public sealed override void _Ready()
        {
            CallDeferred(MethodName._LoadResources);
            CallDeferred(MethodName._SetupGraphNode);
            CallDeferred(MethodName._ReadyNode);
        }

        public virtual void _ReadyNode() { }

        public virtual void _LoadResources() { }

        public virtual void _SetupGraphNode() { }
    }
}