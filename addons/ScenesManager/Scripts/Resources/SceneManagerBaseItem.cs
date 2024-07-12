using System.IO;
using Godot;
using Godot.Collections;

namespace MoF.Addons.ScenesManager.Scripts.Resources
{
    [Tool, GlobalClass]
    public partial class SceneManagerBaseItem : Resource
    {
        [Export] public Array<SceneManagerOutSlotSignal> OutSignals { get; set; } = new();
    }

}