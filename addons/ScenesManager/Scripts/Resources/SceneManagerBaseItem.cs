using Godot;
using Godot.Collections;

namespace MoF.Addons.ScenesManager.Scripts.Resources
{
    [Tool]
    public partial class SceneManagerBaseItem : Resource
    {
        [Export] public string Name { get; set; }
        [Export] public Array<SceneManagerOutSlotSignal> OutSignals { get; set; } = new();
        [Export] public Vector2 Position { get; set; } = new();
    }

}