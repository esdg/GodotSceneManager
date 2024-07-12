using Godot;
using Godot.Collections;

namespace MoF.Addons.ScenesManager.Scripts.Resources
{
    [Tool, GlobalClass]
    public partial class SceneManagerSchema : Resource
    {
        [Export] public Array<SceneManagerBaseItem> Items { get; set; } = new();
    }
}