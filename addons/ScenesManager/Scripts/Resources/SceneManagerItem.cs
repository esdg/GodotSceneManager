using Godot;

namespace MoF.Addons.ScenesManager.Scripts.Resources
{
    [Tool, GlobalClass]
    public partial class SceneManagerItem : SceneManagerBaseItem
    {
        [Export] public PackedScene Scene { get; set; }

    }

}