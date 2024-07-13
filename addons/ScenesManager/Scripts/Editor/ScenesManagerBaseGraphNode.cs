using Godot;
using Godot.Collections;
using MoF.Addons.ScenesManager.Scripts.Resources;

namespace MoF.Addons.ScenesManager
{
    public abstract partial class ScenesManagerBaseGraphNode : GraphNode
    {
        [Signal] public delegate void GraphNodeReadyEventHandler();
        public abstract Array<string> OutSignals { get; }

        public Array<SceneManagerOutSlotSignal> OutSignalsToLoad { get; set; } = new();

        public sealed override void _Ready()
        {
            CallDeferred(MethodName._LoadResources);
            CallDeferred(MethodName._SetupGraphNode);
            CallDeferred(MethodName._ReadyNode);
        }

        public abstract void _ReadyNode();

        public abstract void _LoadResources();

        public abstract void _SetupGraphNode();
    }
}