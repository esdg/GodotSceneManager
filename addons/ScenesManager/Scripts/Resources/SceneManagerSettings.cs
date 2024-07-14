using Godot;

namespace MoF.Addons.ScenesManager.Scripts.Resources
{

    [Tool, GlobalClass]
    public partial class SceneManagerSettings : Resource
    {
        [Export] public string SceneManagerSchemaPath { get; set; }
    }
}