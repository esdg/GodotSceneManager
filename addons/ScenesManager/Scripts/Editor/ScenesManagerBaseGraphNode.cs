using Godot;
using Godot.Collections;

namespace MoF.Addons.ScenesManager
{
    public abstract partial class ScenesManagerBaseGraphNode : GraphNode
    {

        public abstract Array<string> OutSignals { get; }

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