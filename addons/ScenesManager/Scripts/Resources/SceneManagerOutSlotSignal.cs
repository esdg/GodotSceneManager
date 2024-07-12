using System.IO;
using Godot;
using Godot.Collections;

namespace MoF.Addons.ScenesManager.Scripts.Resources
{
    [Tool, GlobalClass]
    public partial class SceneManagerOutSlotSignal : Resource
    {
        [Export] public string OutSlotSignalName { get; set; }
        [Export] public PackedScene TargetPackedScene { get; set; }
    }
}