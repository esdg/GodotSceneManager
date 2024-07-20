using Godot;
using MoF.Addons.ScenesManager.Enums;

namespace MoF.Addons.ScenesManager.Scripts.Resources
{
    [Tool, GlobalClass]
    public partial class SceneManagerOutSlotSignal : Resource
    {
        [Export] public string OutSlotSignalName { get; set; }
        [Export] public TargetScene TargetScene { get; set; } = new();
        [Export] public TargetSceneType TargetSceneType { get; set; } = new();
        [Export] public string TransitionFileName { get; set; }
    }
}