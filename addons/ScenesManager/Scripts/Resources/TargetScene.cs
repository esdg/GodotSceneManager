using Godot;

namespace MoF.Addons.ScenesManager.Scripts.Resources
{
    [Tool]
    public partial class TargetScene : Resource
    {
        [Export] public PackedScene PackedScene;
        [Export] public string graphNodeName;
    }
}