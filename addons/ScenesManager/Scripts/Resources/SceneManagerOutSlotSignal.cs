using Godot;

namespace MoF.Addons.ScenesManager.Scripts.Resources
{
    [Tool, GlobalClass]
    public partial class SceneManagerOutSlotSignal : Resource
    {
        [Export] public string OutSlotSignalName { get; set; }
        [Export] public TargetScene TargetScene { get; set; } = new();
    }
}